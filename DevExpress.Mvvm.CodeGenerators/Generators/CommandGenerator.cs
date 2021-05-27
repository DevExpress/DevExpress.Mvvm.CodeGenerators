using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    class CommandGenerator {
        public static void Generate(StringBuilder source, int tabs, ContextInfo info, INamedTypeSymbol classSymbol, IMethodSymbol methodSymbol) {
            var isCommand = methodSymbol.ReturnsVoid;
            var isAsyncCommand = methodSymbol.ReturnType.ToDisplayStringNullable().StartsWith("System.Threading.Tasks.Task");
            if(methodSymbol.Parameters.Length > 1 || !(isCommand || isAsyncCommand)) {
                info.Context.ReportIncorrectCommandSignature(methodSymbol);
                return;
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
                    return;
                }
            }

            var type = CommandHelper.GetGenericType(isCommand ? "DelegateCommand" : "AsyncCommand", parameterType?.ToDisplayStringNullable() ?? string.Empty);
            var name = CommandHelper.GetCommandName(methodSymbol, info.CommandAttributeSymbol, methodSymbol.Name);
            var parametersList = GetParametersList(methodSymbol, info.CommandAttributeSymbol, canExecuteMethodName, isCommand, methodSymbol.Name, info.IsWinUI);
            source.AppendLine($"{type}? {name};".AddTabs(tabs));
            source.AppendLine($"public {type} {name.FirstToUpperCase()} {{".AddTabs(tabs));
            source.AppendLine($"get => {name} ??= new {type}({parametersList});".AddTabs(tabs + 1));
            source.AppendLine("}".AddTabs(tabs));
        }

        static string GetParametersList(IMethodSymbol methodSymbol, INamedTypeSymbol commandAttributeSymbol, string canExecuteMethodName, bool isCommand, string executeMethod, bool isWinUI) {
            var allowMultipleExecution = CommandHelper.GetAllowMultipleExecutionValue(methodSymbol, commandAttributeSymbol).ToString().ToLower();
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
