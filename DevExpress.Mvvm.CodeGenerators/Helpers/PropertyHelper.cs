using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    static class PropertyHelper {
        static readonly string nameofIsVirtual = AttributesGenerator.IsVirtual;
        static readonly string nameofChangedMethod = AttributesGenerator.OnChangedMethod;
        static readonly string nameofChangingMethod = AttributesGenerator.OnChangingMethod;
        static readonly string nameofSetterAccessModifier = AttributesGenerator.SetterAccessModifier;

        public static string CreatePropertyName(string fieldName) => fieldName.TrimStart('_').FirstToUpperCase();
        public static bool GetIsVirtualValue(IFieldSymbol fieldSymbol, INamedTypeSymbol propertySymbol) =>
            AttributeHelper.GetPropertyActualValue(fieldSymbol, propertySymbol, nameofIsVirtual, false);
        public static string GetChangedMethod(ContextInfo info, INamedTypeSymbol classSymbol, IFieldSymbol fieldSymbol, string propertyName, ITypeSymbol fieldType) {
            var methodName = GetChangedMethodName(fieldSymbol, info.PropertyAttributeSymbol);
            return GetMethod(info, classSymbol, fieldSymbol, methodName, "On" + propertyName + "Changed", "oldValue", fieldType);
        }
        public static string GetChangingMethod(ContextInfo info, INamedTypeSymbol classSymbol, IFieldSymbol fieldSymbol, string propertyName, ITypeSymbol fieldType) {
            var methodName = GetChangingMethodName(fieldSymbol, info.PropertyAttributeSymbol);
            return GetMethod(info, classSymbol, fieldSymbol, methodName, "On" + propertyName + "Changing", "value", fieldType);
        }
        public static string GetSetterAccessModifierValue(IFieldSymbol fieldSymbol, INamedTypeSymbol propertySymbol) {
            var enumIndex = AttributeHelper.GetPropertyActualValue(fieldSymbol, propertySymbol, nameofSetterAccessModifier, 0);
            return AccessModifierGenerator.GetCodeRepresentation(enumIndex);
        }
        public static string GetAttributesList(IFieldSymbol fieldSymbol) {
            var attributeList = fieldSymbol.GetAttributes();
            if(attributeList.Length == 1)
                return string.Empty;
            return "[" + attributeList.Select(atr => atr.ToString())
                                      .Where(str => !str.StartsWith(AttributesGenerator.PropertyAttributeFullName))
                                      .ConcatToString("]" + Environment.NewLine + "[") + "]";
        }
        public static NullableAnnotation GetNullableAnnotation(ITypeSymbol type) =>
            type.IsReferenceType && type.NullableAnnotation == NullableAnnotation.None
                ? NullableAnnotation.Annotated
                : type.NullableAnnotation;
        public static bool HasMemberNotNullAttribute(Compilation compilation) =>
            compilation.References.Select(compilation.GetAssemblyOrModuleSymbol)
                                  .OfType<IAssemblySymbol>()
                                  .Select(assemblySymbol => assemblySymbol.GetTypeByMetadataName("System.Diagnostics.CodeAnalysis.MemberNotNullAttribute"))
                                  .Where(symbol => symbol != null && symbol.ContainingModule.ToDisplayString() == "System.Runtime.dll")
                                  .Any();
        public static bool IsСompatibleType(ITypeSymbol parameterType, ITypeSymbol type) {
            if(ToNotAnnotatedDisplayString(parameterType) != ToNotAnnotatedDisplayString(type))
                return false;
            if(parameterType.IsValueType)
                return parameterType.NullableAnnotation == NullableAnnotation.Annotated || type.NullableAnnotation != NullableAnnotation.Annotated;
            return parameterType.NullableAnnotation != NullableAnnotation.NotAnnotated || type.NullableAnnotation == NullableAnnotation.NotAnnotated;
        }
         static string ToNotAnnotatedDisplayString(ITypeSymbol type) {
            var typeAsString = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return typeAsString.EndsWith("?") ? typeAsString.Remove(typeAsString.Length - 1, 1) : typeAsString;
        }

        static string GetChangedMethodName(IFieldSymbol fieldSymbol, INamedTypeSymbol propertySymbol) =>
            AttributeHelper.GetPropertyActualValue(fieldSymbol, propertySymbol, nameofChangedMethod, (string)null);
        static string GetChangingMethodName(IFieldSymbol fieldSymbol, INamedTypeSymbol propertySymbol) =>
            AttributeHelper.GetPropertyActualValue(fieldSymbol, propertySymbol, nameofChangingMethod, (string)null);
        static string GetMethod(ContextInfo info, INamedTypeSymbol classSymbol, IFieldSymbol fieldSymbol, string methodName, string defaultMethodName, string parameterName, ITypeSymbol fieldType) {
            var hasMethodName = methodName != null;
            if(!hasMethodName)
                methodName = defaultMethodName;

            var methods = GetOnChangedMethods(classSymbol, methodName, fieldType);
            if(methods.Count() == 2) {
                info.Context.ReportTwoSuitableMethods(classSymbol, fieldSymbol, methodName, fieldType.ToDisplayStringNullable());
                return methodName + "(" + parameterName + ");";
            }
            if(methods.Count() == 1) {
                if(methods.First().Parameters.Length == 0)
                    return methodName + "();";
                return methodName + "(" + parameterName + ");";
            }

            if(!hasMethodName)
                return string.Empty;
            info.Context.ReportOnChangedMethodNotFound(fieldSymbol, methodName, fieldType.ToDisplayStringNullable(), CommandHelper.GetMethods(classSymbol, methodName));
            return null;
        }
        static IEnumerable<IMethodSymbol> GetOnChangedMethods(INamedTypeSymbol classSymbol, string methodName, ITypeSymbol fieldType) =>
            CommandHelper.GetMethods(classSymbol,
                                     methodSymbol => methodSymbol.ReturnsVoid && methodSymbol.Name == methodName && methodSymbol.Parameters.Length < 2 &&
                                                    (methodSymbol.Parameters.Length == 0 || IsСompatibleType(methodSymbol.Parameters.First().Type, fieldType)));
    }
}
