using System;

namespace DevExpress.Mvvm.CodeGenerators {
    enum AccessModifier {
        Public,
        Private,
        Protected,
        Internal,
        ProtectedInternal,
        PrivateProtected,
    };
    static class AccessModifierGenerator {
        public static string GetSourceCode() =>
$@"    internal enum AccessModifier {{
        Public,
        Private,
        Protected,
        Internal,
        ProtectedInternal,
        PrivateProtected,
    }}";
        public static string GetCodeRepresentation(AccessModifier modifier) =>
            modifier switch {
                AccessModifier.Public => string.Empty,
                AccessModifier.Private => "private ",
                AccessModifier.Protected => "protected ",
                AccessModifier.Internal => "internal ",
                AccessModifier.ProtectedInternal => "protected internal ",
                AccessModifier.PrivateProtected => "private protected ",
                _ => throw new InvalidOperationException(),
            };
    }
}
