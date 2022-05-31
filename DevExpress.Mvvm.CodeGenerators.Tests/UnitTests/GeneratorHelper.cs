using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Mvvm.CodeGenerators.Tests {
    public static class GeneratorHelper {
        public static string GenerateCode(string source, Type frameworkType) {
            IEnumerable<Type> types = new[] {
                typeof(System.ComponentModel.DataAnnotations.RangeAttribute),
                typeof(System.Windows.Input.ICommand),
                typeof(object),
            };
            if(frameworkType != null)
                types = types.Concat(new[] { frameworkType });
            Compilation inputCompilation = CSharpCompilation.Create(
                "MyCompilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                types.Select(x => MetadataReference.CreateFromFile(x.Assembly.Location)),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );
            ViewModelGenerator generator = new ViewModelGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            GeneratorDriverRunResult runResult = driver.GetRunResult();
            GeneratorRunResult generatorResult = runResult.Results[0];

            var generatedCode = generatorResult.GeneratedSources[1].SourceText.ToString();
            return generatedCode;
        }
    }
}

