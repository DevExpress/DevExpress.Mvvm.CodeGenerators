using NUnit.Framework;
using System.ComponentModel;
using DevExpress.Mvvm.CodeGenerators.Prism;
using System;

namespace Prism.Mvvm.Tests {
    partial class SimpleClass { }
    [GenerateViewModel]
    partial class ClassWithGenerator { }

    [GenerateViewModel(ImplementINotifyPropertyChanging = false)]
    partial class NotImplementINPCing { }
    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class ImplementINPCing { }

    [GenerateViewModel]
    partial class ChildWithInheritedUserINPC : BindableBase { }

    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class GeneratedParent { }
    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class GeneratedChild : GeneratedParent {
        [GenerateProperty]
        int value;
    }
    [GenerateViewModel(ImplementIActiveAware = true)]
    partial class ImplementIAAParent { }

    [GenerateViewModel(ImplementIActiveAware = true)]
    partial class ImplementIAAChild : ImplementIAAParent { }

    [GenerateViewModel(ImplementINotifyPropertyChanging = true, ImplementIActiveAware = true)]
    partial class FullImplemented : INotifyPropertyChanged, INotifyPropertyChanging, IActiveAware{
        public bool IsActive { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;
        public event EventHandler IsActiveChanged;
        public FullImplemented() {
            //avoid warninngs
            IsActiveChanged?.Invoke(null, null);
        }
    }


    [TestFixture]
    public class IntefacesTests {
        [Test]
        public void INPCedImplimentation() {
            var withGenerator = new ClassWithGenerator();
            Assert.IsTrue(withGenerator is INotifyPropertyChanged);

            var withoutGenerator = new SimpleClass();
            Assert.IsTrue(withoutGenerator is not INotifyPropertyChanged);
        }

        [Test]
        public void INPCingImplementation() {
            var inpcingDefault = new ClassWithGenerator();
            Assert.IsTrue(inpcingDefault is not INotifyPropertyChanging);

            var inpcingImpl = new ImplementINPCing();
            Assert.IsTrue(inpcingImpl is INotifyPropertyChanging);

            var inpcingNotImpl = new NotImplementINPCing();
            Assert.IsTrue(inpcingNotImpl is not INotifyPropertyChanging);
        }

        [Test]
        public void IAAImplementation() {
            var iaaDefault = new ClassWithGenerator();
            Assert.IsTrue(iaaDefault is not IActiveAware);
            
            var parent = new ImplementIAAParent();
            var child = new ImplementIAAChild();
            Assert.IsTrue(parent is IActiveAware);
            Assert.IsTrue(child is IActiveAware);
        }

        [Test]
        public void DoubleImplementation() {
            var fullImpl = new FullImplemented();

            Assert.IsTrue(fullImpl is INotifyPropertyChanged);
            Assert.IsTrue(fullImpl is INotifyPropertyChanging);
            Assert.IsTrue(fullImpl is IActiveAware);
        }

        [Test]
        public void InheritanceUserINPC() {
            var child = new ChildWithInheritedUserINPC();

            Assert.IsTrue(child is INotifyPropertyChanged);
            Assert.IsTrue(child is not INotifyPropertyChanging);
        }

        [Test]
        public void InheritanceGeneratedINPC() {
            var parent = new GeneratedParent();
            var child = new GeneratedChild();

            Assert.IsTrue(parent is INotifyPropertyChanged);
            Assert.IsTrue(parent is INotifyPropertyChanging);
            Assert.IsTrue(child is INotifyPropertyChanged);
            Assert.IsTrue(child is INotifyPropertyChanging);
        }
    }
}
