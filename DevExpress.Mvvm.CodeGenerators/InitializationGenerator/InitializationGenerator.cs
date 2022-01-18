using System;

namespace DevExpress.Mvvm.CodeGenerators {
    static class InitializationGenerator {
        public const string DxNamespace = "DevExpress.Mvvm.CodeGenerators";
        public const string PrismNamespace = "DevExpress.Mvvm.CodeGenerators.Prism";
        public const string MvvmLightNamespace = "DevExpress.Mvvm.CodeGenerators.MvvmLight";

        public static string GetSourceCode(SupportedMvvm mvvm, bool isWinUI) =>
$@"using System;
using System.Collections.Generic;

#nullable enable

namespace {mvvm switch {
    SupportedMvvm.None or SupportedMvvm.Dx => DxNamespace,
    SupportedMvvm.Prism => PrismNamespace,
    SupportedMvvm.MvvmLight => MvvmLightNamespace,
    _ => throw new InvalidOperationException()
}} {{
{AccessModifierGenerator.GetSourceCode()}

{AttributesGenerator.GetSourceCode(mvvm, isWinUI)}
}}
";
    }
}
