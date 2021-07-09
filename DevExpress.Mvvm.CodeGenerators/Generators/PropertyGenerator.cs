using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    static class PropertyGenerator {
        public static string Generate(StringBuilder source, int tabs, ContextInfo info, INamedTypeSymbol classSymbol, IFieldSymbol fieldSymbol, string inpcedParameter, string inpcingParameter) {
            var propertyName = PropertyHelper.CreatePropertyName(fieldSymbol.Name);
            if(propertyName == fieldSymbol.Name)
                info.Context.ReportInvalidPropertyName(fieldSymbol, propertyName);

            var changedMethod = PropertyHelper.GetChangedMethod(info, classSymbol, fieldSymbol, propertyName, fieldSymbol.Type);
            var changingMethod = PropertyHelper.GetChangingMethod(info, classSymbol, fieldSymbol, propertyName, fieldSymbol.Type);

            if(propertyName == fieldSymbol.Name || changedMethod == null || changingMethod == null)
                return null;

            var attributesList = PropertyHelper.GetAttributesList(fieldSymbol);
            if(!string.IsNullOrEmpty(attributesList))
                source.AppendMultipleLinesWithTabs(attributesList, tabs);

            var isVirtual = PropertyHelper.GetIsVirtualValue(fieldSymbol, info.PropertyAttributeSymbol);
            var virtuality = isVirtual ? "virtual " : string.Empty;
            var typeName = fieldSymbol.Type.WithNullableAnnotation(PropertyHelper.GetNullableAnnotation(fieldSymbol.Type)).ToDisplayStringNullable();
            var fieldName = fieldSymbol.Name == "value" ? "this.value" : fieldSymbol.Name;
            source.AppendLineWithTabs($"public {virtuality}{typeName} {propertyName} {{", tabs);
            source.AppendLineWithTabs($"get => {fieldName};", tabs + 1);

            string setterAttribute = GetSetterAttribute(info, fieldSymbol, fieldName);
            if(!string.IsNullOrEmpty(setterAttribute))
                source.AppendLineWithTabs(setterAttribute, tabs + 1);

            var setterAccessModifier = PropertyHelper.GetSetterAccessModifierValue(fieldSymbol, info.PropertyAttributeSymbol);
            source.AppendLineWithTabs($"{setterAccessModifier}set {{", tabs + 1);
            source.AppendLineWithTabs($"if(EqualityComparer<{typeName}>.Default.Equals({fieldName}, value)) return;", tabs + 2);

            string raiseChangingMethod = GetRaiseChangingMethod(inpcingParameter, propertyName);
            if(!string.IsNullOrEmpty(raiseChangingMethod))
                source.AppendLineWithTabs(raiseChangingMethod, tabs + 2);
            if(!string.IsNullOrEmpty(changingMethod))
                source.AppendLineWithTabs(changingMethod, tabs + 2);

            if(!string.IsNullOrEmpty(changedMethod) && !changedMethod.EndsWith("();"))
                source.AppendLineWithTabs($"var oldValue = {fieldName};", tabs + 2);
            source.AppendLineWithTabs($"{fieldName} = value;", tabs + 2);

            string raiseChangedMethod = GetRaiseChangedMethod(inpcedParameter, propertyName);
            if(!string.IsNullOrEmpty(raiseChangedMethod))
                source.AppendLineWithTabs(raiseChangedMethod, tabs + 2);

            if(!string.IsNullOrEmpty(changedMethod))
                source.AppendLineWithTabs(changedMethod, tabs + 2);

            source.AppendLineWithTabs("}", tabs + 1);
            source.AppendLineWithTabs("}", tabs);

            return propertyName;
        }

        static string GetRaiseChangingMethod(string inpcingParameter, string propertyName) {
            string raiseChangingMethod = null;
            if(inpcingParameter == "eventargs")
                raiseChangingMethod = $"RaisePropertyChanging({propertyName}ChangingEventArgs);";
            else if(inpcingParameter == "string")
                raiseChangingMethod = $"RaisePropertyChanging(nameof({propertyName}));";
            return raiseChangingMethod;
        }

        static string GetRaiseChangedMethod(string inpcedParameter, string propertyName) {
            string raiseChangedMethod = null;
            if(inpcedParameter == "eventargs")
                raiseChangedMethod = $"RaisePropertyChanged({propertyName}ChangedEventArgs);";
            else if(inpcedParameter == "string")
                raiseChangedMethod = $"RaisePropertyChanged(nameof({propertyName}));";
            return raiseChangedMethod;
        }

        static string GetSetterAttribute(ContextInfo info, IFieldSymbol fieldSymbol, string fieldName) {
            var isNonNullableReferenceType = fieldSymbol.Type.IsReferenceType && fieldSymbol.Type.NullableAnnotation == NullableAnnotation.NotAnnotated;
            string setterAttribute = null;
            if(isNonNullableReferenceType && PropertyHelper.HasMemberNotNullAttribute(info.Compilation))
                setterAttribute = $"[System.Diagnostics.CodeAnalysis.MemberNotNull(nameof({fieldName}))]";
            return setterAttribute;
        }
    }
}
