using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Mvvm.CodeGenerators {
    static class CommandGenerator {
        public static void GenerateDX(SourceBuilder source, ContextInfo info, INamedTypeSymbol classSymbol, IMethodSymbol methodSymbol) {
            bool isCommand;
            SetIsCommand(info, methodSymbol, out isCommand);

            ITypeSymbol? parameterType = methodSymbol.Parameters.FirstOrDefault()?.Type;
            string? canExecuteMethodName = GetCanExecuteMethodName(info, classSymbol, methodSymbol, parameterType);
            string name = CommandHelper.GetCommandName(methodSymbol, info.CommandAttributeSymbol!, methodSymbol.Name);
            string genericArgumentType = parameterType?.ToDisplayStringNullable() ?? string.Empty;

            source.AppendCommandGenericType(isCommand, genericArgumentType).Append("? ").AppendFirstToLowerCase(name).AppendLine(";");

            CSharpSyntaxNode commandSyntaxNode = (CSharpSyntaxNode)methodSymbol.DeclaringSyntaxReferences[0].GetSyntax();
            XMLCommentHelper.AppendComment(source, commandSyntaxNode);

            source.Append("public ").AppendCommandGenericType(isCommand, genericArgumentType).Append(' ').Append(name);
            AppendGetterDx(source, info, methodSymbol, isCommand, canExecuteMethodName, genericArgumentType, name);
        }

        public static void GeneratePrism(SourceBuilder source, ContextInfo info, INamedTypeSymbol classSymbol, IMethodSymbol methodSymbol) {
            bool isCommand;
            SetIsCommand(info, methodSymbol, out isCommand);

            ITypeSymbol? parameterType = methodSymbol.Parameters.FirstOrDefault()?.Type;
            if((parameterType?.IsValueType ==  true) && (parameterType.NullableAnnotation != NullableAnnotation.Annotated)) {
                info.Context.ReportNonNullableDelegateCommandArgument(methodSymbol);
                return;
            }
            string? canExecuteMethodName = GetCanExecuteMethodName(info, classSymbol, methodSymbol, parameterType);

            string name = CommandHelper.GetCommandName(methodSymbol, info.CommandAttributeSymbol!, methodSymbol.Name);
            string genericArgumentType = parameterType?.ToDisplayStringNullable() ?? string.Empty;
            source.AppendCommandGenericType(true, genericArgumentType).Append("? ").AppendFirstToLowerCase(name).AppendLine(";");

            CSharpSyntaxNode commandSyntaxNode = (CSharpSyntaxNode)methodSymbol.DeclaringSyntaxReferences[0].GetSyntax();
            XMLCommentHelper.AppendComment(source, commandSyntaxNode);

            string observesCanExecuteProperty = CommandHelper.GetObservesCanExecuteProperty(methodSymbol, info.CommandAttributeSymbol!);
            string[] observesProperties = CommandHelper.GetObservesProperties(methodSymbol, info.CommandAttributeSymbol!);

            source.Append("public ").AppendCommandGenericType(true, genericArgumentType).Append(" ").Append(name);
            AppendGetterPrism(source, info, methodSymbol, isCommand, canExecuteMethodName, genericArgumentType, name, observesCanExecuteProperty, observesProperties);
        }

        static string GetCanExecuteMethodName(ContextInfo info, INamedTypeSymbol classSymbol, IMethodSymbol methodSymbol, ITypeSymbol? parameterType) {
            string? canExecuteMethodName = CommandHelper.GetCanExecuteMethodName(methodSymbol, info.CommandAttributeSymbol!);
            if(canExecuteMethodName == null) {
                IEnumerable<IMethodSymbol> candidate = CommandHelper.GetCanExecuteMethodCandidates(classSymbol, "Can" + methodSymbol.Name, parameterType, info);
                canExecuteMethodName = candidate.FirstOrDefault()?.Name;
            } else {
                IEnumerable<IMethodSymbol> candidates = CommandHelper.GetCanExecuteMethodCandidates(classSymbol, canExecuteMethodName, parameterType, info);
                if(!candidates.Any()) {
                    info.Context.ReportCanExecuteMethodNotFound(methodSymbol, canExecuteMethodName, parameterType?.ToDisplayStringNullable() ?? string.Empty, CommandHelper.GetMethods(classSymbol, canExecuteMethodName));
                }
            }
            return canExecuteMethodName ?? "null";
        }

        static void SetIsCommand(ContextInfo info, IMethodSymbol methodSymbol, out bool isCommand) {
            isCommand = methodSymbol.ReturnsVoid;
            bool isAsyncCommand = SymbolEqualityComparer.Default.Equals(info.TaskSymbol, methodSymbol.ReturnType)
                || SymbolEqualityComparer.Default.Equals(info.TaskSymbol, methodSymbol.ReturnType?.BaseType);

            if(methodSymbol.Parameters.Length > 1 || !(isCommand || isAsyncCommand)) {
                info.Context.ReportIncorrectCommandSignature(methodSymbol);
                return;
            }
        }

        static void AppendGetterPrism(SourceBuilder source, ContextInfo info, IMethodSymbol methodSymbol, bool isCommand, string canExecuteMethodName, string genericArgumentType, string name, string observesCanExecuteProperty, string[] observesProperties) {
            source.Append(" => ").AppendFirstToLowerCase(name).Append(" ??= new ").AppendCommandGenericType(true, genericArgumentType).AppendMethodNamePrism(isCommand, methodSymbol.Name, genericArgumentType);
            if(canExecuteMethodName != "null")
                source.Append(", ").Append(canExecuteMethodName);
            if(!string.IsNullOrEmpty(observesCanExecuteProperty))
                source.Append(").ObservesCanExecute(() => ").Append(observesCanExecuteProperty);
            if(observesProperties.Count() > 0)
                foreach(string property in observesProperties)
                    if(!string.IsNullOrEmpty(property))
                        source.Append(").ObservesProperty(() => ").Append(property);
            source.AppendLine(");");
        }
        static void AppendGetterDx(SourceBuilder source, ContextInfo info, IMethodSymbol methodSymbol, bool isCommand, string canExecuteMethodName, string genericArgumentType, string name) {
            source.Append(" => ").AppendFirstToLowerCase(name).Append(" ??= new ").AppendCommandGenericType(isCommand, genericArgumentType).Append('(');
            source.AppendParametersList(methodSymbol, info.CommandAttributeSymbol!, canExecuteMethodName, isCommand, methodSymbol.Name, info.IsWinUI);
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
