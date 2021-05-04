using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System;

namespace DevExpress.Mvvm.CodeGenerators.Tests {
    [TestFixture]
    public class GenerationTests {
        [Test]
        public void GenerationFormat() {
            var source = @"using DevExpress.Mvvm.CodeGenerators;

namespace Test {
    [GenerateViewModel(ImplementINotifyPropertyChanging = true, ImplementIDataErrorInfo = true, ImplementISupportServices = true)]
    partial class Example {
        [GenerateProperty]
        [System.ComponentModel.DataAnnotations.Range(0,
                                                     1)]
        int property;
        
        [GenerateCommand]
        public void Method(int arg) { }
    }

    public class Program {
        public static void Main(string[] args) { }
    }
}
";
            Compilation inputCompilation = CSharpCompilation.Create("MyCompilation",
                                                                    new[] { CSharpSyntaxTree.ParseText(source) },
                                                                    new[] {
                                                                        MetadataReference.CreateFromFile(typeof(DelegateCommand).Assembly.Location),
                                                                        MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.RangeAttribute).Assembly.Location),
                                                                        MetadataReference.CreateFromFile(typeof(System.Windows.Input.ICommand).Assembly.Location),
                                                                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                                                                    },
                                                                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            ViewModelGenerator generator = new ViewModelGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            GeneratorDriverRunResult runResult = driver.GetRunResult();
            GeneratorRunResult generatorResult = runResult.Results[0];

            var generatedCode = generatorResult.GeneratedSources[1].SourceText.ToString();
            var tabs = 0;
            foreach(var str in generatedCode.Split(new[] { Environment.NewLine }, StringSplitOptions.None)) {
                if(string.IsNullOrEmpty(str))
                    continue;

                if(str.Contains("}") && !str.Contains("{"))
                    if(str.EndsWith("}"))
                        tabs--;
                    else
                        Assert.Fail();

                var expectedLeadingWhitespaceCount = tabs * 4;
                var leadingWhitespaceCount = str.Length - str.TrimStart().Length;
                Assert.AreEqual(expectedLeadingWhitespaceCount, leadingWhitespaceCount);

                if(str.EndsWith("{"))
                    tabs++;
            }
        }
    }
}
