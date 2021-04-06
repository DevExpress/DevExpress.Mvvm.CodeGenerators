using System.ComponentModel;

namespace DevExpress.Mvvm.CodeGenerators {
    interface IInterfaceGenerator {
        string GetName();
        string GetImplementation();
    }

    class INPCedInterfaceGenerator : IInterfaceGenerator {
        public string GetName() => nameof(INotifyPropertyChanged);
        public string GetImplementation() => "public event PropertyChangedEventHandler PropertyChanged;";
    }
    class INPCingInterfaceGenerator : IInterfaceGenerator {
        public string GetName() => nameof(INotifyPropertyChanging);
        public string GetImplementation() => "public event PropertyChangingEventHandler PropertyChanging;";
    }
    class IDataErrorInfoGenerator : IInterfaceGenerator {
        public string GetName() => nameof(IDataErrorInfo);
        public string GetImplementation() =>
@"string IDataErrorInfo.Error { get => string.Empty; }
string IDataErrorInfo.this[string columnName] { get => IDataErrorInfoHelper.GetErrorText(this, columnName); }";
    }
    class ISupportServicesGenerator : IInterfaceGenerator {
        public string GetName() => "ISupportServices";
        public string GetImplementation() =>
@"IServiceContainer serviceContainer;
protected IServiceContainer ServiceContainer { get => serviceContainer ??= new ServiceContainer(this); }
IServiceContainer ISupportServices.ServiceContainer { get => ServiceContainer; }";
    }
}
