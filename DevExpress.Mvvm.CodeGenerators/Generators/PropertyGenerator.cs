using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    class PropertyGenerator {
        readonly string attributesList;
        readonly string virtuality;
        readonly string type;
        readonly string propertyName;
        readonly string fieldName;
        readonly string setterAttribute;
        readonly string setterAccessModifier;
        readonly string raiseChangedMethod;
        readonly string raiseChangingMethod;
        readonly string changedMethod;
        readonly string changingMethod;

        public string PropertyName { get => propertyName; }

        public static PropertyGenerator Create(ContextInfo info, INamedTypeSymbol classSymbol, IFieldSymbol fieldSymbol, string inpcedParameter, string inpcingParameter) {
            var fieldName = fieldSymbol.Name;
            var propertyName = PropertyHelper.CreatePropertyName(fieldName);
            if(propertyName == fieldName)
                info.Context.ReportInvalidPropertyName(fieldSymbol, propertyName);

            var type = fieldSymbol.Type;
            var changedMethod = PropertyHelper.GetChangedMethod(info, classSymbol, fieldSymbol, propertyName, type);
            var changingMethod = PropertyHelper.GetChangingMethod(info, classSymbol, fieldSymbol, propertyName, type);

            if(propertyName == fieldName || changedMethod == null || changingMethod == null)
                return null;
            return new PropertyGenerator(info, fieldSymbol, type, propertyName, fieldName, inpcedParameter, inpcingParameter, changedMethod, changingMethod);
        }
        public void GetSourceCode(StringBuilder source, int tabs) {
            if(!string.IsNullOrEmpty(attributesList))
                source.AppendLine(attributesList.AddTabs(tabs));
            source.AppendLine($"public {virtuality}{type} {propertyName} {{".AddTabs(tabs));
            source.AppendLine($"get => {fieldName};".AddTabs(tabs + 1));

            if(!string.IsNullOrEmpty(setterAttribute))
                source.AppendLine(setterAttribute.AddTabs(tabs + 1));

            source.AppendLine($"{setterAccessModifier}set {{".AddTabs(tabs + 1));
            source.AppendLine($"if(EqualityComparer<{type}>.Default.Equals({fieldName}, value)) return;".AddTabs(tabs + 2));

            if(!string.IsNullOrEmpty(raiseChangingMethod))
                source.AppendLine(raiseChangingMethod.AddTabs(tabs + 2));
            if(!string.IsNullOrEmpty(changingMethod))
                source.AppendLine(changingMethod.AddTabs(tabs + 2));

            if(!string.IsNullOrEmpty(changedMethod) && !changedMethod.EndsWith("();"))
                source.AppendLine($"var oldValue = {fieldName};".AddTabs(tabs + 2));
            source.AppendLine($"{fieldName} = value;".AddTabs(tabs + 2));

            if(!string.IsNullOrEmpty(raiseChangedMethod))
                source.AppendLine(raiseChangedMethod.AddTabs(tabs + 2));
            if(!string.IsNullOrEmpty(changedMethod))
                source.AppendLine(changedMethod.AddTabs(tabs + 2));

            source.AppendLine("}".AddTabs(tabs + 1));
            source.AppendLine("}".AddTabs(tabs));
        }

        PropertyGenerator(ContextInfo info, IFieldSymbol fieldSymbol, ITypeSymbol type, string propertyName, string fieldName, string inpcedParameter, string inpcingParameter, string changedMethod, string changingMethod) {
            attributesList = PropertyHelper.GetAttributesList(info.Compilation, fieldSymbol);
            setterAccessModifier = PropertyHelper.GetSetterAccessModifierValue(fieldSymbol, info.PropertyAttributeSymbol);

            var isVirtual = PropertyHelper.GetIsVirtualValue(fieldSymbol, info.PropertyAttributeSymbol);
            virtuality = isVirtual ? "virtual " : string.Empty;

            var nullableAnnotation = PropertyHelper.GetNullableAnnotation(type);
            this.type = type.WithNullableAnnotation(nullableAnnotation).ToDisplayStringNullable();
            this.propertyName = propertyName;
            this.fieldName = fieldName == "value" ? "this.value" : fieldName;

            var isNonNullableReferenceType = type.IsReferenceType && type.NullableAnnotation == NullableAnnotation.NotAnnotated;
            if(isNonNullableReferenceType && PropertyHelper.HasMemberNotNullAttribute(info.Compilation))
                setterAttribute = $"[System.Diagnostics.CodeAnalysis.MemberNotNull(nameof({fieldName}))]";

            if(inpcedParameter == "eventargs")
                raiseChangedMethod = $"RaisePropertyChanged({propertyName}ChangedEventArgs);";
            else if(inpcedParameter == "string")
                raiseChangedMethod = $"RaisePropertyChanged(nameof({propertyName}));";

            if(inpcingParameter == "eventargs")
                raiseChangingMethod = $"RaisePropertyChanging({propertyName}ChangingEventArgs);";
            else if(inpcingParameter == "string")
                raiseChangingMethod = $"RaisePropertyChanging(nameof({propertyName}));";

            this.changedMethod = changedMethod;
            this.changingMethod = changingMethod;
        }
    }
}
