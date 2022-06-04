using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace DevExpress.Mvvm.CodeGenerators {
    class INPCInfo {
        readonly bool hasImplementation;
        readonly bool hasImplementationInCurrentClass;

        public bool HasAttribute { get; }
        public bool HasMethodWithEventArgsPrefix { get; }
        public bool HasMethodWithStringPrefix { get; }
        public string RaiseMethodImplementation { get; }

        public static INPCInfo GetINPCedInfo(ContextInfo info, INamedTypeSymbol classSymbol, SupportedMvvm mvvm) =>
            new INPCInfo(classSymbol,
                         info.INPCedSymbol,
                         symbol => AttributeHelper.HasAttribute(symbol, info.GetFrameworkAttributes(mvvm).ViewModelAttributeSymbol),
                         "PropertyChanged",
                         "System.ComponentModel.PropertyChangedEventArgs",
                         $"void {mvvm.GetRasiePrefix().ToStringValue()}PropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);",
                         mvvm.GetRasiePrefix());
        public static INPCInfo GetINPCingInfo(ContextInfo info, INamedTypeSymbol classSymbol, SupportedMvvm mvvm) =>
            new INPCInfo(classSymbol,
                         info.INPCingSymbol,
                         symbol => AttributeHelper.HasAttribute(symbol, info.GetFrameworkAttributes(mvvm).ViewModelAttributeSymbol) &&
                                   AttributeHelper.GetPropertyActualValue(symbol, info.GetFrameworkAttributes(mvvm).ViewModelAttributeSymbol, AttributesGenerator.ImplementINPCing, false),
                         "PropertyChanging",
                         "System.ComponentModel.PropertyChangingEventArgs",
                         $"void {mvvm.GetRasiePrefix().ToStringValue()}PropertyChanging(PropertyChangingEventArgs e) => PropertyChanging?.Invoke(this, e);",
                         mvvm.GetRasiePrefix());

        public bool HasNoImplementation() =>
            HasAttribute && !hasImplementation;
        public bool ShouldImplementRaiseMethod() =>
            HasAttribute && !HasMethodWithEventArgsPrefix && (!hasImplementation || hasImplementationInCurrentClass);

        INPCInfo(INamedTypeSymbol classSymbol, INamedTypeSymbol interfaceSymbol, Func<INamedTypeSymbol, bool> checkAttribute, string methodName, string eventArgsParameter, string raiseMethodImplementation, RaiseMethodPrefix prefix) {
            HasAttribute = checkAttribute(classSymbol);
            hasImplementation = ClassHelper.IsInterfaceImplementedInCurrentClass(classSymbol, interfaceSymbol);
            if(HasAttribute && hasImplementation)
                hasImplementationInCurrentClass = true;

            HasMethodWithEventArgsPrefix = HasRaiseMethod(classSymbol, methodName, eventArgsParameter, true, prefix);
            HasMethodWithStringPrefix = HasRaiseMethod(classSymbol, methodName, "string", true, prefix);

            bool isRaiseMethodGenerated = false;
            for(INamedTypeSymbol parent = classSymbol.BaseType!; parent != null; parent = parent.BaseType!) {
                bool hasAttribute = checkAttribute(parent);
                bool hasImplementation = ClassHelper.IsInterfaceImplementedInCurrentClass(parent, interfaceSymbol);
                if(hasAttribute || hasImplementation)
                    this.hasImplementation = true;
                if(hasImplementation)
                    isRaiseMethodGenerated = false;
                if(hasAttribute)
                    isRaiseMethodGenerated = true;

                if(!HasMethodWithEventArgsPrefix)
                    HasMethodWithEventArgsPrefix = HasRaiseMethod(parent, methodName, eventArgsParameter, false, prefix);
                if(!HasMethodWithStringPrefix)
                    HasMethodWithStringPrefix = HasRaiseMethod(parent, methodName, "string", false, prefix);
            }
            if(isRaiseMethodGenerated)
                HasMethodWithEventArgsPrefix = true;
            RaiseMethodImplementation = raiseMethodImplementation;
        }

        bool HasRaiseMethod(INamedTypeSymbol classSymbol, string methodName, string parameterType, bool ignorePrivateAccessibility, RaiseMethodPrefix prefix) {
            return classSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Any(symbol => {
                    return (symbol.DeclaredAccessibility != Accessibility.Private || ignorePrivateAccessibility) &&
                        symbol.ReturnsVoid &&
                        symbol.Name == prefix.ToStringValue() + methodName &&
                        symbol.Parameters.Length == 1 && symbol.Parameters.First().Type.ToDisplayString(NullableFlowState.None) == parameterType;
                });
        }
    }
}
