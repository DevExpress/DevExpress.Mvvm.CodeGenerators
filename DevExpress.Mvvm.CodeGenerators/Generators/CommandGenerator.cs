using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    static class CommandGenerator {
        public static void Generate(SourceBuilder source, ContextInfo info, INamedTypeSymbol classSymbol, IMethodSymbol methodSymbol) {
            var isCommand = methodSymbol.ReturnsVoid;
            var isAsyncCommand = info.TaskSymbol.Equals(methodSymbol.ReturnType, SymbolEqualityComparer.Default)
                || info.TaskSymbol.Equals(methodSymbol.ReturnType?.BaseType, SymbolEqualityComparer.Default);

            if(methodSymbol.Parameters.Length > 1 || !(isCommand || isAsyncCommand)) {
                info.Context.ReportIncorrectCommandSignature(methodSymbol);
                return;
            }

            var parameterType = methodSymbol.Parameters.FirstOrDefault()?.Type;
            var canExecuteMethodName = CommandHelper.GetCanExecuteMethodName(methodSymbol, info.CommandAttributeSymbol);
            if(canExecuteMethodName == null) {
                var candidate = CommandHelper.GetCanExecuteMethodCandidates(classSymbol, "Can" + methodSymbol.Name, parameterType, info);
                canExecuteMethodName = candidate.FirstOrDefault()?.Name ?? "null";
            } else {
                var candidates = CommandHelper.GetCanExecuteMethodCandidates(classSymbol, canExecuteMethodName, parameterType, info);
                if(!candidates.Any()) {
                    info.Context.ReportCanExecuteMethodNotFound(methodSymbol, canExecuteMethodName, parameterType?.ToDisplayStringNullable() ?? string.Empty, CommandHelper.GetMethods(classSymbol, canExecuteMethodName));
                    return;
                }
            }

            var name = CommandHelper.GetCommandName(methodSymbol, info.CommandAttributeSymbol, methodSymbol.Name);
            var genericArgumentType = parameterType?.ToDisplayStringNullable() ?? string.Empty;
            source.AppendCommandGenericType(isCommand, genericArgumentType).Append("? ").AppendFirstToLowerCase(name).AppendLine(";");
            source.Append("public ").AppendCommandGenericType(isCommand, genericArgumentType).Append(' ').Append(name).AppendLine(" {");
            AppendGetter(source.Tab, info, methodSymbol, isCommand, canExecuteMethodName, genericArgumentType, name);
            source.AppendLine("}");
        }

        static void AppendGetter(SourceBuilder source, ContextInfo info, IMethodSymbol methodSymbol, bool isCommand, string canExecuteMethodName, string genericArgumentType, string name) {
            source.Append("get => ").AppendFirstToLowerCase(name).Append(" ??= new ").AppendCommandGenericType(isCommand, genericArgumentType).Append('(');
            source.AppendParametersList(methodSymbol, info.CommandAttributeSymbol, canExecuteMethodName, isCommand, methodSymbol.Name, info.IsWinUI);
            source.AppendLine(");");
        }

        static void AppendParametersList(this SourceBuilder source, IMethodSymbol methodSymbol, INamedTypeSymbol commandAttributeSymbol, string canExecuteMethodName, bool isCommand, string executeMethod, bool isWinUI) {
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
