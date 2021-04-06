using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace DevExpress.Mvvm.CodeGenerators {
    class INPCInfo {
        readonly bool hasImplementation;
        readonly bool hasImplementationInCurrentClass;

        public bool HasAttribute { get; }
        public bool HasRaiseMethodWithEventArgsParameter { get; }
        public bool HasRaiseMethodWithStringParameter { get; }
        public string RaiseMethodImplementation { get; }

        public static INPCInfo GetINPCedInfo(ContextInfo info, INamedTypeSymbol classSymbol) =>
            new INPCInfo(classSymbol,
                         info.INPCedSymbol,
                         symbol => AttributeHelper.HasAttribute(symbol, info.ViewModelAttributeSymbol),
                         "RaisePropertyChanged",
                         "System.ComponentModel.PropertyChangedEventArgs",
                         "protected void RaisePropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);");
        public static INPCInfo GetINPCingInfo(ContextInfo info, INamedTypeSymbol classSymbol) =>
            new INPCInfo(classSymbol,
                         info.INPCingSymbol,
                         symbol => AttributeHelper.HasAttribute(symbol, info.ViewModelAttributeSymbol) &&
                                   AttributeHelper.GetPropertyActualValue(symbol, info.ViewModelAttributeSymbol, AttributesGenerator.ImplementINPCing, false),
                         "RaisePropertyChanging",
                         "System.ComponentModel.PropertyChangingEventArgs",
                         "protected void RaisePropertyChanging(PropertyChangingEventArgs e) => PropertyChanging?.Invoke(this, e);");

        public bool HasNoImplementation() =>
            HasAttribute && !hasImplementation;
        public bool CanImplementRaiseMethod() =>
            HasAttribute && !HasRaiseMethodWithEventArgsParameter && (!hasImplementation || hasImplementationInCurrentClass);

        INPCInfo(INamedTypeSymbol classSymbol, INamedTypeSymbol interfaceSymbol, Func<INamedTypeSymbol, bool> checkAttribute, string methodName, string eventArgsParameter, string raiseMethodImplementation) {
            HasAttribute = checkAttribute(classSymbol);
            hasImplementation = ClassHelper.IsInterfaceImplemented(classSymbol, interfaceSymbol);
            if(HasAttribute && hasImplementation)
                hasImplementationInCurrentClass = true;

            HasRaiseMethodWithEventArgsParameter = HasMethod(classSymbol, methodName, eventArgsParameter, true);
            HasRaiseMethodWithStringParameter = HasMethod(classSymbol, methodName, "string", true);

            var isRaiseMethodGenerated = false;
            for(var parent = classSymbol.BaseType; parent != null; parent = parent.BaseType) {
                var hasAttribute = checkAttribute(parent);
                var hasImplementation = ClassHelper.IsInterfaceImplemented(parent, interfaceSymbol);
                if(hasAttribute || hasImplementation)
                    this.hasImplementation = true;
                if(hasImplementation)
                    isRaiseMethodGenerated = false;
                if(hasAttribute)
                    isRaiseMethodGenerated = true;

                if(!HasRaiseMethodWithEventArgsParameter)
                    HasRaiseMethodWithEventArgsParameter = HasMethod(parent, methodName, eventArgsParameter);
                if(!HasRaiseMethodWithStringParameter)
                    HasRaiseMethodWithStringParameter = HasMethod(parent, methodName, "string");
            }
            if(isRaiseMethodGenerated)
                HasRaiseMethodWithEventArgsParameter = true;
            RaiseMethodImplementation = raiseMethodImplementation;
        }
        bool HasMethod(INamedTypeSymbol classSymbol, string methodName, string parameterType, bool ignorePrivateAccessibility = false) =>
            CommandHelper.GetMethods(classSymbol,
                                     symbol => (symbol.DeclaredAccessibility != Accessibility.Private || ignorePrivateAccessibility) &&
                                               symbol.ReturnsVoid &&
                                               symbol.Name == methodName &&
                                               symbol.Parameters.Length == 1 && symbol.Parameters.First().Type.ToDisplayStringNullable() == parameterType)
                         .Any();
    }
}
