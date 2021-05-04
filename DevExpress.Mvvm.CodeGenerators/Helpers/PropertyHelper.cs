using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public static string GetAttributesList(Compilation compilation, IFieldSymbol fieldSymbol) {
            if(fieldSymbol.GetAttributes().Length == 1)
                return string.Empty;

            var attributeLists = ((FieldDeclarationSyntax)fieldSymbol.DeclaringSyntaxReferences[0].GetSyntax().Parent.Parent).AttributeLists;
            var semanticModel = compilation.GetSemanticModel(attributeLists[0].Attributes[0].SyntaxTree);
            var attributeListsString = attributeLists
                .Select(listSyntax => listSyntax.Attributes
                                                .Where(syntax => GetAttributeFullName(semanticModel, syntax) != AttributesGenerator.PropertyAttributeFullName)
                                                .Select(syntax => AttributeConnectionString(semanticModel, syntax))
                                                .Where(str => !string.IsNullOrEmpty(str))
                                                .ConcatToString("," + Environment.NewLine))
                .Where(str => !string.IsNullOrEmpty(str))
                .ConcatToString("]" + Environment.NewLine + "[");

            if(string.IsNullOrEmpty(attributeListsString))
                return string.Empty;
            return "[" + attributeListsString + "]";
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
        public static bool IsSameType(ITypeSymbol parameterType, ITypeSymbol type) {
            if(parameterType.IsValueType)
                return parameterType.ToDisplayStringNullable() == type.ToDisplayStringNullable();
            if(parameterType.ToDisplayString(NullableFlowState.None) != type.ToDisplayString(NullableFlowState.None))
                return false;
            if(parameterType.IsReferenceType)
                return parameterType.NullableAnnotation != NullableAnnotation.NotAnnotated || type.NullableAnnotation == NullableAnnotation.NotAnnotated;
            return true;
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
                                                    (methodSymbol.Parameters.Length == 0 || IsSameType(methodSymbol.Parameters.First().Type, fieldType)));
        static string GetAttributeFullName(SemanticModel semanticModel, AttributeSyntax attributeSyntax) {
            var attributeSymbolInfo = semanticModel.GetSymbolInfo(attributeSyntax);
            return attributeSymbolInfo.Symbol?.ContainingSymbol.ToDisplayString() ??
                   attributeSymbolInfo.CandidateSymbols.FirstOrDefault()?.ToDisplayString() ??
                   string.Empty;
        }
        static string AttributeConnectionString(SemanticModel semanticModel, AttributeSyntax attributeSyntax) {
            var attributeFullName = GetAttributeFullName(semanticModel, attributeSyntax);
            var attributeSyntaxString = attributeSyntax.ToString();
            var attributeParameterListStartPosition = attributeSyntaxString.IndexOf('(');
            if(attributeParameterListStartPosition == -1)
                return attributeFullName;

            var attributeParameterList = attributeSyntaxString.Substring(attributeParameterListStartPosition)
                                                              .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                                                              .Select(str => str.TrimStart())
                                                              .ConcatToString(" ");
            return attributeFullName + attributeParameterList;
        }
    }
}
