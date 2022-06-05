using System;

namespace DevExpress.Mvvm.CodeGenerators {
    public static class AttributesGenerator {
        public static readonly string DxPropertyAttributeFullName = $"{InitializationGenerator.DxNamespace}.GeneratePropertyAttribute";
        public static readonly string PrismPropertyAttributeFullName = $"{InitializationGenerator.PrismNamespace}.GeneratePropertyAttribute";
        public static readonly string MvvmLightPropertyAttributeFullName = $"{InitializationGenerator.MvvmLightNamespace}.GeneratePropertyAttribute";
        public static readonly string MvvmToolkitPropertyAttributeFullName = $"{InitializationGenerator.MvvmToolkitNamespace}.GeneratePropertyAttribute";

        public const string ImplementIDEI = "ImplementIDataErrorInfo";
        public const string ImplementINPCing = "ImplementINotifyPropertyChanging";
        public const string ImplementISPVM = "ImplementISupportParentViewModel";
        public const string ImplementISS = "ImplementISupportServices";
        public const string ImplementISSWinUI = "ImplementISupportUIServices";
        public const string ImplementIAA = "ImplementIActiveAware";
        public const string ImplementICU = "ImplementICleanup";

        public const string IsVirtual = "IsVirtual";
        public const string OnChangedMethod = "OnChangedMethod";
        public const string OnChangingMethod = "OnChangingMethod";
        public const string SetterAccessModifier = "SetterAccessModifier";
        public const string Broadcast = "Broadcast";

        public const string AllowMultipleExecution = "AllowMultipleExecution";
        public const string UseCommandManager = "UseCommandManager";
        public const string CanExecuteMethod = "CanExecuteMethod";
        public const string CommandName = "Name";

        public const string ObservesCanExecuteProperty = "ObservesCanExecuteProperty";
        public const string ObservesProperties = "ObservesProperties";

        internal static string GetSourceCode(SupportedMvvm mvvm, bool isWinUI) =>
            mvvm switch {
                SupportedMvvm.Dx => isWinUI ? dxwinUISourceCode : dxMvvmSourceCode,
                SupportedMvvm.Prism => prismMvvmSourceCode,
                SupportedMvvm.MvvmLight => mvvmLightSourceCode,
                SupportedMvvm.MvvmToolkit => mvvmToolkitSourceCode,
                SupportedMvvm.None => commonSourceCode,
                _ => throw new InvalidOperationException()
            };

        const string dxMvvmSourceCode = @"    /// <summary>
    ///     Indicates that the View Model Code Generator should process this class and produce a View Model.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class GenerateViewModelAttribute : Attribute {
        /// <summary>
        ///     Implements
        ///     <see href=""https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.idataerrorinfo"">IDataErrorInfo﻿</see>
        ///     that allows you to validate data.
        /// </summary>
        public bool ImplementIDataErrorInfo { get; set; }
        /// <summary>
        ///     Implements
        ///     <see href=""https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanging"">INotifyPropertyChanging﻿.</see>
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
        const string commonSourceCode = @"    /// <summary>
    ///     Applies to a class. Indicates that the source generator should process this class and produce View Model boilerplate code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class GenerateViewModelAttribute : Attribute {
        /// <summary>
        ///     Implements
        ///     <see href=""https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanging"">INotifyPropertyChanging﻿.</see>
        /// </summary>
        public bool ImplementINotifyPropertyChanging { get; set; }
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
    }";
        const string dxwinUISourceCode = @"    /// <summary>
    ///     Applies to a class. Indicates that the source generator should process this class and produce View Model boilerplate code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class GenerateViewModelAttribute : Attribute {
        /// <summary>
        ///     Implements
        ///     <see href=""https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanging"">INotifyPropertyChanging﻿.</see>
        /// </summary>
        public bool ImplementINotifyPropertyChanging { get; set; }
        /// <summary>
        ///     Implements <b>ISupportUIServices</b> that allows you to include the UI Services mechanism to your View Model.
        /// </summary>
        public bool ImplementISupportUIServices { get; set; }
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
        const string prismMvvmSourceCode = @"    /// <summary>
    ///     Indicates that the View Model Code Generator should process this class and produce a View Model.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class GenerateViewModelAttribute : Attribute {
        /// <summary>
        ///     Implements
        ///     <see href=""https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanging"">INotifyPropertyChanging﻿.</see>
        /// </summary>
        public bool ImplementINotifyPropertyChanging { get; set; }
        /// <summary>
        ///     Implements <b>IActiveAware</b> that allows you to track the active state of a View.
        /// </summary>
        public bool ImplementIActiveAware { get; set; }
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
        ///     Specifies the
        ///     <see href=""https://prismlibrary.com/docs/commanding.html#observescanexecute"">ObservesCanExecute</see>
        ///     ﻿ method for the supplied property. This method listens to property changes and use the property as the <b>CanExecute</b> delegate.
        /// </summary>
        public string? ObservesCanExecuteProperty { get; set; }
        /// <summary>
        ///     Specifies the 
        ///     <see href=""https://prismlibrary.com/docs/commanding.html#observesproperty"">ObservesProperty</see>
        ///     ﻿ methods for all supplied properties. Each method calls the CanExecute method when the corresponding property value changes.
        /// </summary>
        public string[]? ObservesProperties { get; set; }
        /// <summary>
        ///     Specifies a custom <b>CanExecute</b> method name. If the property is not specified, the method’s name should follow the <b>Can[ActionName]</b> pattern.
        /// </summary>
        public string? CanExecuteMethod { get; set; }
        /// <summary>
        ///     Specifies a custom <b>Command</b> name. The default value is <b>[ActionName]Command</b>.
        /// </summary>
        public string? Name { get; set; }
    }";
        const string mvvmLightSourceCode = @"    /// <summary>
    ///     Indicates that the View Model Code Generator should process this class and produce a View Model.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class GenerateViewModelAttribute : Attribute {
        /// <summary>
        ///     Implements
        ///     <see href=""https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanging"">INotifyPropertyChanging﻿.</see>
        /// </summary>
        public bool ImplementINotifyPropertyChanging { get; set; }
        /// <summary>
        ///     Implements the ICleanup interface that allows you to clean your View Model (for example, flush its state to persistent storage, close the stream).
        /// </summary>
        public bool ImplementICleanup { get; set; }
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
        ///     Specifies a custom <b>CanExecute</b> method name. If the property is not specified, the method’s name should follow the <b>Can[ActionName]</b> pattern.
        /// </summary>
        public string? CanExecuteMethod { get; set; }
        /// <summary>
        ///     Specifies a custom <b>Command</b> name. The default value is <b>[ActionName]Command</b>.
        /// </summary>
        public string? Name { get; set; }
    }";
        const string mvvmToolkitSourceCode = @"    /// <summary>
    ///     Indicates that the View Model Code Generator should process this class and produce a View Model.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class GenerateViewModelAttribute : Attribute {
        /// <summary>
        ///     Implements
        ///     <see href=""https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanging"">INotifyPropertyChanging﻿.</see>
        /// </summary>
        public bool ImplementINotifyPropertyChanging { get; set; }
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
        /// <summary>
        ///     Broadcasts a PropertyChangedMessage after the property value is changed. 
        ///     Inherit your class from the ObservableRecipient to enable this functionality. 
        /// </summary>
        public bool Broadcast{ get; set; }
    }

    /// <summary>
    ///     Indicates that the View Model Code Generator should process this method and produce a Command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    class GenerateCommandAttribute : Attribute {
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
