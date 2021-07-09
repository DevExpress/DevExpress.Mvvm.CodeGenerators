namespace DevExpress.Mvvm.CodeGenerators {
    static class InitializationGenerator {
        public static string Namespace { get => "DevExpress.Mvvm.CodeGenerators"; }

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
