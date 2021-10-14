using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Mvvm.CodeGenerators {
    static class InitializationGenerator {
        public const string DxNamespace = "DevExpress.Mvvm.CodeGenerators";
        public const string PrismNamespace = "DevExpress.Mvvm.CodeGenerators.Prism";

        public static string GetSourceCode(SupportedMvvm mvvm, bool isWinUI) =>
$@"using System;
using System.Collections.Generic;

#nullable enable

namespace {((mvvm == SupportedMvvm.Dx || mvvm == SupportedMvvm.None) ? DxNamespace : PrismNamespace)} {{
{AccessModifierGenerator.GetSourceCode()}

{AttributesGenerator.GetSourceCode(mvvm, isWinUI)}
}}
";
    }
}
