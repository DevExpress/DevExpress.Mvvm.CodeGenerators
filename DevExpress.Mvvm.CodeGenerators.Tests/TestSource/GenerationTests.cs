using DevExpress.Mvvm.Native;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System;
using System.Linq;

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
            string generatedCode = GenerateCode(source);
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

        [Test]
        public void DevExpressUsing_Command() {
            var source = @"
using DevExpress.Mvvm.CodeGenerators; 
namespace Test {
    [GenerateViewModel]
    partial class Example {
        [GenerateCommand]
        public void Method(int arg) { }
    }
}";
            string generatedCode = GenerateCode(source);
            StringAssert.Contains("using DevExpress.Mvvm;", generatedCode);
            StringAssert.Contains("MethodCommand", generatedCode);
        }
        [Test]
        public void NoDevExpressUsing_Command() {
            var source = @"
using DevExpress.Mvvm.CodeGenerators; 
namespace Test {
    [GenerateViewModel]
    partial class Example {
        [GenerateCommand]
        public void Method(int arg) { }
    }
}";
            string generatedCode = GenerateCode(source, addMVVM: false);
            StringAssert.DoesNotContain("using DevExpress.Mvvm;", generatedCode);
            StringAssert.DoesNotContain("MethodCommand", generatedCode);
        }
        [Test]
        public void DevExpressUsing_SupportServices() {
            var source = @"
using DevExpress.Mvvm.CodeGenerators; 
namespace Test {
    [GenerateViewModel(ImplementISupportServices = true)]
    partial class Example {
        [GenerateProperty]
        int property;
    }
}";
            string generatedCode = GenerateCode(source);
            StringAssert.Contains("using DevExpress.Mvvm;", generatedCode);
            StringAssert.Contains("ISupportServices", generatedCode);
            StringAssert.Contains("int Property", generatedCode);
        }
        [Test]
        public void DevExpressUsing_NoMVVMComponents() {
            var source = @"
using DevExpress.Mvvm.CodeGenerators;
namespace Test {
    [GenerateViewModel]
    partial class Example {
        [GenerateProperty]
        int property;
    }
}";
            string generatedCode = GenerateCode(source);
            StringAssert.Contains("using DevExpress.Mvvm;", generatedCode);
            StringAssert.Contains("int Property", generatedCode);
        }

        static string GenerateCode(string source, bool addMVVM = true) {
            var references = new[] {
                MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.RangeAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Windows.Input.ICommand).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            };
            if(addMVVM)
                references = MetadataReference.CreateFromFile(typeof(DelegateCommand).Assembly.Location)
                    .Yield()
                    .Concat(references)
                    .ToArray();
            Compilation inputCompilation = CSharpCompilation.Create("MyCompilation",
                                                                    new[] { CSharpSyntaxTree.ParseText(source) },
                                                                    references,
                                                                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
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
