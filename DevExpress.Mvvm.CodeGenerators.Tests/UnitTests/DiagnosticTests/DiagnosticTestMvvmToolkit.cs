using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System.ComponentModel;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace DevExpress.Mvvm.CodeGenerators.Tests {
    [TestFixture]
    public class DiagnosticTestsMvvmToolkit {
        [Test]
        public void NoPartialDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators.MvvmToolkit;

namespace Test {
    [GenerateViewModel]
    class NoPartialClass { }

    public class Program {
        public static void Main(string[] args) { }
    }
}
";
            var (trees, diagnostics) = GeneratorHelper.GetDiagnostics(CreateCompilation(sourceCode));

            Assert.AreEqual(2, trees.Count());
            Assert.AreEqual(GeneratorDiagnostics.NoPartialModifier.Id, diagnostics[0].Id);
        }

        [Test]
        public void InvalidPropertyNameDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators.MvvmToolkit;

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

            var (trees, diagnostics) = GeneratorHelper.GetDiagnostics(CreateCompilation(sourceCode));

            Assert.AreEqual(3, trees.Count());
            Assert.AreEqual(GeneratorDiagnostics.InvalidPropertyName.Id, diagnostics[0].Id);
        }

        [Test]
        public void OnChangedMethodNotFoundDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators.MvvmToolkit;

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
            var (trees, diagnostics) = GeneratorHelper.GetDiagnostics(CreateCompilation(sourceCode));

            Assert.AreEqual(3, trees.Count());
            Assert.AreEqual(4, diagnostics.Count());
            foreach(var diagnostic in diagnostics)
                Assert.AreEqual(GeneratorDiagnostics.OnChangedMethodNotFound.Id, diagnostic.Id);
        }

        [Test]
        public void IncorrectCommandSignatureDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators.MvvmToolkit;

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

            var (trees, diagnostics) = GeneratorHelper.GetDiagnostics(CreateCompilation(sourceCode));

            Assert.AreEqual(3, trees.Count());

            Assert.AreEqual(2, diagnostics.Count());
            Assert.AreEqual(GeneratorDiagnostics.IncorrectCommandSignature.Id, diagnostics[0].Id);
            Assert.AreEqual(GeneratorDiagnostics.IncorrectCommandSignature.Id, diagnostics[1].Id);
        }

        [Test]
        public void CanExecuteMethodNotFoundDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators.MvvmToolkit;
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
        public bool WrongParameter(string arg) => arg.Length > 0;
        public bool WrongParameter(int arg1, int arg2) => arg1 > arg2;

        public int ReturnNoBool(int arg) => arg;
    }

    public class Program {
        public static void Main(string[] args) { }
    }
}
";
            var (trees, diagnostics) = GeneratorHelper.GetDiagnostics(CreateCompilation(sourceCode));

            Assert.AreEqual(3, trees.Count());
            Assert.AreEqual(6, diagnostics.Count());
            foreach(var diagnostic in diagnostics)
                Assert.AreEqual(GeneratorDiagnostics.CanExecuteMethodNotFound.Id, diagnostic.Id);
        }

        [Test]
        public void RaiseMethodNotFoundDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators.MvvmToolkit;
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
            var (trees, diagnostics) = GeneratorHelper.GetDiagnostics(CreateCompilation(sourceCode));

            Assert.AreEqual(3, trees.Count());
            Assert.AreEqual(2, diagnostics.Count());
            foreach(var diagnostic in diagnostics)
                Assert.AreEqual(GeneratorDiagnostics.OnMethodNotFound.Id, diagnostic.Id);
        }

        [Test]
        public void TwoSuitableMethodsDiagnostic() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators.MvvmToolkit;
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
            var (trees, diagnostics) = GeneratorHelper.GetDiagnostics(CreateCompilation(sourceCode));

            Assert.AreEqual(3, trees.Count());
            Assert.AreEqual(2, diagnostics.Count());
            foreach(var diagnostic in diagnostics)
                Assert.AreEqual(GeneratorDiagnostics.TwoSuitableMethods.Id, diagnostic.Id);
        }
        [Test]
        public void NoBaseObservableRecipientClass() {
            var sourceCode = @"using DevExpress.Mvvm.CodeGenerators.MvvmToolkit;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Test {
    [GenerateViewModel]
    partial class Broadcast {
        [GenerateProperty(Broadcast = true)]
        int value;
        public Broadcast() => value.ToString(); //avoid not used warning
    }
}
";
            var (trees, diagnostics) = GeneratorHelper.GetDiagnostics(CreateCompilation(sourceCode));
            Assert.AreEqual(3, trees.Count());
            Assert.AreEqual(GeneratorDiagnostics.NoBaseObservableRecipientClass.Id, diagnostics.Single().Id);
        }
        [TestCase("[DevExpress.Mvvm.CodeGenerators.Prism.GenerateViewModel]\r\n")]
        [TestCase("[DevExpress.Mvvm.CodeGenerators.GenerateViewModel]\r\n")]
        public void TwoGenerateViewModelAttributeDiagnostic(string generateViewModel) {
            var sourceCode = "namespace Test {\r\n"
                + generateViewModel +
    @"[DevExpress.Mvvm.CodeGenerators.MvvmToolkit.GenerateViewModel]
    partial class TwoGenerateViewModelAttributeClass { }
    }
";
            var (trees, diagnostics) = GeneratorHelper.GetDiagnostics(GeneratorHelper.CreateCompilation(sourceCode, new[] {
                typeof(INotifyPropertyChanged),
                typeof(RelayCommand),
                typeof(Prism.Commands.DelegateCommand),
                typeof(DevExpress.Mvvm.DelegateCommand),
            }));

            Assert.AreEqual(GeneratorDiagnostics.MoreThanOneGenerateViewModelAttributes.Id, diagnostics[0].Id);
            Assert.AreEqual(4, trees.Count());
        }
        public static Compilation CreateCompilation(string source) =>
            GeneratorHelper.CreateCompilation(source, new[] { typeof(INotifyPropertyChanged), typeof(RelayCommand) });
    }
}
