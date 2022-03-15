using NUnit.Framework;
#if WINUI
using ISupportServices = DevExpress.Mvvm.ISupportUIServices;
using IServiceContainer = DevExpress.Mvvm.IUIServiceContainer;
using ServiceContainer = DevExpress.Mvvm.UIServiceContainer;
#endif

namespace DevExpress.Mvvm.CodeGenerators.Tests {
#if !WINUI
    [GenerateViewModel(ImplementISupportServices = true)]
#else
    [GenerateViewModel(ImplementISupportUIServices = true)]
#endif
    partial class ClassWithSupportServices { }
    public interface IService { }
    public class TestService : IService { }
#if !WINUI
    [GenerateViewModel(ImplementISupportServices = true)]
#else
    [GenerateViewModel(ImplementISupportUIServices = true)]
#endif
    partial class GetServiceGenerate {
        public GetServiceGenerate(ServiceContainer serv) {
            serviceContainer = serv;
        }
#if !WINUI
        public IService GetService() {
            return GetService<IService>();
        }
#else
        public IService GetUIService() {
            return GetUIService<IService>();
        }
#endif
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
#if !WINUI
            var serviceContainer = new ServiceContainer(null);
            serviceContainer.RegisterService(serv, false);
#else
            var serviceContainer = new ServiceContainer();
            serviceContainer.Register(serv);
#endif
            var serviceImpl = new GetServiceGenerate(serviceContainer);

#if !WINUI
            Assert.IsNotNull(serviceImpl.GetType().GetMethod(nameof(GetServiceGenerate.GetService)));
            Assert.AreEqual(serv, serviceImpl.GetService());
#else
            Assert.IsNotNull(serviceImpl.GetType().GetMethod(nameof(GetServiceGenerate.GetUIService)));
            Assert.AreEqual(serv, serviceImpl.GetUIService());
#endif
        }

    }
}
