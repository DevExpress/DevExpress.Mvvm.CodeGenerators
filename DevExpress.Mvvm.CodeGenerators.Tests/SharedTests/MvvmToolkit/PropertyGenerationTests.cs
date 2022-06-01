using NUnit.Framework;
using System.ComponentModel;
using DevExpress.Mvvm.CodeGenerators.MvvmToolkit;

namespace MvvmToolkit.Mvvm.Tests {
    [GenerateViewModel(ImplementINotifyPropertyChanging = true)]
    partial class GenerateProperties {
        [GenerateProperty(IsVirtual = true)]
        int property;
    }
    [GenerateViewModel]
    partial class WithTwoMvvmAttribute {
        [DevExpress.Mvvm.CodeGenerators.GenerateProperty]
        int dxProperty;
        [DevExpress.Mvvm.CodeGenerators.GenerateCommand]
        void DxMethod() { }
    }

    [TestFixture]
    public class PropertyGenerationTests {
        [Test]
        public void PropertyImplementation() {
            var generated = new GenerateProperties();

            Assert.IsNotNull(generated.GetType().GetProperty("Property"));
            Assert.IsNull(generated.GetType().GetProperty("NotProperty"));
        }
        [Test]
        public void DoNotGenerateDxMembers() {
            var generated = new WithTwoMvvmAttribute();

            Assert.IsNull(generated.GetType().GetProperty("DxProperty"));
            Assert.IsNull(generated.GetType().GetProperty("DxMethodCommand"));
        }
    }
}
