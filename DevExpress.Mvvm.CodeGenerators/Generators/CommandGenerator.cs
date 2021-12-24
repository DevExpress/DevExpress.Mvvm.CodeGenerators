using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Mvvm.CodeGenerators {
    static class CommandGenerator {
        public static void Generate(SourceBuilder source, ContextInfo info, INamedTypeSymbol classSymbol, IMethodSymbol methodSymbol, SupportedMvvm mvvm) {
            var commandAttribute = info.GetFrameworkAttributes(mvvm).CommandAttributeSymbol!;
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


            if(mvvm == SupportedMvvm.Prism || mvvm == SupportedMvvm.MvvmLight) {
                source.AppendCommandGenericType(mvvm, true, genericArgumentType).Append("? ").AppendFirstToLowerCase(name).AppendLine(";");
            } else {
                source.AppendCommandGenericType(mvvm, isCommand, genericArgumentType).Append("? ").AppendFirstToLowerCase(name).AppendLine(";");
            }

            CSharpSyntaxNode commandSyntaxNode = (CSharpSyntaxNode)methodSymbol.DeclaringSyntaxReferences[0].GetSyntax();
            XMLCommentHelper.AppendComment(source, commandSyntaxNode);

            if(mvvm == SupportedMvvm.Prism) {
                string observesCanExecuteProperty = CommandHelper.GetObservesCanExecuteProperty(methodSymbol, commandAttribute);
                string[] observesProperties = CommandHelper.GetObservesProperties(methodSymbol, commandAttribute);

                source.Append("public ").AppendCommandGenericType(mvvm, true, genericArgumentType).Append(" ").Append(name);
                AppendGetterPrism(source, info, methodSymbol, mvvm, isCommand, canExecuteMethodName, genericArgumentType, name, observesCanExecuteProperty, observesProperties);
            } else {
                source.Append("public ").AppendCommandGenericType(mvvm, isCommand, genericArgumentType).Append(' ').Append(name);
                if(mvvm == SupportedMvvm.Dx)
                    AppendGetterDx(source, info, methodSymbol, mvvm, isCommand, canExecuteMethodName, genericArgumentType, name);
                else
                    AppendGetterMvvmLight(source, info, methodSymbol, mvvm, isCommand, canExecuteMethodName, genericArgumentType, name);

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
            source.Append(" => ").AppendFirstToLowerCase(name).Append(" ??= new ").AppendCommandGenericType(mvvm, true, genericArgumentType).AppendMethodName(isCommand, methodSymbol.Name, genericArgumentType);
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
            INamedTypeSymbol commandAttribute = info.Dx!.CommandAttributeSymbol!;
            source.Append(" => ").AppendFirstToLowerCase(name).Append(" ??= new ").AppendCommandGenericType(mvvm, isCommand, genericArgumentType).Append('(');
            source.AppendDxParametersList(methodSymbol, commandAttribute, canExecuteMethodName, isCommand, methodSymbol.Name, info.IsWinUI);
            source.AppendLine(");");
        }
        static void AppendGetterMvvmLight(SourceBuilder source, ContextInfo info, IMethodSymbol methodSymbol, SupportedMvvm mvvm, bool isCommand, string canExecuteMethodName, string genericArgumentType, string name) {
            INamedTypeSymbol commandAttribute = info.MvvmLight!.CommandAttributeSymbol!;
            source.Append(" => ").AppendFirstToLowerCase(name).Append(" ??= new ").AppendCommandGenericType(mvvm, isCommand, genericArgumentType).AppendMethodName(isCommand, methodSymbol.Name, genericArgumentType);
            source.AppendMvvmLightParameterList(methodSymbol, commandAttribute, canExecuteMethodName, methodSymbol.Name);
            source.AppendLine(");");
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
        static void AppendMvvmLightParameterList(this SourceBuilder source, IMethodSymbol methodSymbol, INamedTypeSymbol commandAttributeSymbol, string canExecuteMethodName, string executeMethod) {
            source.Append(", ").Append(canExecuteMethodName);
            bool keepTargetAlive = CommandHelper.GetKeepTargetAliveValue(methodSymbol, commandAttributeSymbol);
            if(keepTargetAlive)
                source.Append(", ").Append(keepTargetAlive.BoolToStringValue());
        }
    }
}
