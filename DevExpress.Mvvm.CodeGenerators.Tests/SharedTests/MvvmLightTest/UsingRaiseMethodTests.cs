using NUnit.Framework;
using System;
using System.ComponentModel;
using DevExpress.Mvvm.CodeGenerators.MvvmLight;
using GalaSoft.MvvmLight;

namespace MvvmLight.Mvvm.Tests {
    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class DefaultRaiseMethod {
        [GenerateProperty]
        int value;
    }

    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class ParentWithGeneraetedRaiseMethod { }
    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class ChildWithParentsGeneratedRaiseMethod : ParentWithGeneraetedRaiseMethod {
        [GenerateProperty]
        int value;
    }

    class ParentWithoutRaiseMethod : INotifyPropertyChanged, INotifyPropertyChanging {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;
    }
    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class ChildWithoutRaiseMethod : ParentWithoutRaiseMethod {
        // Has error DXCG0008! Look at DiagnosticTest.cs/RaiseMethodNotFoundDiagnostic()
    }

    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class ParentWithImplementationAndGeneratedRaiseMethod : INotifyPropertyChanged, INotifyPropertyChanging {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;
    }
    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class ChildWithParentsGeneratedRaiseMethod2 : ParentWithImplementationAndGeneratedRaiseMethod {
        [GenerateProperty]
        int value;
    }

    class ParentWithEventArgsParameterRaiseMethod : INotifyPropertyChanged, INotifyPropertyChanging {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        protected void RaisePropertyChanged(PropertyChangedEventArgs eventArgs) => PropertyChanged?.Invoke(this, eventArgs);
        protected void RaisePropertyChanging(PropertyChangingEventArgs eventArgs) => PropertyChanging?.Invoke(this, eventArgs);
    }
    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class ChildWithParentsImplementedRaiseMethod : ParentWithEventArgsParameterRaiseMethod {
        [GenerateProperty]
        int value;
    }

    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class ChildWithParentsStringParameterRaiseMethod4 : ViewModelBase {
        [GenerateProperty]
        int value;
    }

    [TestFixture]
    class UsingRaiseMethodTests {
        [Test]
        public void DefaultRaiseMethod() {
            var generated = new DefaultRaiseMethod();
            Assert.Throws<Exception>(() =>
                DoWith.PropertyChangedEvent(
                    generated,
                    () => DoWith.PropertyChangingEvent(
                                    generated,
                                    () => generated.Value = 1,
                                    e => throw new Exception()),
                    e => throw new Exception())
                );
        }

        [Test]
        public void ParentsGeneratedRaiseMethod() {
            var generated = new ChildWithParentsGeneratedRaiseMethod();
            Assert.Throws<Exception>(() =>
                DoWith.PropertyChangedEvent(
                    generated,
                    () => DoWith.PropertyChangingEvent(
                                    generated,
                                    () => generated.Value = 1,
                                    e => throw new Exception()),
                    e => throw new Exception())
                );
        }

        [Test]
        public void ParentWithImplementationAndGeneratedRaiseMethod() {
            var generated = new ChildWithParentsGeneratedRaiseMethod2();
            Assert.Throws<Exception>(() =>
            DoWith.PropertyChangedEvent(
                    generated,
                    () => DoWith.PropertyChangingEvent(
                                    generated,
                                    () => generated.Value = 1,
                                    e => throw new Exception()),
                    e => throw new Exception())
            );
        }

        [Test]
        public void ParentWithEventArgsParameterRaiseMethod() {
            var generated = new ChildWithParentsImplementedRaiseMethod();
            Assert.Throws<Exception>(() =>
                DoWith.PropertyChangedEvent(
                    generated,
                    () => DoWith.PropertyChangingEvent(
                                    generated,
                                    () => generated.Value = 1,
                                    e => throw new Exception()),
                    e => throw new Exception())
                );
        }

        [Test]
        public void ParentWithStringParameterRaiseMethod() {
            var generated = new ChildWithParentsStringParameterRaiseMethod4();
            Assert.Throws<Exception>(() =>
                DoWith.PropertyChangedEvent(
                    generated,
                    () => DoWith.PropertyChangingEvent(
                                    generated,
                                    () => generated.Value = 1,
                                    e => throw new Exception()),
                    e => throw new Exception())
                );
        }
    }
}
