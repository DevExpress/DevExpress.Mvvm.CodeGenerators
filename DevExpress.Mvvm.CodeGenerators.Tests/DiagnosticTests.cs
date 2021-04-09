using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System.Linq;

namespace DevExpress.Mvvm.CodeGenerators.Tests {
    [TestFixture]
    public class DiagnosticTests {
        [Test]
        public void NoPartialDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators;

namespace Test {
    [GenerateViewModel]
    class NoPartialClass { }

    public class Program {
        public static void Main(string[] args) { }
    }
}
";
            Compilation inputCompilation = Helper.CreateCompilation(sourceCode);
            ViewModelGenerator generator = new ViewModelGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            _ = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.AreEqual(2, outputCompilation.SyntaxTrees.Count());
            Assert.AreEqual(GeneratorDiagnostics.NoPartialModifier.Id, diagnostics[0].Id);
        }

        [Test]
        public void ClassWithinClassDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators;

namespace Test {
    [GenerateViewModel]
    partial class OuterClass {
        [GenerateViewModel]
        partial class InnerClass { }
}

    public class Program {
        public static void Main(string[] args) { }
    }
}
";
            Compilation inputCompilation = Helper.CreateCompilation(sourceCode);
            ViewModelGenerator generator = new ViewModelGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            _ = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.AreEqual(3, outputCompilation.SyntaxTrees.Count());
            Assert.AreEqual(GeneratorDiagnostics.ClassWithinClass.Id, diagnostics[0].Id);
        }

        [Test]
        public void MVVMNotAvailableDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators;
using System.Threading.Tasks;

namespace Test {
    [GenerateViewModel(ImplementIDataErrorInfo = true)]
    partial class WithIDataErrorInfo { }

    [GenerateViewModel]
    partial class WithCommand {
        [GenerateCommand]
        public void Command() { }
    }
    
    [GenerateViewModel]
    partial class WithAsyncCommand {
        [GenerateCommand]
        public Task AsyncCommand() { }
    }

    public class Program {
        public static void Main(string[] args) { }
    }
}
";
            Compilation inputCompilation = CSharpCompilation.Create("MyCompilation",
                                                                    new[] { CSharpSyntaxTree.ParseText(sourceCode) },
                                                                    new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
                                                                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            ViewModelGenerator generator = new ViewModelGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            _ = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.AreEqual(5, outputCompilation.SyntaxTrees.Count());

            Assert.AreEqual(3, diagnostics.Count());
            Assert.AreEqual(GeneratorDiagnostics.MVVMNotAvailable.Id, diagnostics[0].Id);
            Assert.AreEqual(GeneratorDiagnostics.MVVMNotAvailable.Id, diagnostics[1].Id);
            Assert.AreEqual(GeneratorDiagnostics.MVVMNotAvailable.Id, diagnostics[2].Id);
        }

        [Test]
        public void InvalidPropertyNameDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators;

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
            Compilation inputCompilation = Helper.CreateCompilation(sourceCode);
            ViewModelGenerator generator = new ViewModelGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            _ = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.AreEqual(3, outputCompilation.SyntaxTrees.Count());
            Assert.AreEqual(GeneratorDiagnostics.InvalidPropertyName.Id, diagnostics[0].Id);
        }

        [Test]
        public void OnChangedMethodNotFoundDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators;

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
            Compilation inputCompilation = Helper.CreateCompilation(sourceCode);
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
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators;

namespace Test {
    [GenerateViewModel]
    partial class WithCommand {
        [GenerateCommand]
        public int Command1() {}

        [GenerateCommand]
        public void Command2(int a, int b) {}
    }

    public class Program {
        public static void Main(string[] args) { }
    }
}
";
            Compilation inputCompilation = Helper.CreateCompilation(sourceCode);
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
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators;
using System.Threading.Tasks;

namespace Test {
    [GenerateViewModel]
    partial class CanExecuteMethodNotFound {
        [GenerateCommand(CanExecuteMethod = ""WrongParameter"")]
        public void Command1(int arg) { }
        [GenerateCommand(CanExecuteMethod = ""NoCreated"")]
        public void Command2(int arg) { }
        [GenerateCommand(CanExecuteMethod = ""ReturnNoBool"")]
        public void Command3(int arg) { }

        [GenerateCommand(CanExecuteMethod = ""WrongParameter"")]
        public Task AsyncCommand1(int arg) => Task.CompletedTask;
        [GenerateCommand(CanExecuteMethod = ""NoCreated"")]
        public Task AsyncCommand2(int arg) => Task.CompletedTask;
        [GenerateCommand(CanExecuteMethod = ""ReturnNoBool"")]
        public Task AsyncCommand3(int arg) => Task.CompletedTask;

        public bool WrongParameter() => true;
        public bool WrongParameter(int? arg) => arg.HasValue;
        public bool WrongParameter(string arg) => arg.Length > 0;
        public bool WrongParameter(int arg1, int arg2) => arg1 > arg2

        public int ReturnNoBool(int arg) => arg;
    }

    public class Program {
        public static void Main(string[] args) { }
    }
}
";
            Compilation inputCompilation = Helper.CreateCompilation(sourceCode);
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
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators;
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
            Compilation inputCompilation = Helper.CreateCompilation(sourceCode);
            ViewModelGenerator generator = new ViewModelGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            _ = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.AreEqual(3, outputCompilation.SyntaxTrees.Count());
            Assert.AreEqual(2, diagnostics.Count());
            foreach(var diagnostic in diagnostics)
                Assert.AreEqual(GeneratorDiagnostics.RaiseMethodNotFound.Id, diagnostic.Id);
        }

        [Test]
        public void GenericViewModelDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators;

namespace Test {
    [GenerateViewModel]
    partial class GenericClass<T> { }

    public class Program {
        public static void Main(string[] args) { }
    }
}
";
            Compilation inputCompilation = Helper.CreateCompilation(sourceCode);
            ViewModelGenerator generator = new ViewModelGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            _ = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.AreEqual(2, outputCompilation.SyntaxTrees.Count());
            Assert.AreEqual(GeneratorDiagnostics.GenericViewModel.Id, diagnostics[0].Id);
        }

        [Test]
        public void TwoSuitableMethodsDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators;
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
            Compilation inputCompilation = Helper.CreateCompilation(sourceCode);
            ViewModelGenerator generator = new ViewModelGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            _ = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.AreEqual(3, outputCompilation.SyntaxTrees.Count());
            Assert.AreEqual(2, diagnostics.Count());
            foreach(var diagnostic in diagnostics)
                Assert.AreEqual(GeneratorDiagnostics.TwoSuitableMethods.Id, diagnostic.Id);
        }
    }
}
