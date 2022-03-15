using NUnit.Framework;
using System.ComponentModel;

namespace DevExpress.Mvvm.CodeGenerators.Tests {
    class ViewModelParent {
        public int a = 0;
        public void OnParentViewModelChanged(object o) { a = 1; }
    }
#if !WINUI
    [GenerateViewModel(ImplementISupportParentViewModel = true)]
    partial class WithInheritedParentViewModelMethod : ViewModelParent {
    }
    [GenerateViewModel(ImplementISupportParentViewModel = true)]
    partial class WithParentViewModelMethod {
        public int a = 0;
        void OnParentViewModelChanged(object o) { a = 1; }
    }
#endif
    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class GenerateProperties {
        [GenerateProperty(IsVirtual = true)]
        int property;
    }
    [GenerateViewModel]
    partial class WithTwoMvvmAttribute {
        [Prism.GenerateProperty]
        int prismProperty;
        [Prism.GenerateCommand]
        void PrismMethod() { }
    }

    [TestFixture]
    public class PropertyGenerationTests {
        [Test]
        public void PropertyImplementation() {
            var generated = new GenerateProperties();

            Assert.IsNotNull(generated.GetType().GetProperty("Property"));
            Assert.IsNull(generated.GetType().GetProperty("NotProperty"));
        }
#if !WINUI
        [Test]
        public void ISupportParentViewModelTest() {
            var generatedWithParent = new WithInheritedParentViewModelMethod();
            var generated = new WithParentViewModelMethod();
            Assert.AreEqual(0, generatedWithParent.a);
            Assert.AreEqual(0, generated.a);
            generatedWithParent.ParentViewModel = new ViewModelParent();
            generated.ParentViewModel = new ViewModelParent();
            Assert.AreEqual(0, generatedWithParent.a);
            Assert.AreEqual(1, generated.a);
            Assert.Throws<System.InvalidOperationException>(() => generated.ParentViewModel = generated);
        }
#endif
        [Test]
        public void DoNotGeneratePrismMembers() {
            var generated = new WithTwoMvvmAttribute();
            Assert.IsNull(generated.GetType().GetProperty("PrismProperty"));
            Assert.IsNull(generated.GetType().GetProperty("PrismMethodCommand"));
        }
    }
}
