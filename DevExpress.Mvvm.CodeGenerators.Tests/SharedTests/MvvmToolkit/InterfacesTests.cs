using NUnit.Framework;
using System.ComponentModel;
using DevExpress.Mvvm.CodeGenerators.MvvmToolkit;
using GalaSoft.MvvmLight;

namespace MvvmToolkit.Mvvm.Tests {
    partial class SimpleClass { }
    [GenerateViewModel]
    partial class ClassWithGenerator { }

    [GenerateViewModel(ImplementINotifyPropertyChanging = false)]
    partial class NotImplementINPCing { }
    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class ImplementINPCing { }

    [GenerateViewModel]
    partial class ChildWithInheritedUserINPC : ViewModelBase { }

    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class GeneratedParent { }
    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class GeneratedChild : GeneratedParent {
        [GenerateProperty]
        int value;
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
