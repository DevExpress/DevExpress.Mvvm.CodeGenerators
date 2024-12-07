using NUnit.Framework;
using System.ComponentModel;
using DevExpress.Mvvm.CodeGenerators.MvvmLight;
using GalaSoft.MvvmLight;

namespace MvvmLight.Mvvm.Tests {
    partial class SimpleClass { }
    [GenerateViewModel]
    partial class ClassWithGenerator { }

    [GenerateViewModel(ImplementINotifyPropertyChanging = false)]
    partial class NotImplementINPCing { }
    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class ImplementINPCing { }

    [GenerateViewModel(ImplementICleanup = false)]
    partial class NotImplementICU { }
    [GenerateViewModel(ImplementICleanup = true)]
    partial class ImplementICUParent { }
    [GenerateViewModel(ImplementICleanup = true)]
    partial class ImplementICUChild : ImplementICUParent { }

    [GenerateViewModel(ImplementINotifyPropertyChanging = true,
    ImplementICleanup = true)]
    partial class FullImplemented : INotifyPropertyChanged, INotifyPropertyChanging, ICleanup{
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;
        public void Cleanup() => throw new System.NotImplementedException();
    }

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
        public void DoubleImplementation() {
            var fullImpl = new FullImplemented();

            Assert.IsTrue(fullImpl is INotifyPropertyChanged);
            Assert.IsTrue(fullImpl is INotifyPropertyChanging);
            Assert.IsTrue(fullImpl is ICleanup);
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
        [Test]
        public void ICUImplementation() {
            var iaaDefault = new ClassWithGenerator();
            Assert.IsTrue(iaaDefault is not ICleanup);

            var parent = new ImplementICUParent();
            var child = new ImplementICUChild();

            Assert.IsTrue(parent is ICleanup);
            Assert.IsTrue(child is ICleanup);
        }
    }
}
