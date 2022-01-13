using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System.ComponentModel;
using System.Linq;

namespace DevExpress.Mvvm.CodeGenerators.Tests {
    [TestFixture]
    public class DiagnosticTestsPrism {
        [Test]
        public void NoPartialDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators.Prism;

namespace Test {
    [GenerateViewModel]
    class NoPartialClass { }

    public class Program {
        public static void Main(string[] args) { }
    }
}
";
            Compilation inputCompilation = CreateCompilation(sourceCode);
            ViewModelGenerator generator = new ViewModelGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            _ = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.AreEqual(2, outputCompilation.SyntaxTrees.Count());
            Assert.AreEqual(GeneratorDiagnostics.NoPartialModifier.Id, diagnostics[0].Id);
        }

        [Test]
        public void InvalidPropertyNameDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators.Prism;

namespace Test {
    [GenerateViewModel]
    partial class WithProperty {
        [GenerateProperty]
        int Property;
    }

    public class Program {
        public static void Main(string[] args) { }
    }
}
";
            Compilation inputCompilation = CreateCompilation(sourceCode);
            ViewModelGenerator generator = new ViewModelGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            _ = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.AreEqual(3, outputCompilation.SyntaxTrees.Count());
            Assert.AreEqual(GeneratorDiagnostics.InvalidPropertyName.Id, diagnostics[0].Id);
        }

        [Test]
        public void OnChangedMethodNotFoundDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators.Prism;

namespace Test {
    [GenerateViewModel]
    partial class OnChangedMethodNotFound {
        [GenerateProperty(OnChangedMethod = ""NotCreatedChangedMethod"", OnChangingMethod = ""NotCreatedChangingMethod"")]
        int value1;
        [GenerateProperty(OnChangedMethod = ""IncorrectSignatureChangedMethod"", OnChangingMethod = ""IncorrectSignatureChangingMethod"")]
        int value2;

        public int IncorrectSignatureChangedMethod() { return 1; }
        public void IncorrectSignatureChangingMethod(int arg1, int arg2) { }
    }

    public class Program {
        public static void Main(string[] args) { }
    }
}
";
            Compilation inputCompilation = CreateCompilation(sourceCode);
            ViewModelGenerator generator = new ViewModelGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            _ = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.AreEqual(3, outputCompilation.SyntaxTrees.Count());
            Assert.AreEqual(4, diagnostics.Count());
            foreach(var diagnostic in diagnostics)
                Assert.AreEqual(GeneratorDiagnostics.OnChangedMethodNotFound.Id, diagnostic.Id);
        }

        [Test]
        public void IncorrectCommandSignatureDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators.Prism;

namespace Test {
    [GenerateViewModel]
    partial class WithCommand {
        [GenerateCommand]
        public int Command1() {}

        [GenerateCommand]
        public void Command2(int? a, int? b) {}
    }

    public class Program {
        public static void Main(string[] args) { }
    }
}
";
            Compilation inputCompilation = CreateCompilation(sourceCode);
            ViewModelGenerator generator = new ViewModelGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            _ = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.AreEqual(3, outputCompilation.SyntaxTrees.Count());

            Assert.AreEqual(2, diagnostics.Count());
            Assert.AreEqual(GeneratorDiagnostics.IncorrectCommandSignature.Id, diagnostics[0].Id);
            Assert.AreEqual(GeneratorDiagnostics.IncorrectCommandSignature.Id, diagnostics[1].Id);
        }

        [Test]
        public void CanExecuteMethodNotFoundDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators.Prism;
using System.Threading.Tasks;

namespace Test {
    [GenerateViewModel]
    partial class CanExecuteMethodNotFound {
        [GenerateCommand(CanExecuteMethod = ""WrongParameter"")]
        public void Command1(int? arg) { }
        [GenerateCommand(CanExecuteMethod = ""NoCreated"")]
        public void Command2(int? arg) { }
        [GenerateCommand(CanExecuteMethod = ""ReturnNoBool"")]
        public void Command3(int? arg) { }

        [GenerateCommand(CanExecuteMethod = ""WrongParameter"")]
        public Task AsyncCommand1(int? arg) => Task.CompletedTask;
        [GenerateCommand(CanExecuteMethod = ""NoCreated"")]
        public Task AsyncCommand2(int? arg) => Task.CompletedTask;
        [GenerateCommand(CanExecuteMethod = ""ReturnNoBool"")]
        public Task AsyncCommand3(int? arg) => Task.CompletedTask;

        public bool WrongParameter() => true;
        public bool WrongParameter(string arg) => arg.Length > 0;
        public bool WrongParameter(int arg1, int arg2) => arg1 > arg2;

        public int ReturnNoBool(int arg) => arg;
    }

