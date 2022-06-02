using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace DevExpress.Mvvm.CodeGenerators {
    class INPCInfo {
        readonly bool hasImplementation;
        readonly bool hasImplementationInCurrentClass;

        public bool HasAttribute { get; }
        public RaiseMethodPrefix? RaiseMethodWithEventArgsPrefix { get; }
        public RaiseMethodPrefix? RaiseMethodWithStringPrefix { get; }
        public string RaiseMethodImplementation { get; }

        public static INPCInfo GetINPCedInfo(ContextInfo info, INamedTypeSymbol classSymbol, SupportedMvvm mvvm) =>
            new INPCInfo(classSymbol,
                         info.INPCedSymbol,
                         symbol => AttributeHelper.HasAttribute(symbol, info.GetFrameworkAttributes(mvvm).ViewModelAttributeSymbol),
                         "PropertyChanged",
                         "System.ComponentModel.PropertyChangedEventArgs",
                         "void RaisePropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);");
        public static INPCInfo GetINPCingInfo(ContextInfo info, INamedTypeSymbol classSymbol, SupportedMvvm mvvm) =>
            new INPCInfo(classSymbol,
                         info.INPCingSymbol,
                         symbol => AttributeHelper.HasAttribute(symbol, info.GetFrameworkAttributes(mvvm).ViewModelAttributeSymbol) &&
                                   AttributeHelper.GetPropertyActualValue(symbol, info.GetFrameworkAttributes(mvvm).ViewModelAttributeSymbol, AttributesGenerator.ImplementINPCing, false),
                         "PropertyChanging",
                         "System.ComponentModel.PropertyChangingEventArgs",
                         "void RaisePropertyChanging(PropertyChangingEventArgs e) => PropertyChanging?.Invoke(this, e);");

        public bool HasNoImplementation() =>
            HasAttribute && !hasImplementation;
        public bool ShouldImplementRaiseMethod() =>
            HasAttribute && RaiseMethodWithEventArgsPrefix == null && (!hasImplementation || hasImplementationInCurrentClass);

        INPCInfo(INamedTypeSymbol classSymbol, INamedTypeSymbol interfaceSymbol, Func<INamedTypeSymbol, bool> checkAttribute, string methodName, string eventArgsParameter, string raiseMethodImplementation) {
            HasAttribute = checkAttribute(classSymbol);
            hasImplementation = ClassHelper.IsInterfaceImplementedInCurrentClass(classSymbol, interfaceSymbol);
            if(HasAttribute && hasImplementation)
                hasImplementationInCurrentClass = true;

            RaiseMethodWithEventArgsPrefix = TryGetRaiseMethodPrefix(classSymbol, methodName, eventArgsParameter, true);
            RaiseMethodWithStringPrefix = TryGetRaiseMethodPrefix(classSymbol, methodName, "string", true);

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

                if(RaiseMethodWithEventArgsPrefix == null)
                    RaiseMethodWithEventArgsPrefix = TryGetRaiseMethodPrefix(parent, methodName, eventArgsParameter);
                if(RaiseMethodWithStringPrefix == null)
                    RaiseMethodWithStringPrefix = TryGetRaiseMethodPrefix(parent, methodName, "string");
            }
            if(isRaiseMethodGenerated)
                RaiseMethodWithEventArgsPrefix = RaiseMethodPrefix.Raise;
            RaiseMethodImplementation = raiseMethodImplementation;
        }
        RaiseMethodPrefix? TryGetRaiseMethodPrefix(INamedTypeSymbol classSymbol, string methodName, string parameterType, bool ignorePrivateAccessibility = false) {
            return classSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Select(symbol => {
                    var isCandidate = (symbol.DeclaredAccessibility != Accessibility.Private || ignorePrivateAccessibility) &&
                        symbol.ReturnsVoid &&
                        symbol.Parameters.Length == 1 && symbol.Parameters.First().Type.ToDisplayString(NullableFlowState.None) == parameterType;
                    if(isCandidate) {
                        if(symbol.Name == "Raise" + methodName) return RaiseMethodPrefix.Raise;
                        if(symbol.Name == "On" + methodName) return RaiseMethodPrefix.On;
                    }
                    return default(RaiseMethodPrefix?);
                })
                .FirstOrDefault();
        }
    }
}
