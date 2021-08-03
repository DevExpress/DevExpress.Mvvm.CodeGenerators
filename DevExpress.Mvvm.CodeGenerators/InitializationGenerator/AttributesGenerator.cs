namespace DevExpress.Mvvm.CodeGenerators {
    public static class AttributesGenerator {
        static readonly string viewModelAttribute = "GenerateViewModelAttribute";
        static readonly string propertyAttribute = "GeneratePropertyAttribute";
        static readonly string commandAttribute = "GenerateCommandAttribute";

        public static readonly string ViewModelAttributeFullName = $"{InitializationGenerator.Namespace}.{viewModelAttribute}";
        public static readonly string PropertyAttributeFullName = $"{InitializationGenerator.Namespace}.{propertyAttribute}";
        public static readonly string CommandAttributeFullName = $"{InitializationGenerator.Namespace}.{commandAttribute}";

        public static string ImplementIDEI { get => "ImplementIDataErrorInfo"; }
        public static string ImplementINPCing { get => "ImplementINotifyPropertyChanging"; }
        public static string ImplementISPVM { get => "ImplementISupportParentViewModel"; }
        public static string ImplementISS { get => "ImplementISupportServices"; }

        public static string IsVirtual { get => "IsVirtual"; }
        public static string OnChangedMethod { get => "OnChangedMethod"; }
        public static string OnChangingMethod { get => "OnChangingMethod"; }
        public static string SetterAccessModifier { get => "SetterAccessModifier"; }

        public static string AllowMultipleExecution { get => "AllowMultipleExecution"; }
        public static string UseCommandManager { get => "UseCommandManager"; }
        public static string CanExecuteMethod { get => "CanExecuteMethod"; }
        public static string CommandName { get => "Name"; }


        static readonly string UseCommandManagerProperty = 
        $@"/// <summary>
        ///     Specifies the <b>useCommandManager</b> parameter in the <b>Command</b> constructor. The default value is <see langword=""true""/>.
        /// </summary>
        public bool {UseCommandManager} {{ get; set; }}";
        static readonly string ImplementIDEIProperty = 
        $@"/// <summary>
        ///     Implements 
        /// <see href=""https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.idataerrorinfo?view=net-5.0"">
        ///     IDataErrorInfo﻿
        /// </see>
        ///     that allows you to validate data.
        /// </summary>
        public bool {ImplementIDEI} {{ get; set; }}";

        public static string GetSourceCode(bool isWinUI) =>
$@"    /// <summary>
    ///     Applies to a class. Indicates that the source generator should process this class and produce View Model boilerplate code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class {viewModelAttribute} : Attribute {{
        {(isWinUI ? null : ImplementIDEIProperty)}
        /// <summary>
        ///     Implements 
        /// <see href=""https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanging?view=net-5.0"">
        ///     INotifyPropertyChanging﻿.
        /// </see>
        /// </summary>
        public bool {ImplementINPCing} {{ get; set; }}
        /// <summary>
        ///     Implements 
        /// <see href=""https://docs.devexpress.com/CoreLibraries/DevExpress.Mvvm.ISupportParentViewModel"">
        ///     ISupportParentViewModel
        /// </see>
        ///     that allows you to establish a parent-child relationship between View Models.
        /// </summary>
        public bool {ImplementISPVM} {{ get; set; }}
        /// <summary>
        ///     Implements 
        /// <see href=""https://docs.devexpress.com/CoreLibraries/DevExpress.Mvvm.ISupportServices"" >
        ///     ISupportServices
        /// </see>
        ///     that allows you to include the
        /// <see href=""https://docs.devexpress.com/WPF/17444/mvvm-framework/services/getting-started"">
        ///     Services
        /// </see>
        ///     mechanism to your View Model.
        /// </summary>
        public bool {ImplementISS} {{ get; set; }}
    }}
    /// <summary>
    ///     Applies to a field. The source generator produces boilerplate code for the property getter and setter based on the field declaration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    class {propertyAttribute} : Attribute {{
        /// <summary>
        ///     Assigns a virtual modifier to the property.
        /// </summary>
        public bool {IsVirtual} {{ get; set; }}
        /// <summary>
        ///     Specifies the name of the method invoked after the property value is changed.<br/>
        ///     If the property is not specified, the method’s name should follow the <b>On[PropertyName]Changed</b> pattern.
        /// </summary>
        public string? {OnChangedMethod} {{ get; set; }}
        /// <summary>
        ///     Specifies the name of the method invoked when the property value is changing.<br/>
        ///     If the property is not specified, the method’s name should follow the <b>On[PropertyName]Changing</b> pattern.
        /// </summary>
        public string? {OnChangingMethod} {{ get; set; }}
        /// <summary>
        ///     Specifies an access modifier for a set accessor. The default value is the same as a property’s modifier.<br/>
        ///     Available values: <i>Public, Private, Protected, Internal, ProtectedInternal, PrivateProtected.</i>
        /// </summary>
        public AccessModifier {SetterAccessModifier} {{ get; set; }}
    }}

    /// <summary>
    ///     Applies to a method. The source generator produces boilerplate code for a Command based on this method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    class {commandAttribute} : Attribute {{
        /// <summary>
        ///     Specifies the <b>allowMultipleExecution</b> parameter in the <b>AsyncCommand</b> constructor. The default value is <see langword=""false""/>.
        /// </summary>
        public bool {AllowMultipleExecution} {{ get; set; }}
        {(isWinUI ? null : UseCommandManagerProperty)}
        /// <summary>
        ///     Specifies a custom <b>CanExecute</b> method name. If the property is not specified, the method’s name should follow the <b>Can[ActionName]</b> pattern.
        /// </summary>
        public string? {CanExecuteMethod} {{ get; set; }}
        /// <summary>
        ///     Specifies a custom <b>Command</b> name. The default value is <b>[ActionName]Command</b>.
        /// </summary>
        public string? {CommandName} {{ get; set; }}
    }}";
    }
}
