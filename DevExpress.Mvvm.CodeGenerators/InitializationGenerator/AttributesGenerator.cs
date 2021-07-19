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


        static readonly string UseCommandManagerProperty = $@"public bool {UseCommandManager} {{ get; set; }}";
        static readonly string ImplementIDEIProperty = $@"public bool {ImplementIDEI} {{ get; set; }}";

        public static string GetSourceCode(bool isWinUI) =>
$@"    [AttributeUsage(AttributeTargets.Class)]
    class {viewModelAttribute} : Attribute {{
        {(isWinUI ? null : ImplementIDEIProperty)}
        public bool {ImplementINPCing} {{ get; set; }}
        public bool {ImplementISPVM} {{ get; set; }}
        public bool {ImplementISS} {{ get; set; }}
    }}

    [AttributeUsage(AttributeTargets.Field)]
    class {propertyAttribute} : Attribute {{
        public bool {IsVirtual} {{ get; set; }}
        public string? {OnChangedMethod} {{ get; set; }}
        public string? {OnChangingMethod} {{ get; set; }}
        public AccessModifier {SetterAccessModifier} {{ get; set; }}
    }}

    [AttributeUsage(AttributeTargets.Method)]
    class {commandAttribute} : Attribute {{
        public bool {AllowMultipleExecution} {{ get; set; }}
        {(isWinUI ? null : UseCommandManagerProperty)}
        public string? {CanExecuteMethod} {{ get; set; }}
        public string? {CommandName} {{ get; set; }}
    }}";
    }
}
