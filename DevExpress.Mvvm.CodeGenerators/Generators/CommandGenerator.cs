using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Mvvm.CodeGenerators {
    static class CommandGenerator {
        public static void Generate(SourceBuilder source, ContextInfo info, INamedTypeSymbol classSymbol, IMethodSymbol methodSymbol) {
            bool isCommand = methodSymbol.ReturnsVoid;
            bool isAsyncCommand = SymbolEqualityComparer.Default.Equals(info.TaskSymbol, methodSymbol.ReturnType)
                || SymbolEqualityComparer.Default.Equals(info.TaskSymbol, methodSymbol.ReturnType?.BaseType);

            if(methodSymbol.Parameters.Length > 1 || !(isCommand || isAsyncCommand)) {
                info.Context.ReportIncorrectCommandSignature(methodSymbol);
                return;
            }

            ITypeSymbol? parameterType = methodSymbol.Parameters.FirstOrDefault()?.Type;
            string? canExecuteMethodName = CommandHelper.GetCanExecuteMethodName(methodSymbol, info.CommandAttributeSymbol);
            if(canExecuteMethodName == null) {
                IEnumerable<IMethodSymbol> candidate = CommandHelper.GetCanExecuteMethodCandidates(classSymbol, "Can" + methodSymbol.Name, parameterType, info);
                canExecuteMethodName = candidate.FirstOrDefault()?.Name ?? "null";
            } else {
                IEnumerable<IMethodSymbol> candidates = CommandHelper.GetCanExecuteMethodCandidates(classSymbol, canExecuteMethodName, parameterType, info);
                if(!candidates.Any()) {
                    info.Context.ReportCanExecuteMethodNotFound(methodSymbol, canExecuteMethodName, parameterType?.ToDisplayStringNullable() ?? string.Empty, CommandHelper.GetMethods(classSymbol, canExecuteMethodName));
                    return;
                }
            }

            string name = CommandHelper.GetCommandName(methodSymbol, info.CommandAttributeSymbol, methodSymbol.Name);
            string genericArgumentType = parameterType?.ToDisplayStringNullable() ?? string.Empty;
            source.AppendCommandGenericType(isCommand, genericArgumentType).Append("? ").AppendFirstToLowerCase(name).AppendLine(";");

            CSharpSyntaxNode commandSyntaxNode = (CSharpSyntaxNode)methodSymbol.DeclaringSyntaxReferences[0].GetSyntax();
            XMLCommentHelper.AppendComment(source, commandSyntaxNode);

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
                string allowMultipleExecution = CommandHelper.GetAllowMultipleExecutionValue(methodSymbol, commandAttributeSymbol).BoolToStringValue();
                source.Append(", ").Append(allowMultipleExecution);
            }
            if(!isWinUI) {
                string useCommandManager = CommandHelper.GetUseCommandManagerValue(methodSymbol, commandAttributeSymbol).BoolToStringValue();
                source.Append(", ").Append(useCommandManager);
            }
        }
    }
}
