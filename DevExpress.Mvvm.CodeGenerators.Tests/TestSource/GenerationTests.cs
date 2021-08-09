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
        [Test]
        public void GenerateComments() {
            var source = @"using DevExpress.Mvvm.CodeGenerators;

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
        public void Method(int arg) { }
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
        public void ISPVMGenerateOnChanged() {
            var sourceWithOnChanged = @"
using DevExpress.Mvvm.CodeGenerators;
namespace Test {
    [GenerateViewModel(ImplementISupportParentViewModel = true)]
    partial class Example {
        void OnParentViewModelChanged(object o) { }
    }
}";
            var sourceWithOutOnChanged = @"
using DevExpress.Mvvm.CodeGenerators;
namespace Test {
    [GenerateViewModel(ImplementISupportParentViewModel = true)]
    partial class Example {
    }
}";
            var generatedWithOnChanged = GenerateCode(sourceWithOnChanged);
            var generatedWithOutOnChanged = GenerateCode(sourceWithOutOnChanged);
            StringAssert.Contains("OnParentViewModelChanged", generatedWithOnChanged);
            StringAssert.DoesNotContain("OnParentViewModelChanged", generatedWithOutOnChanged);
        }
        [Test]
        public void GeneratePropertyName_() {
            var source = @"using DevExpress.Mvvm.CodeGenerators;

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
#if !WINUI
        [Test]
        public void FormattingTest() {
            const string source =
@"using DevExpress.Mvvm.CodeGenerators; 
using System.Threading.Tasks; 
namespace Test {
    [GenerateViewModel(ImplementINotifyPropertyChanging = true, ImplementISupportParentViewModel = true, ImplementISupportServices = true, ImplementIDataErrorInfo = true)]
    partial class Example0 {
        [GenerateProperty]
        int _Int;

        [GenerateProperty(OnChangedMethod=""OnStrChanged_"", OnChangingMethod=""OnStrChanging_"")]
        string _Str;
        void OnStrChanged_() {}
        void OnStrChanging_(string newValue) {}

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
        public void Command1(int arg) { }
        bool CanCommand1(int arg) => true;

        [GenerateCommand(Name = ""SomeCommand"")]
        public Task Command2() => null
        bool CanCommand2(int arg) => true;

        [GenerateCommand(CanExecuteMethod = ""CanCommand3_"", UseCommandManager = true)]
        public void Command3(int arg) { }
        bool CanCommand3_(int arg) => true;

        void OnParentViewModelChanged(object o) { }
    }
}";
            const string expected =
@"using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.Mvvm;

#nullable enable

namespace Test {
    partial class Example0 : INotifyPropertyChanged, INotifyPropertyChanging, IDataErrorInfo, ISupportParentViewModel, ISupportServices {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event PropertyChangingEventHandler? PropertyChanging;
        string IDataErrorInfo.Error { get => string.Empty; }
        string IDataErrorInfo.this[string columnName] { get => IDataErrorInfoHelper.GetErrorText(this, columnName); }
        object? parentViewModel;
        public object? ParentViewModel {
            get { return parentViewModel; }
            set {
                if(parentViewModel == value)
                    return;
                if(value == this)
                    throw new System.InvalidOperationException(""ViewModel cannot be parent of itself."");
                parentViewModel = value;
                OnParentViewModelChanged(parentViewModel);
            }
        }
        IServiceContainer? serviceContainer;
        protected IServiceContainer ServiceContainer { get => serviceContainer ??= new ServiceContainer(this); }
        IServiceContainer ISupportServices.ServiceContainer { get => ServiceContainer; }
        T? GetService<T>() where T : class => ServiceContainer.GetService<T>();

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
        public DateTime? DateTime {
            get => _DateTime;
            set {
                if(EqualityComparer<DateTime?>.Default.Equals(_DateTime, value)) return;
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
        DelegateCommand<int>? command1Command;
        /// <summary>
        /// Test command comment
        /// </summary>
        public DelegateCommand<int> Command1Command {
            get => command1Command ??= new DelegateCommand<int>(Command1, CanCommand1, true);
        }
        AsyncCommand? someCommand;
        public AsyncCommand SomeCommand {
            get => someCommand ??= new AsyncCommand(Command2, null, false, true);
        }
        DelegateCommand<int>? command3Command;
        public DelegateCommand<int> Command3Command {
            get => command3Command ??= new DelegateCommand<int>(Command3, CanCommand3_, true);
        }
        static PropertyChangedEventArgs IntChangedEventArgs = new PropertyChangedEventArgs(nameof(Int));
        static PropertyChangedEventArgs StrChangedEventArgs = new PropertyChangedEventArgs(nameof(Str));
        static PropertyChangedEventArgs LongChangedEventArgs = new PropertyChangedEventArgs(nameof(Long));
        static PropertyChangedEventArgs DateTimeChangedEventArgs = new PropertyChangedEventArgs(nameof(DateTime));
        static PropertyChangedEventArgs DoubleChangedEventArgs = new PropertyChangedEventArgs(nameof(Double));
        static PropertyChangingEventArgs IntChangingEventArgs = new PropertyChangingEventArgs(nameof(Int));
        static PropertyChangingEventArgs StrChangingEventArgs = new PropertyChangingEventArgs(nameof(Str));
        static PropertyChangingEventArgs LongChangingEventArgs = new PropertyChangingEventArgs(nameof(Long));
        static PropertyChangingEventArgs DateTimeChangingEventArgs = new PropertyChangingEventArgs(nameof(DateTime));
        static PropertyChangingEventArgs DoubleChangingEventArgs = new PropertyChangingEventArgs(nameof(Double));
    }
}
";
            string generatedCode = GenerateCode(source);
            Assert.AreEqual(expected, generatedCode);

        }
#endif
    }
}
