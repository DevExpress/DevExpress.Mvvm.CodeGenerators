using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    static class CommandGenerator {
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

            var name = CommandHelper.GetCommandName(methodSymbol, info.CommandAttributeSymbol, methodSymbol.Name);
            var genericArgumentType = parameterType?.ToDisplayStringNullable() ?? string.Empty;
            source.AppendTabs(tabs).AppendCommandGenericType(isCommand, genericArgumentType).Append("? ").Append(name).AppendLine(";");
            source.AppendTabs(tabs).Append("public ").AppendCommandGenericType(isCommand, genericArgumentType).Append(' ').AppendFirstToUpperCase(name).AppendLine(" {");
            AppendGetter(source, tabs, info, methodSymbol, isCommand, canExecuteMethodName, genericArgumentType, name);
            source.AppendLineWithTabs("}", tabs);
        }

        static void AppendGetter(StringBuilder source, int tabs, ContextInfo info, IMethodSymbol methodSymbol, bool isCommand, string canExecuteMethodName, string genericArgumentType, string name) {
            source.AppendTabs(tabs + 1).Append("get => ").Append(name).Append(" ??= new ").AppendCommandGenericType(isCommand, genericArgumentType).Append('(');
            source.AppendParametersList(methodSymbol, info.CommandAttributeSymbol, canExecuteMethodName, isCommand, methodSymbol.Name, info.IsWinUI);
            source.AppendLine(");");
        }

        static void AppendParametersList(this StringBuilder source, IMethodSymbol methodSymbol, INamedTypeSymbol commandAttributeSymbol, string canExecuteMethodName, bool isCommand, string executeMethod, bool isWinUI) {
            source.Append(executeMethod).Append(", ").Append(canExecuteMethodName);
            if(!isCommand) {
                var allowMultipleExecution = CommandHelper.GetAllowMultipleExecutionValue(methodSymbol, commandAttributeSymbol).BoolToStringValue();
                source.Append(", ").Append(allowMultipleExecution);
            }
            if(!isWinUI) {
                var useCommandManager = CommandHelper.GetUseCommandManagerValue(methodSymbol, commandAttributeSymbol).BoolToStringValue();
                source.Append(", ").Append(useCommandManager);
            }
        }
    }
}
