﻿namespace DevExpress.Mvvm.CodeGenerators {
    public static class AttributesGenerator {
        public static readonly string ViewModelAttributeFullName = $"{InitializationGenerator.Namespace}.GenerateViewModelAttribute";
        public static readonly string PropertyAttributeFullName = $"{InitializationGenerator.Namespace}.GeneratePropertyAttribute";
        public static readonly string CommandAttributeFullName = $"{InitializationGenerator.Namespace}.GenerateCommandAttribute";

        public const string ImplementIDEI = "ImplementIDataErrorInfo";
        public const string ImplementINPCing = "ImplementINotifyPropertyChanging";
        public const string ImplementISPVM = "ImplementISupportParentViewModel";
        public const string ImplementISS = "ImplementISupportServices";

        public const string IsVirtual = "IsVirtual";
        public const string OnChangedMethod = "OnChangedMethod";
        public const string OnChangingMethod = "OnChangingMethod";
        public const string SetterAccessModifier = "SetterAccessModifier";

        public const string AllowMultipleExecution = "AllowMultipleExecution";
        public const string UseCommandManager = "UseCommandManager";
        public const string CanExecuteMethod = "CanExecuteMethod";
        public const string CommandName = "Name";

        public static string GetSourceCode(bool isWinUI) => isWinUI ? winUISourceCode : commonSourceCode;

        const string commonSourceCode = @"    /// <summary>
    ///     Indicates that the View Model Code Generator should process this class and produce a View Model.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class GenerateViewModelAttribute : Attribute {
        /// <summary>
        ///     Implements
        ///     <see href=""https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.idataerrorinfo?view=net-5.0"">IDataErrorInfo﻿</see>
        ///     that allows you to validate data.
        /// </summary>
        public bool ImplementIDataErrorInfo { get; set; }
        /// <summary>
        ///     Implements
        ///     <see href=""https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanging?view=net-5.0"">INotifyPropertyChanging﻿.</see>
        /// </summary>
        public bool ImplementINotifyPropertyChanging { get; set; }
        /// <summary>
        ///     Implements
        ///     <see href=""https://docs.devexpress.com/CoreLibraries/DevExpress.Mvvm.ISupportParentViewModel"">ISupportParentViewModel</see>
        ///     that allows you to establish a parent-child relationship between View Models.
        /// </summary>
        public bool ImplementISupportParentViewModel { get; set; }
        /// <summary>
        ///     Implements
        ///     <see href=""https://docs.devexpress.com/CoreLibraries/DevExpress.Mvvm.ISupportServices"">ISupportServices</see>
        ///     that allows you to include the
        ///     <see href=""https://docs.devexpress.com/WPF/17444/mvvm-framework/services/getting-started"">Services</see>
        ///     mechanism to your View Model.
        /// </summary>
        public bool ImplementISupportServices { get; set; }
    }

    /// <summary>
    ///     Indicates that the View Model Code Generator should process this field and produce a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    class GeneratePropertyAttribute : Attribute {
        /// <summary>
        ///     Assigns a virtual modifier to the property.
        /// </summary>
        public bool IsVirtual { get; set; }
        /// <summary>
        ///     Specifies the name of the method invoked after the property value is changed.<br/>
        ///     If the property is not specified, the method’s name should follow the <b>On[PropertyName]Changed</b> pattern.
        /// </summary>
        public string? OnChangedMethod { get; set; }
        /// <summary>
        ///     Specifies the name of the method invoked when the property value is changing.<br/>
        ///     If the property is not specified, the method’s name should follow the <b>On[PropertyName]Changing</b> pattern.
        /// </summary>
        public string? OnChangingMethod { get; set; }
        /// <summary>
        ///     Specifies an access modifier for a set accessor. The default value is the same as a property’s modifier.<br/>
        ///     Available values: <i>Public, Private, Protected, Internal, ProtectedInternal, PrivateProtected.</i>
        /// </summary>
        public AccessModifier SetterAccessModifier { get; set; }
    }

    /// <summary>
    ///     Indicates that the View Model Code Generator should process this method and produce a Command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    class GenerateCommandAttribute : Attribute {
        /// <summary>
        ///     Specifies the <b>allowMultipleExecution</b> parameter in the <b>AsyncCommand</b> constructor. The default value is <see langword=""false""/>.
        /// </summary>
        public bool AllowMultipleExecution { get; set; }
        /// <summary>
        ///     Specifies the <b>useCommandManager</b> parameter in the <b>Command</b> constructor. The default value is <see langword=""true""/>.
        /// </summary>
        public bool UseCommandManager { get; set; }
        /// <summary>
        ///     Specifies a custom <b>CanExecute</b> method name. If the property is not specified, the method’s name should follow the <b>Can[ActionName]</b> pattern.
        /// </summary>
        public string? CanExecuteMethod { get; set; }
        /// <summary>
        ///     Specifies a custom <b>Command</b> name. The default value is <b>[ActionName]Command</b>.
        /// </summary>
        public string? Name { get; set; }
    }";
        const string winUISourceCode = @"    /// <summary>
    ///     Applies to a class. Indicates that the source generator should process this class and produce View Model boilerplate code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class GenerateViewModelAttribute : Attribute {
        /// <summary>
        ///     Implements
        ///     <see href=""https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanging?view=net-5.0"">INotifyPropertyChanging﻿.</see>
        /// </summary>
        public bool ImplementINotifyPropertyChanging { get; set; }
        /// <summary>
        ///     Implements
        ///     <see href=""https://docs.devexpress.com/CoreLibraries/DevExpress.Mvvm.ISupportParentViewModel"">ISupportParentViewModel</see>
        ///     that allows you to establish a parent-child relationship between View Models.
        /// </summary>
        public bool ImplementISupportParentViewModel { get; set; }
        /// <summary>
        ///     Implements
        ///     <see href=""https://docs.devexpress.com/CoreLibraries/DevExpress.Mvvm.ISupportServices"">ISupportServices</see>
        ///     that allows you to include the
        ///     <see href=""https://docs.devexpress.com/WPF/17444/mvvm-framework/services/getting-started"">Services</see>
        ///     mechanism to your View Model.
        /// </summary>
        public bool ImplementISupportServices { get; set; }
    }

    /// <summary>
    ///     Applies to a field. The source generator produces boilerplate code for the property getter and setter based on the field declaration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    class GeneratePropertyAttribute : Attribute {
        /// <summary>
        ///     Assigns a virtual modifier to the property.
        /// </summary>
        public bool IsVirtual { get; set; }
        /// <summary>
        ///     Specifies the name of the method invoked after the property value is changed.<br/>
        ///     If the property is not specified, the method’s name should follow the <b>On[PropertyName]Changed</b> pattern.
        /// </summary>
        public string? OnChangedMethod { get; set; }
        /// <summary>
        ///     Specifies the name of the method invoked when the property value is changing.<br/>
        ///     If the property is not specified, the method’s name should follow the <b>On[PropertyName]Changing</b> pattern.
        /// </summary>
        public string? OnChangingMethod { get; set; }
        /// <summary>
        ///     Specifies an access modifier for a set accessor. The default value is the same as a property’s modifier.<br/>
        ///     Available values: <i>Public, Private, Protected, Internal, ProtectedInternal, PrivateProtected.</i>
        /// </summary>
        public AccessModifier SetterAccessModifier { get; set; }
    }

    /// <summary>
    ///     Applies to a method. The source generator produces boilerplate code for a Command based on this method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    class GenerateCommandAttribute : Attribute {
        /// <summary>
        ///     Specifies the <b>allowMultipleExecution</b> parameter in the <b>AsyncCommand</b> constructor. The default value is <see langword=""false""/>.
        /// </summary>
        public bool AllowMultipleExecution { get; set; }
        /// <summary>
        ///     Specifies a custom <b>CanExecute</b> method name. If the property is not specified, the method’s name should follow the <b>Can[ActionName]</b> pattern.
        /// </summary>
        public string? CanExecuteMethod { get; set; }
        /// <summary>
        ///     Specifies a custom <b>Command</b> name. The default value is <b>[ActionName]Command</b>.
        /// </summary>
        public string? Name { get; set; }
    }";
    }
}
