namespace DevExpress.Mvvm.CodeGenerators {
    static class InitializationGenerator {
        public const string Namespace = "DevExpress.Mvvm.CodeGenerators";

        public static string GetSourceCode(bool isWinUI) =>
$@"using System;

#nullable enable

namespace {Namespace} {{
{AccessModifierGenerator.GetSourceCode()}

{AttributesGenerator.GetSourceCode(isWinUI)}
}}
";
    }
}
