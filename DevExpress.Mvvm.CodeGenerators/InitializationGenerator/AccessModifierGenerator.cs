using System;

namespace DevExpress.Mvvm.CodeGenerators {
    enum AccessModifier {
        Public,
        Private,
        Protected,
        Internal,
        ProtectedInternal,
    };
    static class AccessModifierGenerator {
        public static string GetSourceCode() =>
$@"    public enum AccessModifier {{
        Public,
        Private,
        Protected,
        Internal,
        ProtectedInternal,
    }}";
        public static string GetCodeRepresentation(AccessModifier modifier) =>
            modifier switch {
                AccessModifier.Public => string.Empty,
                AccessModifier.Private => "private ",
                AccessModifier.Protected => "protected ",
                AccessModifier.Internal => "internal ",
                AccessModifier.ProtectedInternal => "protected internal ",
                _ => throw new InvalidOperationException(),
            };
    }
}
