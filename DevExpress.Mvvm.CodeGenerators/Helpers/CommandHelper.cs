using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DevExpress.Mvvm.CodeGenerators {
    static class CommandHelper {
        static readonly string allowMultipleExecution = AttributesGenerator.AllowMultipleExecution;
        static readonly string useCommandManager = AttributesGenerator.UseCommandManager;
        static readonly string commandName = AttributesGenerator.CommandName;
        static readonly string canExecuteMethod = AttributesGenerator.CanExecuteMethod;

        public static bool GetAllowMultipleExecutionValue(IMethodSymbol methodSymbol, INamedTypeSymbol commandSymbol) =>
            AttributeHelper.GetPropertyActualValue(methodSymbol, commandSymbol, allowMultipleExecution, false);
        public static bool GetUseCommandManagerValue(IMethodSymbol methodSymbol, INamedTypeSymbol commandSymbol) =>
            AttributeHelper.GetPropertyActualValue(methodSymbol, commandSymbol, useCommandManager, true);
        public static string GetCommandName(IMethodSymbol methodSymbol, INamedTypeSymbol commandSymbol, string executeMethodName) =>
            AttributeHelper.GetPropertyActualValue(methodSymbol, commandSymbol, commandName, executeMethodName + "Command").FirstToLowerCase();
        public static string GetCanExecuteMethodName(IMethodSymbol methodSymbol, INamedTypeSymbol commandSymbol) =>
            AttributeHelper.GetPropertyActualValue(methodSymbol, commandSymbol, canExecuteMethod, (string)null);
        public static string ParametersToDisplayString(params string[] parameters) => parameters.ConcatToString(", ");
        public static string GetGenericType(string baseType, string genericArgumentType) => baseType + (string.IsNullOrEmpty(genericArgumentType) ? string.Empty : "<" + genericArgumentType + ">");
        public static IEnumerable<IMethodSymbol> GetMethods(INamedTypeSymbol classSymbol, Func<IMethodSymbol, bool> condition) =>
            classSymbol.GetMembers().OfType<IMethodSymbol>().Where(condition);
        public static IEnumerable<IMethodSymbol> GetMethods(INamedTypeSymbol classSymbol, string methodName) =>
            classSymbol.GetMembers().OfType<IMethodSymbol>().Where(method => method.Name == methodName);
#nullable enable
        public static IEnumerable<IMethodSymbol> GetCanExecuteMethodCandidates(INamedTypeSymbol classSymbol, string canExecuteMethodName, ITypeSymbol? parameterType) =>
            GetMethods(classSymbol,
                       method => method.ReturnType.ToDisplayStringNullable() == "bool" &&
                                 method.Name == canExecuteMethodName &&
                                 HaveSameParametersList(method.Parameters, parameterType));

        static bool HaveSameParametersList(ImmutableArray<IParameterSymbol> parameters, ITypeSymbol? parameterType) {
            if(parameterType == null)
                return parameters.Count() == 0;
            return parameters.Count() == 1 && PropertyHelper.IsSameType(parameters.First().Type, parameterType);
        }
    }
}
