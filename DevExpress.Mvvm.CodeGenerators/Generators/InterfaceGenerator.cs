using System.ComponentModel;

namespace DevExpress.Mvvm.CodeGenerators {
    interface IInterfaceGenerator {
        string GetName();
        string GetImplementation();
    }

    class INPCedInterfaceGenerator : IInterfaceGenerator {
        public string GetName() => nameof(INotifyPropertyChanged);
        public string GetImplementation() => "public event PropertyChangedEventHandler? PropertyChanged;";
    }
    class INPCingInterfaceGenerator : IInterfaceGenerator {
        public string GetName() => nameof(INotifyPropertyChanging);
        public string GetImplementation() => "public event PropertyChangingEventHandler? PropertyChanging;";
    }
    class IDataErrorInfoGenerator : IInterfaceGenerator {
        public string GetName() => nameof(IDataErrorInfo);
        public string GetImplementation() =>
@"string IDataErrorInfo.Error { get => string.Empty; }
string IDataErrorInfo.this[string columnName] { get => IDataErrorInfoHelper.GetErrorText(this, columnName); }";
    }
    class ISupportParentViewModelGenerator : IInterfaceGenerator {
        readonly bool generateChangedMethod;
        readonly string onChangedMethod;
        public ISupportParentViewModelGenerator(bool shouldGenerateChangedMethod) {
            generateChangedMethod = shouldGenerateChangedMethod;
            onChangedMethod = generateChangedMethod ? System.Environment.NewLine + "        OnParentViewModelChanged(parentViewModel);" : string.Empty;
        }
        public string GetName() => "ISupportParentViewModel";
        public string GetImplementation() =>
$@"object? parentViewModel;
object? ISupportParentViewModel.ParentViewModel {{
    get {{ return parentViewModel; }}
    set {{
        if(parentViewModel == value)
            return;
        if(value == this)
            throw new System.InvalidOperationException(""ViewModel cannot be parent of itself."");
        parentViewModel = value;{onChangedMethod}
    }}
}}";
    }
    class ISupportServicesGenerator : IInterfaceGenerator {
        public string GetName() => "ISupportServices";
        public string GetImplementation() =>
@"IServiceContainer? serviceContainer;
protected IServiceContainer ServiceContainer { get => serviceContainer ??= new ServiceContainer(this); }
IServiceContainer ISupportServices.ServiceContainer { get => ServiceContainer; }
T? GetService<T>() where T : class => ServiceContainer.GetService<T>();";
    }
}
