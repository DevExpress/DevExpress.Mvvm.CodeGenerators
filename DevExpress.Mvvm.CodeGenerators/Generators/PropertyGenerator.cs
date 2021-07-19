using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    static class PropertyGenerator {
        public static string Generate(StringBuilder source, int tabs, ContextInfo info, INamedTypeSymbol classSymbol, IFieldSymbol fieldSymbol, ChangeEventRaiseMode? changedEventRaiseMode, ChangeEventRaiseMode? changingEventRaiseMode) {
            var propertyName = PropertyHelper.CreatePropertyName(fieldSymbol.Name);
            if(propertyName == fieldSymbol.Name)
                info.Context.ReportInvalidPropertyName(fieldSymbol, propertyName);

            var changedMethod = PropertyHelper.GetChangedMethod(info, classSymbol, fieldSymbol, propertyName, fieldSymbol.Type);
            var changingMethod = PropertyHelper.GetChangingMethod(info, classSymbol, fieldSymbol, propertyName, fieldSymbol.Type);

            if(propertyName == fieldSymbol.Name || changedMethod == null || changingMethod == null)
                return null;

            PropertyHelper.AppendAttributesList(source, fieldSymbol, tabs);

            var isVirtual = PropertyHelper.GetIsVirtualValue(fieldSymbol, info.PropertyAttributeSymbol);
            var virtuality = isVirtual ? "virtual " : string.Empty;
            var typeName = fieldSymbol.Type.WithNullableAnnotation(PropertyHelper.GetNullableAnnotation(fieldSymbol.Type)).ToDisplayStringNullable();
            var fieldName = fieldSymbol.Name == "value" ? "this.value" : fieldSymbol.Name;
            source.AppendTabs(tabs).Append("public ").Append(virtuality).Append(typeName).Append(' ').Append(propertyName).AppendLine(" {");
            source.AppendTabs(tabs + 1).Append("get => ").Append(fieldName).AppendLine(";");

            AppendSetterAttribute(source, info, fieldSymbol, fieldName, tabs + 1);

            var setterAccessModifier = PropertyHelper.GetSetterAccessModifierValue(fieldSymbol, info.PropertyAttributeSymbol);
            source.AppendTabs(tabs + 1).Append(setterAccessModifier).AppendLine("set {");
            source.AppendTabs(tabs + 2).Append("if(EqualityComparer<").Append(typeName).Append(">.Default.Equals(").Append(fieldName).AppendLine(", value)) return;");

            AppendRaiseChangingMethod(source, changingEventRaiseMode, propertyName, tabs + 2);
            if(!string.IsNullOrEmpty(changingMethod))
                source.AppendLineWithTabs(changingMethod, tabs + 2);

            if(!string.IsNullOrEmpty(changedMethod) && !changedMethod.EndsWith("();"))
                source.AppendTabs(tabs + 2).Append("var oldValue = ").Append(fieldName).AppendLine(";");
            source.AppendTabs(tabs + 2).Append(fieldName).AppendLine(" = value;");

            AppendRaiseChangedMethod(source, changedEventRaiseMode, propertyName, tabs + 2);

            if(!string.IsNullOrEmpty(changedMethod))
                source.AppendLineWithTabs(changedMethod, tabs + 2);

            source.AppendLineWithTabs("}", tabs + 1);
            source.AppendLineWithTabs("}", tabs);

            return propertyName;
        }

        static void AppendRaiseChangingMethod(StringBuilder source, ChangeEventRaiseMode? eventRaiseMode, string propertyName, int tabs) {
            source.AppendTabs(tabs);
            if(eventRaiseMode == ChangeEventRaiseMode.EventArgs)
                source.Append("RaisePropertyChanging(").Append(propertyName).AppendLine("ChangingEventArgs);");
            if(eventRaiseMode == ChangeEventRaiseMode.PropertyName)
                source.Append("RaisePropertyChanging(nameof(").Append(propertyName).AppendLine("));");
        }

        static void AppendRaiseChangedMethod(StringBuilder source, ChangeEventRaiseMode? eventRaiseMode, string propertyName, int tabs) {
            source.AppendTabs(tabs);
            if(eventRaiseMode == ChangeEventRaiseMode.EventArgs)
                source.Append("RaisePropertyChanged(").Append(propertyName).AppendLine("ChangedEventArgs);");
            if(eventRaiseMode == ChangeEventRaiseMode.PropertyName)
                source.Append("RaisePropertyChanged(nameof(").Append(propertyName).AppendLine("));");
        }

        static void AppendSetterAttribute(StringBuilder source, ContextInfo info, IFieldSymbol fieldSymbol, string fieldName, int tabs) {
            var isNonNullableReferenceType = fieldSymbol.Type.IsReferenceType && fieldSymbol.Type.NullableAnnotation == NullableAnnotation.NotAnnotated;
            if(isNonNullableReferenceType && PropertyHelper.HasMemberNotNullAttribute(info.Compilation))
                source.AppendTabs(tabs).Append("[System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(").Append(fieldName).AppendLine("))]");
        }
    }
}
