using DevExpress.Mvvm.Native;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevExpress.Mvvm.CodeGenerators.Tests {
    [TestFixture]
    public class GenerationTestsDx {
        static string GenerateCode(string source, bool addMVVM = true) {
            var generatorResult = GenerateCodeCore(source, addMVVM);
            var generatedCode = generatorResult.GeneratedSources[1].SourceText.ToString();
            return generatedCode;
        }
        static GeneratorRunResult GenerateCodeCore(string source, bool addMVVM = true) {
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
            return runResult.Results[0];
        }
        void AssertCode(string expected, string actual) {
            expected = Regex.Replace(expected, @"(\s*)\/\/.*", "");
            actual = Regex.Replace(actual, @"(\s*)\/\/.*", "");
            expected = Regex.Replace(expected, @"\r\n|\n\r|\n|\r", "\r\n");
            actual = Regex.Replace(actual, @"\r\n|\n\r|\n|\r", "\r\n");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AttributesTest() {
            var source = @"";
            var expected = @"using System;
using System.Collections.Generic;

#nullable enable

namespace DevExpress.Mvvm.CodeGenerators {
    enum AccessModifier {
        Public,
        Private,
        Protected,
        Internal,
        ProtectedInternal,
        PrivateProtected,
    }
    [AttributeUsage(AttributeTargets.Class)]
    class GenerateViewModelAttribute : Attribute {
        public bool ImplementINotifyPropertyChanging { get; set; }
        public bool ImplementISupportUIServices { get; set; }
    }
    [AttributeUsage(AttributeTargets.Field)]
    class GeneratePropertyAttribute : Attribute {
        public bool IsVirtual { get; set; }
        public string? OnChangedMethod { get; set; }
        public string? OnChangingMethod { get; set; }
        public AccessModifier SetterAccessModifier { get; set; }
    }
    [AttributeUsage(AttributeTargets.Method)]
    class GenerateCommandAttribute : Attribute {
        public bool AllowMultipleExecution { get; set; }
        public string? CanExecuteMethod { get; set; }
        public string? Name { get; set; }
    }
}
";
            var res = GenerateCodeCore(source).GeneratedSources[0].SourceText.ToString();
            AssertCode(expected, res);
        }
        [Test]
        public void PropertyTest() {
            var source = @"using DevExpress.Mvvm.CodeGenerators;
namespace Test {
    [GenerateViewModel]
    partial class Example {
        [GenerateProperty]
        int property1;

        [GenerateProperty(OnChangedMethod = nameof(OnProperty2Changed), OnChangingMethod = nameof(OnProperty2Changing))]
        int property2;

        void OnProperty2Changed(int v) { }
        void OnProperty2Changing(int v) { }
    }
}";
            var expected = @"using System;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.Mvvm;

#nullable enable

namespace Test {
    partial class Example : INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

        public int Property1 {
            get => property1;
            set {
                if(EqualityComparer<int>.Default.Equals(property1, value)) return;
                property1 = value;
                RaisePropertyChanged(Property1ChangedEventArgs);
            }
        }
        public int Property2 {
            get => property2;
            set {
                if(EqualityComparer<int>.Default.Equals(property2, value)) return;
                OnProperty2Changing(value);
                var oldValue = property2;
                property2 = value;
                RaisePropertyChanged(Property2ChangedEventArgs);
                OnProperty2Changed(oldValue);
            }
        }
        static PropertyChangedEventArgs Property1ChangedEventArgs = new PropertyChangedEventArgs(nameof(Property1));
        static PropertyChangedEventArgs Property2ChangedEventArgs = new PropertyChangedEventArgs(nameof(Property2));
    }
}
";
            var res = GenerateCode(source);
            AssertCode(expected, res);
        }
        [Test]
        public void ImplementINotifyPropertyChangingTest() {
            var source = @"using DevExpress.Mvvm.CodeGenerators;
namespace Test {
    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class Example {
        [GenerateProperty]
        int property1;

        [GenerateProperty(OnChangedMethod = nameof(OnProperty2Changed), OnChangingMethod = nameof(OnProperty2Changing))]
        int property2;

        void OnProperty2Changed(int v) { }
        void OnProperty2Changing(int v) { }
    }
}";
            var expected = @"using System;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.Mvvm;

#nullable enable

namespace Test {
    partial class Example : INotifyPropertyChanged, INotifyPropertyChanging {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event PropertyChangingEventHandler? PropertyChanging;

        protected void RaisePropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);
        protected void RaisePropertyChanging(PropertyChangingEventArgs e) => PropertyChanging?.Invoke(this, e);

        public int Property1 {
            get => property1;
            set {
                if(EqualityComparer<int>.Default.Equals(property1, value)) return;
                RaisePropertyChanging(Property1ChangingEventArgs);
                property1 = value;
                RaisePropertyChanged(Property1ChangedEventArgs);
            }
        }
        public int Property2 {
            get => property2;
            set {
                if(EqualityComparer<int>.Default.Equals(property2, value)) return;
                RaisePropertyChanging(Property2ChangingEventArgs);
                OnProperty2Changing(value);
                var oldValue = property2;
                property2 = value;
                RaisePropertyChanged(Property2ChangedEventArgs);
                OnProperty2Changed(oldValue);
            }
        }
        static PropertyChangedEventArgs Property1ChangedEventArgs = new PropertyChangedEventArgs(nameof(Property1));
        static PropertyChangedEventArgs Property2ChangedEventArgs = new PropertyChangedEventArgs(nameof(Property2));
        static PropertyChangingEventArgs Property1ChangingEventArgs = new PropertyChangingEventArgs(nameof(Property1));
        static PropertyChangingEventArgs Property2ChangingEventArgs = new PropertyChangingEventArgs(nameof(Property2));
    }
}
";
            var res = GenerateCode(source);
            AssertCode(expected, res);
        }
        [Test]
        public void ImplementISupportUIServicesTest() {
            var source = @"using DevExpress.Mvvm.CodeGenerators;
namespace Test {
    [GenerateViewModel(ImplementISupportUIServices = true)]
    partial class Example {
        [GenerateProperty]
        int property1;
    }
}";
            var expected = @"using System;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.Mvvm;

#nullable enable

namespace Test {
    partial class Example : INotifyPropertyChanged, ISupportUIServices {
        public event PropertyChangedEventHandler? PropertyChanged;

        IUIServiceContainer? serviceContainer;
        IUIServiceContainer ServiceContainer => serviceContainer ??= new UIServiceContainer();
        IUIServiceContainer ISupportUIServices.ServiceContainer => ServiceContainer;

        protected object? GetUIService(Type type, string key = null) => ServiceContainer.GetService(type, key);
        protected T? GetUIService<T>(string key = null) where T : class => ServiceContainer.GetService<T>(key);

        protected void RaisePropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

        public int Property1 {
            get => property1;
            set {
                if(EqualityComparer<int>.Default.Equals(property1, value)) return;
                property1 = value;
                RaisePropertyChanged(Property1ChangedEventArgs);
            }
        }
        static PropertyChangedEventArgs Property1ChangedEventArgs = new PropertyChangedEventArgs(nameof(Property1));
    }
}
";
            var res = GenerateCode(source);
            AssertCode(expected, res);
        }
        [Test]
        public void CommandTest() {
            var source = @"using DevExpress.Mvvm.CodeGenerators;
namespace Test {
    [GenerateViewModel]
    partial class Example {
        [GenerateCommand]
        void Do1() { }

        [GenerateCommand]
        void Do2(int p) { }
        bool CanDo2(int p) => true;
    }
}";
            var expected = @"using System;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.Mvvm;

#nullable enable

namespace Test {
    partial class Example : INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

        DelegateCommand? do1Command;
        public DelegateCommand Do1Command => do1Command ??= new DelegateCommand(Do1, null);
        DelegateCommand<int>? do2Command;
        public DelegateCommand<int> Do2Command => do2Command ??= new DelegateCommand<int>(Do2, CanDo2);
    }
}
";
            var res = GenerateCode(source);
            AssertCode(expected, res);
        }
        [Test]
        public void AsyncCommandTest() {
            var source = @"using DevExpress.Mvvm.CodeGenerators;
using System.Threading;
using System.Threading.Tasks;

namespace Test {
    [GenerateViewModel]
    partial class Example {
        [GenerateCommand]
        Task Do1() => null;

        [GenerateCommand]
        Task Do2(int p) => null;
        bool CanDo2(int p) => true;

        [GenerateCommand]
        Task Do3(CancellationToken t) => null;
        bool CanDo3() => true;

        [GenerateCommand]
        Task Do4(int p, CancellationToken t) => null;
        bool CanDo4(int p) => true;
    }
}";
            var expected = @"using System;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.Mvvm;

#nullable enable

namespace Test {
    partial class Example : INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

        AsyncCommand? do1Command;
        public AsyncCommand Do1Command => do1Command ??= new AsyncCommand(Do1, null, false);
        AsyncCommand<int>? do2Command;
        public AsyncCommand<int> Do2Command => do2Command ??= new AsyncCommand<int>(Do2, CanDo2, false);
        AsyncCommand? do3Command;
        public AsyncCommand Do3Command => do3Command ??= new AsyncCommand(Do3, CanDo3, false);
        AsyncCommand<int>? do4Command;
        public AsyncCommand<int> Do4Command => do4Command ??= new AsyncCommand<int>(Do4, CanDo4, false);
    }
}
";
            var res = GenerateCode(source);
            AssertCode(expected, res);
        }
    }
}

