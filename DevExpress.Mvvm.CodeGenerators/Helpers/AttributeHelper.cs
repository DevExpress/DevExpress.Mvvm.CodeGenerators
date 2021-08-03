using Microsoft.CodeAnalysis;
using System.Linq;

namespace DevExpress.Mvvm.CodeGenerators {
    static class AttributeHelper {
        public static T? GetPropertyActualValue<T>(ISymbol sourceSymbol, INamedTypeSymbol? attributeSymbol, string propertyName, T defaultValue) {
            TypedConstant? argument = GetAttributeData(sourceSymbol, attributeSymbol)?.NamedArguments
                                                                          .SingleOrDefault(kvp => kvp.Key == propertyName)
                                                                          .Value;
            if(argument == null || ((TypedConstant)argument).IsNull)
                return defaultValue;
            return (T?)((TypedConstant)argument).Value;
        }
        public static bool HasAttribute(ISymbol sourceSymbol, INamedTypeSymbol? attributeSymbol) =>
            GetAttributeData(sourceSymbol, attributeSymbol) != null;

        static AttributeData? GetAttributeData(ISymbol sourceSymbol, INamedTypeSymbol? attributeSymbol) =>
            sourceSymbol.GetAttributes().FirstOrDefault(ad => SymbolEqualityComparer.Default.Equals(ad.AttributeClass, attributeSymbol));
    }
}
