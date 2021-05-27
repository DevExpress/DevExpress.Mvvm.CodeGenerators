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

            var parameterType = methodSymbol.Parameters.FirstOrDefault()?.Type;
            var canExecuteMethodName = CommandHelper.GetCanExecuteMethodName(methodSymbol, info.CommandAttributeSymbol);
            if(canExecuteMethodName == null) {
                var candidate = CommandHelper.GetCanExecuteMethodCandidates(classSymbol, "Can" + methodSymbol.Name, parameterType);
                canExecuteMethodName = candidate.FirstOrDefault()?.Name ?? "null";
            } else {
                var candidates = CommandHelper.GetCanExecuteMethodCandidates(classSymbol, canExecuteMethodName, parameterType);
                if(!candidates.Any()) {
                    info.Context.ReportCanExecuteMethodNotFound(methodSymbol, canExecuteMethodName, parameterType?.ToDisplayStringNullable() ?? string.Empty, CommandHelper.GetMethods(classSymbol, canExecuteMethodName));
                    hasError = true;
                }
            }
            return hasError ? null : new CommandGenerator(methodSymbol, info.CommandAttributeSymbol, parameterType?.ToDisplayStringNullable() ?? string.Empty, canExecuteMethodName, isCommand, info.IsWinUI);
        }
        public void GetSourceCode(StringBuilder source, int tabs) {
            source.AppendLine($"{type}? {name};".AddTabs(tabs));
            source.AppendLine($"public {type} {name.FirstToUpperCase()} {{".AddTabs(tabs));
            source.AppendLine($"get => {name} ??= new {type}({parametersList});".AddTabs(tabs + 1));
            source.AppendLine("}".AddTabs(tabs));
        }

        CommandGenerator(IMethodSymbol methodSymbol, INamedTypeSymbol commandAttributeSymbol, string parameterType, string canExecuteMethodName, bool isCommand, bool isWinUI) {
            var baseType = isCommand ? "DelegateCommand" : "AsyncCommand";
            var executeMethod = methodSymbol.Name;
            
            var allowMultipleExecution = CommandHelper.GetAllowMultipleExecutionValue(methodSymbol, commandAttributeSymbol).ToString().ToLower();

            type = CommandHelper.GetGenericType(baseType, parameterType);
            name = CommandHelper.GetCommandName(methodSymbol, commandAttributeSymbol, executeMethod);
            if(isWinUI) {
            }
            parametersList = GetParametersList(methodSymbol, commandAttributeSymbol, canExecuteMethodName, isCommand, executeMethod, allowMultipleExecution, isWinUI);
        }

        private static string GetParametersList(IMethodSymbol methodSymbol, INamedTypeSymbol commandAttributeSymbol, string canExecuteMethodName, bool isCommand, string executeMethod, string allowMultipleExecution, bool isWinUI) {
            if(isWinUI)
                return isCommand
                    ? CommandHelper.ParametersToDisplayString(executeMethod, canExecuteMethodName)
                    : CommandHelper.ParametersToDisplayString(executeMethod, canExecuteMethodName, allowMultipleExecution);
            else {
                var useCommandManager = CommandHelper.GetUseCommandManagerValue(methodSymbol, commandAttributeSymbol).ToString().ToLower();
                return isCommand
                    ? CommandHelper.ParametersToDisplayString(executeMethod, canExecuteMethodName, useCommandManager)
                    : CommandHelper.ParametersToDisplayString(executeMethod, canExecuteMethodName, allowMultipleExecution, useCommandManager);
            }

        }
    }
}
