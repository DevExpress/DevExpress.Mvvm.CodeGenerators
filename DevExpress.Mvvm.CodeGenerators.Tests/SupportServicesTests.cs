using NUnit.Framework;

namespace DevExpress.Mvvm.CodeGenerator.Tests {
    [GenerateViewModel(ImplementISupportServices = true)]
    partial class ClassWithSupportServices {
    }

    [TestFixture]
    public class SupportServicesTests {
        [Test]
        public void CreateServiceContainerOnce() {
            var generated = new ClassWithSupportServices();
            var serviceContainer1 = ((ISupportServices)generated).ServiceContainer;
            var serviceContainer2 = ((ISupportServices)generated).ServiceContainer;
            Assert.AreSame(serviceContainer1, serviceContainer2);
        }
    }
}
