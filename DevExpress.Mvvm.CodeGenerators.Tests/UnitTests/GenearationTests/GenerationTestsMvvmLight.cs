using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System;
using Prism.Commands;
using DevExpress.Mvvm.CodeGenerators;
using GalaSoft.MvvmLight.Command;

namespace MvvmLight.Mvvm.Tests {
    [TestFixture]
    public class GenerationTests {
        [Test]
        public void GenerationFormat() {
            var source = @"using DevExpress.Mvvm.CodeGenerators.MvvmLight;

        namespace Test {
            [GenerateViewModel(ImplementICleanup = true, ImplementINotifyPropertyChanging = true)]
            partial class ExamplePrism {
                [GenerateProperty]
                [System.ComponentModel.DataAnnotations.Range(0,
                                                             1)]
                int property;

                [GenerateCommand]
                public void Method(int? arg) { }
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
        public void PrismUsing_Command() {
            var source = @"
        using DevExpress.Mvvm.CodeGenerators.MvvmLight; 
        namespace Test {
            [GenerateViewModel]
            partial class Example {
                [GenerateCommand]
                public void Method(int arg) { }
            }
        }";
            string generatedCode = GenerateCode(source);
            StringAssert.Contains("using GalaSoft.MvvmLight.Command;", generatedCode);
            StringAssert.Contains("MethodCommand", generatedCode);
        }
        [Test]
        public void GenerateComments() {
            var source = @"using DevExpress.Mvvm.CodeGenerators.MvvmLight;

        namespace Test {
            [GenerateViewModel]
            partial class Example {

                // Ordinary comment
                /// <summary>
                /// Ignorable comment
                /// </summary>

                /// <summary>
                /// Test property comment
                /// </summary>
                // Ordinary comment
                [GenerateProperty]
                int property;

                /**
                <summary>
                MultiLine Comment
                </summary>
                */
                [GenerateProperty]
                int property2;

             /// <summary>
        /// Test command comment
                            /// </summary>
                [GenerateCommand]
                public void Method(int? arg) { }
            }
        }
        ";
            var propertyComment =
@"        /// <summary>
        /// Test command comment
        /// </summary>";
            var property2Comment =
@"        /**
        <summary>
        MultiLine Comment
        </summary>
        */";
            var commandComment =
@"        /// <summary>
        /// Test property comment
        /// </summary>";
            var generatedCode = GenerateCode(source);
            StringAssert.Contains(propertyComment, generatedCode);
            StringAssert.Contains(property2Comment, generatedCode);
            StringAssert.Contains(commandComment, generatedCode);
            StringAssert.DoesNotContain("Ignorable comment", generatedCode);
            StringAssert.DoesNotContain("Ordinary comment", generatedCode);
        }
        [Test]
        public void GeneratePropertyName_() {
            var source = @"using DevExpress.Mvvm.CodeGenerators.MvvmLight;

        namespace Test {
            [GenerateViewModel]
            partial class Example {
                [GenerateProperty]
                int _;
            }
        }
        ";
            Assert.DoesNotThrow(() => GenerateCode(source));
        }

        static string GenerateCode(string source) {
            var references = new[] {
                MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.RangeAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Windows.Input.ICommand).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(RelayCommand).Assembly.Location),
            };
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
        [Test]
        public void FormattingTest() {
            const string source =
@"using DevExpress.Mvvm.CodeGenerators.MvvmLight;
using System.Threading.Tasks;
using System;

namespace Test {
    [GenerateViewModel(ImplementINotifyPropertyChanging = true, ImplementICleanup = true)]
    partial class Example0 {
        [GenerateProperty]
        int _Int;

        [GenerateProperty(OnChangedMethod = ""OnStrChanged_"", OnChangingMethod = ""OnStrChanging_"")]
        string _Str;
            void OnStrChanged_() { }
            void OnStrChanging_(string newValue) { }

            /// <summary>
            /// Test property comment
            /// </summary>
            [GenerateProperty]
            [System.ComponentModel.DataAnnotations.Required]
            [System.ComponentModel.DataAnnotations.Key]
            long _Long;
            void OnLongChanged(long oldValue) { }

            [GenerateProperty]
            DateTime _DateTime;
            void OnDateTimeChanging() { }

            [GenerateProperty(IsVirtual = true, SetterAccessModifier = AccessModifier.Protected)]
            double _Double;

            [GenerateProperty]
            int _;

            /// <summary>
            /// Test command comment
            /// </summary>
            [GenerateCommand]
            public void Command1(int? arg) { }
            bool CanCommand1(int? arg) => true;

            [GenerateCommand(Name = ""SomeCommand"")]
            public Task Command2() => null;
            bool CanCommand2(int arg) => true;

            [GenerateCommand(CanExecuteMethod = ""CanCommand3_"")]
            public void Command3(int? arg) { }
            bool CanCommand3_(int? arg) => true;

            [GenerateProperty]
            bool a, b;

            void OnCleanup() { }
        }
    }";
            const string expected =
@"using System.Collections.Generic;
using System.ComponentModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

#nullable enable

namespace Test {
    partial class Example0 : INotifyPropertyChanged, INotifyPropertyChanging, ICleanup {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event PropertyChangingEventHandler? PropertyChanging;
        protected IMessenger MessengerInstance { get; set; } = Messenger.Default;
        public virtual void Cleanup() {
            MessengerInstance.Unregister(this);
            OnCleanup();
        }

        protected void RaisePropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);
        protected void RaisePropertyChanging(PropertyChangingEventArgs e) => PropertyChanging?.Invoke(this, e);

        public int Int {
            get => _Int;
            set {
                if(EqualityComparer<int>.Default.Equals(_Int, value)) return;
                RaisePropertyChanging(IntChangingEventArgs);
                _Int = value;
                RaisePropertyChanged(IntChangedEventArgs);
            }
        }
        public string? Str {
            get => _Str;
            set {
                if(EqualityComparer<string?>.Default.Equals(_Str, value)) return;
                RaisePropertyChanging(StrChangingEventArgs);
                OnStrChanging_(value);
                _Str = value;
                RaisePropertyChanged(StrChangedEventArgs);
                OnStrChanged_();
            }
        }
        /// <summary>
        /// Test property comment
        /// </summary>
        [System.ComponentModel.DataAnnotations.RequiredAttribute]
        [System.ComponentModel.DataAnnotations.KeyAttribute]
        public long Long {
            get => _Long;
            set {
                if(EqualityComparer<long>.Default.Equals(_Long, value)) return;
                RaisePropertyChanging(LongChangingEventArgs);
                var oldValue = _Long;
                _Long = value;
                RaisePropertyChanged(LongChangedEventArgs);
                OnLongChanged(oldValue);
            }
        }
        public System.DateTime DateTime {
            get => _DateTime;
            set {
                if(EqualityComparer<System.DateTime>.Default.Equals(_DateTime, value)) return;
                RaisePropertyChanging(DateTimeChangingEventArgs);
                OnDateTimeChanging();
                _DateTime = value;
                RaisePropertyChanged(DateTimeChangedEventArgs);
            }
        }
        public virtual double Double {
            get => _Double;
            protected set {
                if(EqualityComparer<double>.Default.Equals(_Double, value)) return;
                RaisePropertyChanging(DoubleChangingEventArgs);
                _Double = value;
                RaisePropertyChanged(DoubleChangedEventArgs);
            }
        }
        public bool A {
            get => a;
            set {
                if(EqualityComparer<bool>.Default.Equals(a, value)) return;
                RaisePropertyChanging(AChangingEventArgs);
                a = value;
                RaisePropertyChanged(AChangedEventArgs);
            }
        }
        public bool B {
            get => b;
            set {
                if(EqualityComparer<bool>.Default.Equals(b, value)) return;
                RaisePropertyChanging(BChangingEventArgs);
                b = value;
                RaisePropertyChanged(BChangedEventArgs);
            }
        }
        RelayCommand<int?>? command1Command;
        /// <summary>
        /// Test command comment
        /// </summary>
        public RelayCommand<int?> Command1Command => command1Command ??= new RelayCommand<int?>(Command1, CanCommand1);
        RelayCommand? someCommand;
        public RelayCommand SomeCommand => someCommand ??= new RelayCommand(async () => await Command2(), null);
        RelayCommand<int?>? command3Command;
        public RelayCommand<int?> Command3Command => command3Command ??= new RelayCommand<int?>(Command3, CanCommand3_);
        static PropertyChangedEventArgs IntChangedEventArgs = new PropertyChangedEventArgs(nameof(Int));
        static PropertyChangedEventArgs StrChangedEventArgs = new PropertyChangedEventArgs(nameof(Str));
        static PropertyChangedEventArgs LongChangedEventArgs = new PropertyChangedEventArgs(nameof(Long));
        static PropertyChangedEventArgs DateTimeChangedEventArgs = new PropertyChangedEventArgs(nameof(DateTime));
        static PropertyChangedEventArgs DoubleChangedEventArgs = new PropertyChangedEventArgs(nameof(Double));
        static PropertyChangedEventArgs AChangedEventArgs = new PropertyChangedEventArgs(nameof(A));
        static PropertyChangedEventArgs BChangedEventArgs = new PropertyChangedEventArgs(nameof(B));
        static PropertyChangingEventArgs IntChangingEventArgs = new PropertyChangingEventArgs(nameof(Int));
        static PropertyChangingEventArgs StrChangingEventArgs = new PropertyChangingEventArgs(nameof(Str));
        static PropertyChangingEventArgs LongChangingEventArgs = new PropertyChangingEventArgs(nameof(Long));
        static PropertyChangingEventArgs DateTimeChangingEventArgs = new PropertyChangingEventArgs(nameof(DateTime));
        static PropertyChangingEventArgs DoubleChangingEventArgs = new PropertyChangingEventArgs(nameof(Double));
        static PropertyChangingEventArgs AChangingEventArgs = new PropertyChangingEventArgs(nameof(A));
        static PropertyChangingEventArgs BChangingEventArgs = new PropertyChangingEventArgs(nameof(B));
    }
}
";
            string generatedCode = GenerateCode(source);
            Assert.AreEqual(expected, generatedCode);

        }
        [Test]
        public void PrivateInSealedClass() {
            const string source =
@"using DevExpress.Mvvm.CodeGenerators.MvvmLight;
using GalaSoft.MvvmLight; 

namespace Test {
    [GenerateViewModel(ImplementINotifyPropertyChanging = true, ImplementICleanup = true)]
    sealed partial class SealdClass {
        [GenerateProperty]
        string str;
        [GenerateCommand]
        public void Command1(int arg) { }
    }
}";
            var generated = GenerateCode(source);
            StringAssert.DoesNotContain("protected", generated);
        }
        [Test]
        public void DoesNotGenerateIAA() {
            const string source =
@"using DevExpress.Mvvm.CodeGenerators.MvvmLight; 
using GalaSoft.MvvmLight;

namespace Test {
    class ParentClass : ICleanup {
        public void Cleanup() => throw new System.NotImplementedException();
}
    [GenerateViewModel(ImplementICleanup = true)]
    partial class ChildClass : ParentClass {
    }
}";
            var generated = GenerateCode(source);
            StringAssert.DoesNotContain("ICleanup", generated);
        }
    }
}

