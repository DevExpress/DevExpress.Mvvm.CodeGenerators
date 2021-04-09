using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    class CommandGenerator {
        readonly string type;
        readonly string name;
        readonly string parametersList;

        public static CommandGenerator Create(ContextInfo info, INamedTypeSymbol classSymbol, IMethodSymbol methodSymbol) {
            var hasError = false;
            var isCommand = methodSymbol.ReturnsVoid;
            var isAsyncCommand = methodSymbol.ReturnType.ToDisplayStringNullable().StartsWith("System.Threading.Tasks.Task");
            if(methodSymbol.Parameters.Length > 1 || !(isCommand || isAsyncCommand)) {
                info.Context.ReportIncorrectCommandSignature(methodSymbol);
                hasError = true;
            }

            var parameterType = methodSymbol.Parameters.FirstOrDefault()?.Type.ToDisplayStringNullable() ?? string.Empty;
            var canExecuteMethodName = CommandHelper.GetCanExecuteMethodName(methodSymbol, info.CommandAttributeSymbol);
            if(canExecuteMethodName == null) {
                var candidate = CommandHelper.GetCanExecuteMethodCandidates(classSymbol, "Can" + methodSymbol.Name, parameterType);
                canExecuteMethodName = candidate.SingleOrDefault()?.Name ?? "null";
            } else {
                var candidates = CommandHelper.GetCanExecuteMethodCandidates(classSymbol, canExecuteMethodName, parameterType);
                if(!candidates.Any()) {
                    info.Context.ReportCanExecuteMethodNotFound(methodSymbol, canExecuteMethodName, parameterType, CommandHelper.GetMethods(classSymbol, canExecuteMethodName));
                    hasError = true;
                }
            }
            return hasError ? null : new CommandGenerator(methodSymbol, info.CommandAttributeSymbol, parameterType, canExecuteMethodName, isCommand);
        }
        public string GetSourceCode() {
            var source = new StringBuilder();
            source.AppendLine($"{type} {name};");
            source.AppendLine($"public {type} {name.FirstToUpperCase()} {{");
            source.AppendLine($"get => {name} ??= new {type}({parametersList});".AddTabs(1));
            source.AppendLine("}");
            return source.ToString();
        }

        CommandGenerator(IMethodSymbol methodSymbol, INamedTypeSymbol commandAttributeSymbol, string parameterType, string canExecuteMethodName, bool isCommand) {
            var baseType = isCommand ? "DelegateCommand" : "AsyncCommand";
            var executeMethod = methodSymbol.Name;
            var useCommandManager = CommandHelper.GetUseCommandManagerValue(methodSymbol, commandAttributeSymbol).ToString().ToLower();
            var allowMultipleExecution = CommandHelper.GetAllowMultipleExecutionValue(methodSymbol, commandAttributeSymbol).ToString().ToLower();

            type = CommandHelper.GetGenericType(baseType, parameterType);
            name = CommandHelper.GetCommandName(methodSymbol, commandAttributeSymbol, executeMethod);
            parametersList = isCommand ?
                                CommandHelper.ParametersToDisplayString(executeMethod, canExecuteMethodName, useCommandManager) :
                                CommandHelper.ParametersToDisplayString(executeMethod, canExecuteMethodName, allowMultipleExecution, useCommandManager);
        }
    }
}
