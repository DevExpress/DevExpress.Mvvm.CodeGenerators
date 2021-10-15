using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Mvvm.CodeGenerators {
    static class ClassHelper {
        static readonly string nameofImplementIDEI = AttributesGenerator.ImplementIDEI;
        static readonly string nameofImplementISS = AttributesGenerator.ImplementISS;
        static readonly string nameofImplementISPVM = AttributesGenerator.ImplementISPVM;
        static readonly string nameofImplementIAA = AttributesGenerator.ImplementIAA;

        public static bool GetImplementIDEIValue(ContextInfo contextInfo, INamedTypeSymbol classSymbol) =>
            !contextInfo.IsWinUI && AttributeHelper.GetPropertyActualValue(classSymbol, contextInfo.ViewModelAttributeSymbol!, nameofImplementIDEI, false);
        public static bool GetImplementISPVMValue(ContextInfo contextInfo, INamedTypeSymbol classSymbol) =>
            AttributeHelper.GetPropertyActualValue(classSymbol, contextInfo.ViewModelAttributeSymbol!, nameofImplementISPVM, false);
        public static bool GetImplementISSValue(ContextInfo contextInfo, INamedTypeSymbol classSymbol) =>
            AttributeHelper.GetPropertyActualValue(classSymbol, contextInfo.ViewModelAttributeSymbol!, nameofImplementISS, false);
        public static bool GetImplementIAAValue(ContextInfo contextInfo, INamedTypeSymbol classSymbol) =>
            AttributeHelper.GetPropertyActualValue(classSymbol, contextInfo.ViewModelAttributeSymbol!, nameofImplementIAA, false);
        public static IEnumerable<IFieldSymbol> GetFieldCandidates(INamedTypeSymbol classSymbol, INamedTypeSymbol propertySymbol) =>
            GetProcessingMembers<IFieldSymbol>(classSymbol, propertySymbol);
        public static IEnumerable<IMethodSymbol> GetCommandCandidates(INamedTypeSymbol classSymbol, INamedTypeSymbol commandSymbol) =>
            GetProcessingMembers<IMethodSymbol>(classSymbol, commandSymbol);
        public static bool IsInterfaceImplementedInCurrentClass(INamedTypeSymbol classSymbol, INamedTypeSymbol interfaceSymbol) =>
            classSymbol.Interfaces.Contains(interfaceSymbol);
        public static bool IsInterfaceImplemented(INamedTypeSymbol classSymbol, INamedTypeSymbol interfaceSymbol, ContextInfo contextInfo) {
            if(IsInterfaceImplementedInCurrentClass(classSymbol, interfaceSymbol))
                return true;
            for(INamedTypeSymbol parent = classSymbol.BaseType!; parent != null; parent = parent.BaseType!) {
                bool hasAttribute = AttributeHelper.HasAttribute(parent, contextInfo.ViewModelAttributeSymbol!) && GetImplementISPVMValue(contextInfo, parent);
                bool hasImplementation = IsInterfaceImplementedInCurrentClass(parent, interfaceSymbol);
                if(hasAttribute || hasImplementation)
                    return true;
            }
            return false;
        }

        static IEnumerable<T> GetProcessingMembers<T>(INamedTypeSymbol classSymbol, INamedTypeSymbol attributeSymbol) where T : ISymbol =>
            classSymbol.GetMembers()
                       .OfType<T>()
                       .Where(symbol => AttributeHelper.HasAttribute(symbol, attributeSymbol));

        public static bool ContainsOnChangedMethod(INamedTypeSymbol classSymbol, string methodName, int parametersCount, string? parameterType) {
            IEnumerable<IMethodSymbol> onChangedMethod = CommandHelper.GetMethods(classSymbol, methodSymbol => methodSymbol.ReturnsVoid && methodSymbol.Name == methodName && methodSymbol.Parameters.Length == parametersCount &&
                                                   (string.IsNullOrEmpty(parameterType) || methodSymbol.Parameters[0].ToDisplayString().StartsWith(parameterType)));
            return onChangedMethod.Any();
        }
        public static Dictionary<string, TypeKind> GetOuterClasses(INamedTypeSymbol classSymbol) {
            Dictionary<string, TypeKind> outerClasses = new Dictionary<string, TypeKind>();
            ISymbol outerClass = classSymbol.ContainingSymbol;
            while(!outerClass.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default)) {
                outerClasses.Add(outerClass.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), ((INamedTypeSymbol)outerClass).TypeKind);

                outerClass = outerClass.ContainingSymbol;
            }
            return outerClasses;
        }
        public static string CreateFileName(string prefix) => $"{prefix}.g.cs";
        public static string CreateFileName(string prefix, HashSet<string> generatedClasses) {
            string name = prefix;
            int i = 1;
            while(generatedClasses.Contains(name)) {
                name = $"{prefix}_{i}";
                i++;
            }
            generatedClasses.Add(name);
            return $"{name}.g.cs";
        }
    }
}
