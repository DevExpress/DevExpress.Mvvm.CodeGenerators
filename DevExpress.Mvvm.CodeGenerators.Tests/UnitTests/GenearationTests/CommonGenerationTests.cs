using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevExpress.Mvvm.CodeGenerators.Tests {
    [TestFixture]
    public class CommonGenerationTests {
        [Test]
        public void NoAnyMvvmUsing_Command() {
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
            StringAssert.DoesNotContain("using DevExpress.Mvvm;", generatedCode);
            StringAssert.DoesNotContain("using Prism.Commands;", generatedCode);
            StringAssert.DoesNotContain("MethodCommand", generatedCode);
        }
        [Test]
        public void FormattingTest() {
            const string source =
@"using DevExpress.Mvvm.CodeGenerators; 
using System;
namespace Test {
    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
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
    }
}";
            const string expected =
@"using System.Collections.Generic;
using System.ComponentModel;

#nullable enable

namespace Test {
    partial class Example0 : INotifyPropertyChanged, INotifyPropertyChanging {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event PropertyChangingEventHandler? PropertyChanging;

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
        [Test]
        public void PrivateInSealedClass() {
            const string source =
@"using DevExpress.Mvvm.CodeGenerators; 

namespace Test {
    [GenerateViewModel(ImplementINotifyPropertyChanging = true, ImplementISupportParentViewModel = true, ImplementISupportServices = true, ImplementIDataErrorInfo = true)]
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

        static string GenerateCode(string source) {
            return GeneratorHelper.GenerateCode(source, null);
        }
    }
}

