using System.ComponentModel;

namespace DevExpress.Mvvm.CodeGenerators {
    interface IInterfaceGenerator {
        string GetName();
        void AppendImplementation(SourceBuilder source);
    }

    class INPCedInterfaceGenerator : IInterfaceGenerator {
        public string GetName() => nameof(INotifyPropertyChanged);
        public void AppendImplementation(SourceBuilder source) =>
            source.AppendLine("public event PropertyChangedEventHandler? PropertyChanged;");
    }
    class INPCingInterfaceGenerator : IInterfaceGenerator {
        public string GetName() => nameof(INotifyPropertyChanging);
        public void AppendImplementation(SourceBuilder source) =>
            source.AppendLine("public event PropertyChangingEventHandler? PropertyChanging;");
    }
    class IDataErrorInfoGenerator : IInterfaceGenerator {
        const string Implementation = @"string IDataErrorInfo.Error { get => string.Empty; }
string IDataErrorInfo.this[string columnName] { get => IDataErrorInfoHelper.GetErrorText(this, columnName); }";
        public string GetName() => nameof(IDataErrorInfo);
        public void AppendImplementation(SourceBuilder source) => source.AppendMultipleLines(Implementation);
    }
    class ISupportParentViewModelGenerator : IInterfaceGenerator {
        readonly bool generateChangedMethod;
        public ISupportParentViewModelGenerator(bool shouldGenerateChangedMethod) => generateChangedMethod = shouldGenerateChangedMethod;
        public string GetName() => "ISupportParentViewModel";
        public void AppendImplementation(SourceBuilder source) {
            source.AppendMultipleLines(@"object? parentViewModel;
public object? ParentViewModel {
    get { return parentViewModel; }
    set {
        if(parentViewModel == value)
            return;
        if(value == this)
            throw new System.InvalidOperationException(""ViewModel cannot be parent of itself."");
        parentViewModel = value;");
            if(generateChangedMethod)
                source.AppendLine("        OnParentViewModelChanged(parentViewModel);");
            source.AppendMultipleLines(
@"    }
}");
        }
    }
    class ISupportServicesGenerator : IInterfaceGenerator {
        readonly bool isSealed;
        public ISupportServicesGenerator(bool isSealed) => this.isSealed = isSealed;
        public string GetName() => "ISupportServices";
        public void AppendImplementation(SourceBuilder source) {
            source.AppendLine("IServiceContainer? serviceContainer;")
                  .AppendMultipleLinesIf(!isSealed, "protected ", @"IServiceContainer ServiceContainer { get => serviceContainer ??= new ServiceContainer(this); }
IServiceContainer ISupportServices.ServiceContainer { get => ServiceContainer; }");
            source.AppendLineIf(!isSealed, "protected ", "T? GetService<T>() where T : class => ServiceContainer.GetService<T>();")
                  .AppendLineIf(!isSealed, "protected ", "T? GetRequiredService<T>() where T : class => ServiceContainer.GetRequiredService<T>();");
        }
    }
}