    public class Program {
        public static void Main(string[] args) { }
    }
}
";
            Compilation inputCompilation = CreateCompilation(sourceCode);
            ViewModelGenerator generator = new ViewModelGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            _ = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.AreEqual(3, outputCompilation.SyntaxTrees.Count());
            Assert.AreEqual(6, diagnostics.Count());
            foreach(var diagnostic in diagnostics)
                Assert.AreEqual(GeneratorDiagnostics.CanExecuteMethodNotFound.Id, diagnostic.Id);
        }

        [Test]
        public void RaiseMethodNotFoundDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators.Prism;
using System.ComponentModel;

namespace Test {
    class ParentWithoutRaiseMethod : INotifyPropertyChanged, INotifyPropertyChanging {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;
    }
    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class ChildWithoutRaiseMethod : ParentWithoutRaiseMethod {
        [GenerateProperty]
        int value;
    }

    public class Program {
        public static void Main(string[] args) { }
    }
}
";
            Compilation inputCompilation = CreateCompilation(sourceCode);
            ViewModelGenerator generator = new ViewModelGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            _ = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.AreEqual(3, outputCompilation.SyntaxTrees.Count());
            Assert.AreEqual(2, diagnostics.Count());
            foreach(var diagnostic in diagnostics)
                Assert.AreEqual(GeneratorDiagnostics.RaiseMethodNotFound.Id, diagnostic.Id);
        }

        [Test]
        public void NonNullableDelegateCommandArgumentDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators.Prism;
using System.ComponentModel;

namespace Test {
    [GenerateViewModel]
    partial class NonNullableArgumentClass {
        [GenerateCommand]
        void IntArgument(int arg) { }
        [GenerateCommand]
        void StructArgument(StructArg arg) { ]
    }

    struct StructArg { }
    public class Program {
        public static void Main(string[] args) { }
    }
}
";
            Compilation inputCompilation = CreateCompilation(sourceCode);
            ViewModelGenerator generator = new ViewModelGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            _ = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.AreEqual(3, outputCompilation.SyntaxTrees.Count());
            Assert.AreEqual(2, diagnostics.Count());
            foreach(var diagnostic in diagnostics)
                Assert.AreEqual(GeneratorDiagnostics.NonNullableDelegateCommandArgument.Id, diagnostic.Id);
        }

        [Test]
        public void TwoSuitableMethodsDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators.Prism;
using System.Threading.Tasks;

namespace Test {
    [GenerateViewModel]
    partial class TwoSuitableMethods {
        [GenerateProperty(OnChangedMethod = ""TwoChangedMethods"", OnChangingMethod = ""TwoChangingMethods"")]
        int value;

        public void TwoChangedMethods() { }
        public void TwoChangedMethods(int arg) { }

        public void TwoChangingMethods() { }
        public void TwoChangingMethods(int arg) { }
    }

    public class Program {
        public static void Main(string[] args) { }
    }
}
";
            Compilation inputCompilation = CreateCompilation(sourceCode);
            ViewModelGenerator generator = new ViewModelGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            _ = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.AreEqual(3, outputCompilation.SyntaxTrees.Count());
            Assert.AreEqual(2, diagnostics.Count());
            foreach(var diagnostic in diagnostics)
                Assert.AreEqual(GeneratorDiagnostics.TwoSuitableMethods.Id, diagnostic.Id);
        }

        [Test]
        public void TwoGenerateViewModelAttributeDiagnostic() {
            var sourceCode = @"namespace Test {
    [DevExpress.Mvvm.CodeGenerators.GenerateViewModel]
    [DevExpress.Mvvm.CodeGenerators.Prism.GenerateViewModel]
    partial class TwoGenerateViewModelAttributeClass { }
    }
";
            Compilation inputCompilation = CSharpCompilation.Create("MyCompilation",
                                     new[] { CSharpSyntaxTree.ParseText(sourceCode) },
                                     new[] {
                                         MetadataReference.CreateFromFile(typeof(System.Windows.Input.ICommand).Assembly.Location),
                                         MetadataReference.CreateFromFile(typeof(DelegateCommand).Assembly.Location),
                                         MetadataReference.CreateFromFile(typeof(Prism.Commands.DelegateCommand).Assembly.Location),
                                         MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                                         MetadataReference.CreateFromFile(typeof(INotifyPropertyChanged).Assembly.Location),
                                     },
                                     new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            ViewModelGenerator generator = new ViewModelGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            _ = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.AreEqual(GeneratorDiagnostics.MoreThanOneGenerateViewModelAttributes.Id, diagnostics[0].Id);
            Assert.AreEqual(3, outputCompilation.SyntaxTrees.Count());
        }
        public static Compilation CreateCompilation(string source) =>
            CSharpCompilation.Create("MyCompilation",
                                     new[] { CSharpSyntaxTree.ParseText(source) },
                                     new[] {
                                         MetadataReference.CreateFromFile(typeof(System.Windows.Input.ICommand).Assembly.Location),
                                         MetadataReference.CreateFromFile(typeof(Prism.Commands.DelegateCommand).Assembly.Location),
                                         MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                                         MetadataReference.CreateFromFile(typeof(INotifyPropertyChanged).Assembly.Location),
                                     },
                                     new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
