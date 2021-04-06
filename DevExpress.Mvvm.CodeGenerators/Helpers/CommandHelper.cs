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
        public static string ParameterTypeToDisplayString(params string[] parameterType) => parameterType.ConcatToString(", ");
        public static string GetGenericType(string baseType, string genericArgumentType) => baseType + (string.IsNullOrEmpty(genericArgumentType) ? string.Empty : "<" + genericArgumentType + ">");
        public static IEnumerable<IMethodSymbol> GetCanExecuteMethodCandidates(INamedTypeSymbol classSymbol, string canExecuteMethodName, string parameterType) =>
            GetMethods(classSymbol,
                       method => method.ReturnType.ToDisplayStringNullable() == "bool" &&
                                 method.Name == canExecuteMethodName &&
                                 HaveSameParametersList(method.Parameters, parameterType));
        public static IEnumerable<IMethodSymbol> GetMethods(INamedTypeSymbol classSymbol, Func<IMethodSymbol, bool> condition) =>
            classSymbol.GetMembers().OfType<IMethodSymbol>().Where(condition);
        public static IEnumerable<IMethodSymbol> GetMethods(INamedTypeSymbol classSymbol, string methodName) =>
            classSymbol.GetMembers().OfType<IMethodSymbol>().Where(method => method.Name == methodName);

        static bool HaveSameParametersList(ImmutableArray<IParameterSymbol> parameters, string parameterType) {
            var bothHaveNoParameter = parameters.Count() == 0 && parameterType == string.Empty;
            var bothHaveSameSingleParameter = parameters.Count() == 1 && parameters.First().Type.ToDisplayStringNullable() == parameterType;
            return bothHaveNoParameter || bothHaveSameSingleParameter;
        }
    }
}
