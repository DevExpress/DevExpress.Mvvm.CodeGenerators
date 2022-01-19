using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DevExpress.Mvvm.CodeGenerators {
    static class PropertyGenerator {
        public static string? Generate(SourceBuilder source, ContextInfo info, INamedTypeSymbol classSymbol, IFieldSymbol fieldSymbol, ChangeEventRaiseMode? changedEventRaiseMode, ChangeEventRaiseMode? changingEventRaiseMode, SupportedMvvm mvvm) {
            var propertyAttributeSymbol = info.GetFrameworkAttributes(mvvm).PropertyAttributeSymbol;
            string propertyName = PropertyHelper.CreatePropertyName(fieldSymbol.Name);
            if(propertyName == fieldSymbol.Name)
                info.Context.ReportInvalidPropertyName(fieldSymbol, propertyName);

            string? changedMethod = PropertyHelper.GetChangedMethod(info, classSymbol, fieldSymbol, propertyName, fieldSymbol.Type, mvvm);
            string? changingMethod = PropertyHelper.GetChangingMethod(info, classSymbol, fieldSymbol, propertyName, fieldSymbol.Type, mvvm);

            if(propertyName == fieldSymbol.Name || string.IsNullOrEmpty(propertyName) || changedMethod == null || changingMethod == null)
                return null;

            CSharpSyntaxNode fieldSyntaxNode = (CSharpSyntaxNode)fieldSymbol.DeclaringSyntaxReferences[0].GetSyntax().Parent!.Parent!;
            XMLCommentHelper.AppendComment(source, fieldSyntaxNode);

            AttributeHelper.AppendAttributesList(source, fieldSymbol);

            bool isVirtual = PropertyHelper.GetIsVirtualValue(fieldSymbol, propertyAttributeSymbol);
            string virtuality = isVirtual ? "virtual " : string.Empty;
            string typeName = fieldSymbol.Type.WithNullableAnnotation(PropertyHelper.GetNullableAnnotation(fieldSymbol.Type)).ToDisplayStringNullable();
            string fieldName = fieldSymbol.Name == "value" ? "this.value" : fieldSymbol.Name;
            source.Append("public ").Append(virtuality).Append(typeName).Append(' ').Append(propertyName).AppendLine(" {");
            source.Tab.Append("get => ").Append(fieldName).AppendLine(";");

            AppendSetterAttribute(source.Tab, info, fieldSymbol, fieldName);

            string setterAccessModifier = PropertyHelper.GetSetterAccessModifierValue(fieldSymbol, propertyAttributeSymbol);
            source.Tab.Append(setterAccessModifier).AppendLine("set {");
            source.Tab.Tab.Append("if(EqualityComparer<").Append(typeName).Append(">.Default.Equals(").Append(fieldName).AppendLine(", value)) return;");

            AppendRaiseChangingMethod(source.Tab.Tab, changingEventRaiseMode, propertyName);
            if(!string.IsNullOrEmpty(changingMethod))
                source.Tab.Tab.AppendLine(changingMethod);

            if(!string.IsNullOrEmpty(changedMethod) && !changedMethod.EndsWith("();"))
                source.Tab.Tab.Append("var oldValue = ").Append(fieldName).AppendLine(";");
            source.Tab.Tab.Append(fieldName).AppendLine(" = value;");

            AppendRaiseChangedMethod(source.Tab.Tab, changedEventRaiseMode, propertyName);

            if(!string.IsNullOrEmpty(changedMethod))
                source.Tab.Tab.AppendLine(changedMethod);

            source.Tab.AppendLine("}");
            source.AppendLine("}");

            return propertyName;
        }

        static void AppendRaiseChangingMethod(SourceBuilder source, ChangeEventRaiseMode? eventRaiseMode, string propertyName) {
            if(eventRaiseMode == ChangeEventRaiseMode.EventArgs)
                source.Append("RaisePropertyChanging(").Append(propertyName).AppendLine("ChangingEventArgs);");
            if(eventRaiseMode == ChangeEventRaiseMode.PropertyName)
                source.Append("RaisePropertyChanging(nameof(").Append(propertyName).AppendLine("));");
        }

        static void AppendRaiseChangedMethod(SourceBuilder source, ChangeEventRaiseMode? eventRaiseMode, string propertyName) {
            if(eventRaiseMode == ChangeEventRaiseMode.EventArgs)
                source.Append("RaisePropertyChanged(").Append(propertyName).AppendLine("ChangedEventArgs);");
            if(eventRaiseMode == ChangeEventRaiseMode.PropertyName)
                source.Append("RaisePropertyChanged(nameof(").Append(propertyName).AppendLine("));");
        }

        static void AppendSetterAttribute(SourceBuilder source, ContextInfo info, IFieldSymbol fieldSymbol, string fieldName) {
            bool isNonNullableReferenceType = fieldSymbol.Type.IsReferenceType && fieldSymbol.Type.NullableAnnotation == NullableAnnotation.NotAnnotated;
            if(isNonNullableReferenceType && PropertyHelper.HasMemberNotNullAttribute(info.Compilation))
                source.Append("[System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(").Append(fieldName).AppendLine("))]");
        }
    }
}
