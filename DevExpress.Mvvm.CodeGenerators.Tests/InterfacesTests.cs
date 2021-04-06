using NUnit.Framework;
using System.ComponentModel;

namespace DevExpress.Mvvm.CodeGenerators.Tests {
    partial class SimpleClass { }
    [GenerateViewModel]
    partial class ClassWithGenerator { }

    [GenerateViewModel(ImplementINotifyPropertyChanging = false)]
    partial class NotImplementINPCing { }
    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class ImplementINPCing { }

    [GenerateViewModel(ImplementIDataErrorInfo = false)]
    partial class NotImplementIDEI { }
    [GenerateViewModel(ImplementIDataErrorInfo = true)]
    partial class ImplementIDEI { }

    [GenerateViewModel(ImplementISupportServices = false)]
    partial class NotImplementISS { }
    [GenerateViewModel(ImplementISupportServices = true)]
    partial class ImplementISS { }

    [GenerateViewModel(ImplementINotifyPropertyChanging = true, ImplementIDataErrorInfo = true, ImplementISupportServices = true)]
    partial class FullImplemented : INotifyPropertyChanged, INotifyPropertyChanging, IDataErrorInfo, ISupportServices {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;
        string IDataErrorInfo.this[string columnName] { get => string.Empty; }
        string IDataErrorInfo.Error { get => string.Empty; }
        IServiceContainer ISupportServices.ServiceContainer => throw new System.NotImplementedException();
    }

    [GenerateViewModel]
    partial class ChildWithInheritedUserINPC : BindableBase { }

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
        public void IDEIImplementation() {
            var ideiDefault = new ClassWithGenerator();
            Assert.IsTrue(ideiDefault is not IDataErrorInfo);

            var ideiImpl = new ImplementIDEI();
            Assert.IsTrue(ideiImpl is IDataErrorInfo);

            var ideiNotImpl = new NotImplementIDEI();
            Assert.IsTrue(ideiNotImpl is not IDataErrorInfo);
        }

        [Test]
        public void ISSImplementation() {
            var issDefault = new ClassWithGenerator();
            Assert.IsTrue(issDefault is not ISupportServices);

            var issImpl = new ImplementISS();
            Assert.IsTrue(issImpl is ISupportServices);

            var issNotImpl = new NotImplementISS();
            Assert.IsTrue(issNotImpl is not ISupportServices);
        }

        [Test]
        public void DoubleImplementation() {
            var fullImpl = new FullImplemented();

            Assert.IsTrue(fullImpl is INotifyPropertyChanged);
            Assert.IsTrue(fullImpl is INotifyPropertyChanging);
            Assert.IsTrue(fullImpl is IDataErrorInfo);
            Assert.IsTrue(fullImpl is ISupportServices);
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
