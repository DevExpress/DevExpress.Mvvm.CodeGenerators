using Microsoft.CodeAnalysis;
using System.Linq;

namespace DevExpress.Mvvm.CodeGenerators {
    static class AttributeHelper {
        public static T GetPropertyActualValue<T>(ISymbol sourceSymbol, INamedTypeSymbol attributeSymbol, string propertyName, T defaultValue) {
            var argument = GetAttributeData(sourceSymbol, attributeSymbol).NamedArguments
                                                                          .SingleOrDefault(kvp => kvp.Key == propertyName)
                                                                          .Value;
            if(argument.IsNull)
                return defaultValue;
            return (T)argument.Value;
        }
        public static bool HasAttribute(ISymbol sourceSymbol, INamedTypeSymbol attributeSymbol) =>
            GetAttributeData(sourceSymbol, attributeSymbol) != null;

        static AttributeData GetAttributeData(ISymbol sourceSymbol, INamedTypeSymbol attributeSymbol) =>
            sourceSymbol.GetAttributes().FirstOrDefault(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));
    }
}
