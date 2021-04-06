namespace DevExpress.Mvvm.CodeGenerators {
    static class InitializationGenerator {
        public static string Namespace { get => "DevExpress.Mvvm.CodeGenerators"; }

        public static string GetSourceCode() =>
$@"using System;

namespace {Namespace} {{
{AccessModifierGenerator.GetSourceCode().AddTabs(1)}

{AttributesGenerator.GetSourceCode().AddTabs(1)}
}}
";
    }
}
