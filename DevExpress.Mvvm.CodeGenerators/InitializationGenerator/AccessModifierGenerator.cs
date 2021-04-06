namespace DevExpress.Mvvm.CodeGenerators {
    static class AccessModifierGenerator {
        static readonly string[] accessModifiers = new[] {
            "Public",
            "Private",
            "Protected",
            "Internal",
            "ProtectedInternal",
        };

        public static string GetSourceCode() =>
$@"public enum AccessModifier {{
{accessModifiers.ConcatToString("," + System.Environment.NewLine).AddTabs(1)}
}}";
        public static string GetCodeRepresentation(int enumIndex) =>
            enumIndex switch {
                0 => string.Empty,
                4 => "protected internal ",
                _ => accessModifiers[enumIndex].ToLower() + " ",
            };
    }
}
