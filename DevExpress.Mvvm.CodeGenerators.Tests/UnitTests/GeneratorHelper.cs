using DevExpress.Mvvm.Native;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Mvvm.CodeGenerators.Tests {
    public static class GeneratorHelper {
        public static string GenerateCode(string source, Type frameworkType) {
            Compilation inputCompilation = CreateCompilation(source, frameworkType.YieldIfNotNull().Concat(typeof(System.ComponentModel.DataAnnotations.RangeAttribute).Yield()));
            ViewModelGenerator generator = new ViewModelGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            GeneratorDriverRunResult runResult = driver.GetRunResult();
            GeneratorRunResult generatorResult = runResult.Results[0];

            var generatedCode = generatorResult.GeneratedSources[1].SourceText.ToString();
            return generatedCode;
        }

        public static Compilation CreateCompilation(string source, IEnumerable<Type> types) {
            IEnumerable<Type> baseTypes = new[] {
                typeof(System.Windows.Input.ICommand),
                typeof(object),
            }.Concat(types);
            Compilation inputCompilation = CSharpCompilation.Create(
                "MyCompilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                baseTypes.Select(x => MetadataReference.CreateFromFile(x.Assembly.Location)),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );
            return inputCompilation;
        }
    }
}

