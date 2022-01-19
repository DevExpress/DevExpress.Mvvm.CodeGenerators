using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DevExpress.Mvvm.CodeGenerators {
    static class CommandGenerator {
        public static void Generate(SourceBuilder source, ContextInfo info, INamedTypeSymbol classSymbol, IMethodSymbol methodSymbol, SupportedMvvm mvvm) {
            var commandAttribute = info.GetFrameworkAttributes(mvvm).CommandAttributeSymbol;
            bool isCommand = methodSymbol.ReturnsVoid;
            bool isAsyncCommand = SymbolEqualityComparer.Default.Equals(info.TaskSymbol, methodSymbol.ReturnType)
                || SymbolEqualityComparer.Default.Equals(info.TaskSymbol, methodSymbol.ReturnType?.BaseType);

            if(methodSymbol.Parameters.Length > 1 || !(isCommand || isAsyncCommand)) {
                info.Context.ReportIncorrectCommandSignature(methodSymbol);
                return;
            }

            ITypeSymbol? parameterType = methodSymbol.Parameters.FirstOrDefault()?.Type;

            if(mvvm == SupportedMvvm.Prism) {
                if((parameterType?.IsValueType == true) && (parameterType.NullableAnnotation != NullableAnnotation.Annotated)) {
                    info.Context.ReportNonNullableDelegateCommandArgument(methodSymbol);
                    return;
                }
            }

            string? canExecuteMethodName = GetCanExecuteMethodName(info, classSymbol, methodSymbol, parameterType, commandAttribute);
            string name = CommandHelper.GetCommandName(methodSymbol, commandAttribute, methodSymbol.Name);
            string genericArgumentType = parameterType?.ToDisplayStringNullable() ?? string.Empty;

            source.AppendCommandGenericType(mvvm, isCommand, genericArgumentType).Append("? ").AppendFirstToLowerCase(name).AppendLine(";");

            CSharpSyntaxNode commandSyntaxNode = (CSharpSyntaxNode)methodSymbol.DeclaringSyntaxReferences[0].GetSyntax();
            XMLCommentHelper.AppendComment(source, commandSyntaxNode);

            AttributeHelper.AppendMethodAttriutes(source, methodSymbol, info);

            source.Append("public ").AppendCommandGenericType(mvvm, isCommand, genericArgumentType).Append(" ").Append(name);

            switch(mvvm) {
                case SupportedMvvm.None:
                    break;
                case SupportedMvvm.Dx:
                    AppendGetterDx(source, info, methodSymbol, mvvm, isCommand, canExecuteMethodName, genericArgumentType, name);
                    break;
                case SupportedMvvm.Prism:
                    string observesCanExecuteProperty = CommandHelper.GetObservesCanExecuteProperty(methodSymbol, commandAttribute);
                    string[] observesProperties = CommandHelper.GetObservesProperties(methodSymbol, commandAttribute);
                    AppendGetterPrism(source, info, methodSymbol, mvvm, isCommand, canExecuteMethodName, genericArgumentType, name, observesCanExecuteProperty, observesProperties);
                    break;
                case SupportedMvvm.MvvmLight:
                    AppendGetterMvvmLight(source, info, methodSymbol, mvvm, isCommand, canExecuteMethodName, genericArgumentType, name);
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        static string GetCanExecuteMethodName(ContextInfo info, INamedTypeSymbol classSymbol, IMethodSymbol methodSymbol, ITypeSymbol? parameterType, INamedTypeSymbol commandAttributeSymbol) {
            string? canExecuteMethodName = CommandHelper.GetCanExecuteMethodName(methodSymbol, commandAttributeSymbol);
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

        static void AppendGetterPrism(SourceBuilder source, ContextInfo info, IMethodSymbol methodSymbol, SupportedMvvm mvvm, bool isCommand, string canExecuteMethodName, string genericArgumentType, string name, string observesCanExecuteProperty, string[] observesProperties) {
            source.AppendCommandNameWithGenericType(mvvm, isCommand, genericArgumentType, name).AppendMethodName(isCommand, methodSymbol.Name, genericArgumentType);
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
        static void AppendGetterDx(SourceBuilder source, ContextInfo info, IMethodSymbol methodSymbol, SupportedMvvm mvvm, bool isCommand, string canExecuteMethodName, string genericArgumentType, string name) {
            INamedTypeSymbol commandAttribute = info.Dx!.CommandAttributeSymbol;
            source.AppendCommandNameWithGenericType(mvvm, isCommand, genericArgumentType, name).Append('(');
            source.AppendDxParametersList(methodSymbol, commandAttribute, canExecuteMethodName, isCommand, methodSymbol.Name, info.IsWinUI);
            source.AppendLine(");");
        }
        static void AppendGetterMvvmLight(SourceBuilder source, ContextInfo info, IMethodSymbol methodSymbol, SupportedMvvm mvvm, bool isCommand, string canExecuteMethodName, string genericArgumentType, string name) {
            source.AppendCommandNameWithGenericType(mvvm, isCommand, genericArgumentType, name).AppendMethodName(isCommand, methodSymbol.Name, genericArgumentType);
            source.Append(", ").Append(canExecuteMethodName).AppendLine(");");
        }
        static void AppendDxParametersList(this SourceBuilder source, IMethodSymbol methodSymbol, INamedTypeSymbol commandAttributeSymbol, string canExecuteMethodName, bool isCommand, string executeMethod, bool isWinUI) {
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
