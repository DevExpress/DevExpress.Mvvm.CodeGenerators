using NUnit.Framework;

namespace DevExpress.Mvvm.CodeGenerators.Tests {
    [GenerateViewModel(ImplementISupportServices = true)]
    partial class ClassWithSupportServices {
    }
    public interface IService { }
    public class TestService : IService { }
    [GenerateViewModel(ImplementISupportServices = true)]
    partial class GetServiceGenerate {
        public GetServiceGenerate(ServiceContainer serv) {
            serviceContainer = serv;
        }
        public IService GetService() {
            return GetService<IService>();
        }
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
        [Test]
        public void GenerateGetService() {
            var serv = new TestService();
            var serviceContainer = new ServiceContainer(null);
            serviceContainer.RegisterService(serv, false);
            var serviceImpl = new GetServiceGenerate(serviceContainer);

            Assert.IsNotNull(serviceImpl.GetType().GetMethod(nameof(GetServiceGenerate.GetService)));
            Assert.AreEqual(serv, serviceImpl.GetService());
        }

    }
}
