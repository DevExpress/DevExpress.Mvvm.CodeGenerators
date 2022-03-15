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
        const string protectedModifier = "protected ";
        readonly bool isSealed;
        readonly bool isWinUI;
        public ISupportServicesGenerator(bool isSealed, bool isWinUI) {
            this.isSealed = isSealed;
            this.isWinUI = isWinUI;
        }
        public string GetName() => isWinUI ? "ISupportUIServices" : "ISupportServices";
        public void AppendImplementation(SourceBuilder source) {
            if(isWinUI) {
                source.AppendLine();
                source.AppendLine("IUIServiceContainer? serviceContainer;")
                      .AppendLine(@"IUIServiceContainer ServiceContainer => serviceContainer ??= new UIServiceContainer();")
                      .AppendLine(@"IUIServiceContainer ISupportUIServices.ServiceContainer => ServiceContainer;");
                source.AppendLine();
                source.AppendIf(!isSealed, protectedModifier)
                      .AppendLine("object? GetUIService(Type type, string key = null) => ServiceContainer.GetService(type, key);");
                source.AppendIf(!isSealed, protectedModifier)
                      .AppendLine("T? GetUIService<T>(string key = null) where T : class => ServiceContainer.GetService<T>(key);");
                return;
            }
            source.AppendLine("IServiceContainer? serviceContainer;")
                  .AppendIf(!isSealed, protectedModifier)
                  .AppendMultipleLines(@"IServiceContainer ServiceContainer { get => serviceContainer ??= new ServiceContainer(this); }
IServiceContainer ISupportServices.ServiceContainer { get => ServiceContainer; }");
            source.AppendIf(!isSealed, protectedModifier)
                  .AppendLine("T? GetService<T>() where T : class => ServiceContainer.GetService<T>();")
                  .AppendIf(!isSealed, protectedModifier)
                  .AppendLine("T? GetRequiredService<T>() where T : class => ServiceContainer.GetRequiredService<T>();");
        }
    }
    class IActiveAwareGenerator : IInterfaceGenerator {
        readonly bool generateChangedMethod;
        public IActiveAwareGenerator(bool shouldGenerateChangedMethod) => generateChangedMethod = shouldGenerateChangedMethod;
        public string GetName() => "IActiveAware";
        public void AppendImplementation(SourceBuilder source) {
            source.AppendMultipleLines(@"bool isActive;
public bool IsActive {
    get => isActive;
    set {
        isActive = value;");
            if(generateChangedMethod)
                source.AppendLine("        OnIsActiveChanged();");
            source.AppendMultipleLines(
@"        IsActiveChanged?.Invoke(this, EventArgs.Empty);
    }
}
public event EventHandler? IsActiveChanged;");
        }
    }
    class ICleanupGenerator : IInterfaceGenerator {
        readonly bool generateOnCleanupMethod;
        readonly bool isSealed;
        public ICleanupGenerator(bool shouldGenerateOnCleanupMethod, bool isSealed) {
            generateOnCleanupMethod = shouldGenerateOnCleanupMethod;
            this.isSealed = isSealed;
            }
        public string GetName() => "ICleanup";
        public void AppendImplementation(SourceBuilder source) {
            source.AppendIf(!isSealed, "protected ")
                  .AppendMultipleLines(@"IMessenger MessengerInstance { get; set; } = Messenger.Default;
public virtual void Cleanup() {
    MessengerInstance.Unregister(this);");
            if(generateOnCleanupMethod)
                source.AppendLine("    OnCleanup();");
            source.AppendLine("}");
        }
    }
}
