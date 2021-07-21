using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    interface IInterfaceGenerator {
        string GetName();
        void AppendImplementation(StringBuilder source, int tabs);
    }

    class INPCedInterfaceGenerator : IInterfaceGenerator {
        public string GetName() => nameof(INotifyPropertyChanged);
        public void AppendImplementation(StringBuilder source, int tabs) 
            => source.AppendLineWithTabs("public event PropertyChangedEventHandler? PropertyChanged;", tabs);
    }
    class INPCingInterfaceGenerator : IInterfaceGenerator {
        public string GetName() => nameof(INotifyPropertyChanging);
        public void AppendImplementation(StringBuilder source, int tabs) => 
            source.AppendLineWithTabs("public event PropertyChangingEventHandler? PropertyChanging;", tabs);
    }
    class IDataErrorInfoGenerator : IInterfaceGenerator {
        const string Implementation = @"string IDataErrorInfo.Error { get => string.Empty; }
string IDataErrorInfo.this[string columnName] { get => IDataErrorInfoHelper.GetErrorText(this, columnName); }";
        public string GetName() => nameof(IDataErrorInfo);
        public void AppendImplementation(StringBuilder source, int tabs) => source.AppendMultipleLinesWithTabs(Implementation, tabs);
    }
    class ISupportParentViewModelGenerator : IInterfaceGenerator {
        readonly bool generateChangedMethod;
        readonly string onChangedMethod;
        public ISupportParentViewModelGenerator(bool shouldGenerateChangedMethod) {
            generateChangedMethod = shouldGenerateChangedMethod;
            onChangedMethod = generateChangedMethod ? System.Environment.NewLine + "        OnParentViewModelChanged(parentViewModel);" : string.Empty;
        }
        public string GetName() => "ISupportParentViewModel";
        public void AppendImplementation(StringBuilder source, int tabs) {
            source.AppendMultipleLinesWithTabs(@"object? parentViewModel;
public object? ParentViewModel {
    get { return parentViewModel; }
    set {
        if(parentViewModel == value)
            return;
        if(value == this)
            throw new System.InvalidOperationException(""ViewModel cannot be parent of itself."");
        parentViewModel = value;", tabs);
            source.AppendLineWithTabs("        OnParentViewModelChanged(parentViewModel);", tabs);
            source.AppendMultipleLinesWithTabs(
@"    }
}", tabs);
        }
    }
    class ISupportServicesGenerator : IInterfaceGenerator {
        const string Implementation = @"IServiceContainer? serviceContainer;
protected IServiceContainer ServiceContainer { get => serviceContainer ??= new ServiceContainer(this); }
IServiceContainer ISupportServices.ServiceContainer { get => ServiceContainer; }
T? GetService<T>() where T : class => ServiceContainer.GetService<T>();";
        public string GetName() => "ISupportServices";
        public void AppendImplementation(StringBuilder source, int tabs) => source.AppendMultipleLinesWithTabs(Implementation, tabs);

    }
}
