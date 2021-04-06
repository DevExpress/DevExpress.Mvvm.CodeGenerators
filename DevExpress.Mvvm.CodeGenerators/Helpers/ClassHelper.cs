using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevExpress.Mvvm.CodeGenerators {
    static class ClassHelper {
        static readonly string nameofImplementIDEI = AttributesGenerator.ImplementIDEI;
        static readonly string nameofImplementISS = AttributesGenerator.ImplementISS;

        public static bool IsMvvmAvailable(Compilation compilation) =>
            compilation.ReferencedAssemblyNames.Any(ai => Regex.IsMatch(ai.Name, @"DevExpress\.Mvvm(\.v\d{2}\.\d)?$"));
        public static bool GetImplementIDEIValue(ContextInfo contextInfo, INamedTypeSymbol classSymbol) =>
            AttributeHelper.GetPropertyActualValue(classSymbol, contextInfo.ViewModelAttributeSymbol, nameofImplementIDEI, false);
        public static bool GetImplementISSValue(ContextInfo contextInfo, INamedTypeSymbol classSymbol) =>
            AttributeHelper.GetPropertyActualValue(classSymbol, contextInfo.ViewModelAttributeSymbol, nameofImplementISS, false);
        public static IEnumerable<IFieldSymbol> GetFieldCandidates(INamedTypeSymbol classSymbol, INamedTypeSymbol propertyAttribute) =>
            GetProcessingMembers<IFieldSymbol>(classSymbol, propertyAttribute);
        public static IEnumerable<IMethodSymbol> GetCommandCandidates(INamedTypeSymbol classSymbol, INamedTypeSymbol commandAttribute) =>
            GetProcessingMembers<IMethodSymbol>(classSymbol, commandAttribute);
        public static bool IsInterfaceImplemented(INamedTypeSymbol classSymbol, INamedTypeSymbol interfaceSymbol) =>
            classSymbol.Interfaces.Contains(interfaceSymbol);

        static IEnumerable<T> GetProcessingMembers<T>(INamedTypeSymbol classSymbol, INamedTypeSymbol attributeSymbol) where T : ISymbol =>
            classSymbol.GetMembers()
                       .OfType<T>()
                       .Where(symbol => AttributeHelper.HasAttribute(symbol, attributeSymbol));
    }
}
