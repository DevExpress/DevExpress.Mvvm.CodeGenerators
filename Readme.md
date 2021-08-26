# View Model Code Generator

The DevExpress MVVM Framework includes a [source generator](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.md) that produces boilerplate code for your View Models at compile time. You need to define a stub View Model class that defines the required logic. Our MVVM Framework analyzes your implementation and applied attributes to generate the final View Model class with all required boilerplate code.



https://user-images.githubusercontent.com/22204671/120662325-b28fc280-c491-11eb-9c8e-f104ee629fde.mp4





* **Base View Model**
    
    Create a partial class. Add attributes to the class and its fields/methods:

    ```csharp
    using DevExpress.Mvvm.CodeGenerators;

    [GenerateViewModel]
    partial class ViewModel {
        [GenerateProperty]
        string username;
        [GenerateProperty]
        string status;

        [GenerateCommand]
        void Login() => Status = "User: " + Username;
        bool CanLogin() => !string.IsNullOrEmpty(Username);
    }
    ```
    
* **Generated View Model**

    The generator inspects the base View Model and produces a partial class that complements your implementation with the following boilerplate code:
    
    * Properties
    * Property change notifications
    * Command declarations
    * INotifyPropertyChanged, INotifyPropertyChanging, IDataErrorInfo, ISupportServices implementation 
    
    You can view and debug the generated View Model:
  
    ```csharp   
    partial class ViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

        public string? Username {
            get => username;
            set {
                if(EqualityComparer<string?>.Default.Equals(username, value)) return;
                username = value;
                RaisePropertyChanged(UsernameChangedEventArgs);
            }
        }

        public string? Status {
            get => status;
            set {
                if(EqualityComparer<string?>.Default.Equals(status, value)) return;
                status = value;
                RaisePropertyChanged(StatusChangedEventArgs);
            }
        }

        DelegateCommand? loginCommand;
        public DelegateCommand LoginCommand {
            get => loginCommand ??= new DelegateCommand(Login, CanLogin, true);
        }

        static PropertyChangedEventArgs UsernameChangedEventArgs = new PropertyChangedEventArgs(nameof(Username));
        static PropertyChangedEventArgs StatusChangedEventArgs = new PropertyChangedEventArgs(nameof(Status));
    }
    ```

## Prerequisites

Your project should meet the following requirements:
- C# 9+ (VB is not supported)
- .NET Framework v4.6.1+ or .NET Core v3.0+
- Visual Studio v16.9.0+

## Prepare Your Project

Prepare your project as outlined below to enable support for View Models generated at compile time:

1. Add a reference to the **DevExpress.Mvvm.v21.1**+ or install the [DevExpress.Mvvm](https://www.nuget.org/packages/DevExpressMvvm/) NuGet package.  
2. Install the [DevExpress.Mvvm.CodeGenerators](https://www.nuget.org/packages/DevExpress.Mvvm.CodeGenerators/) NuGet package in your project.
3. Set the language version to **9** in the **.csproject** file:

    ```xaml
    <PropertyGroup>
        <LangVersion>9</LangVersion>
    </PropertyGroup>
    ```

    For .NET Core projects, set the **IncludePackageReferencesDuringMarkupCompilation** property to **true** additionally:

    ```xaml
    <PropertyGroup>
        <IncludePackageReferencesDuringMarkupCompilation>true</IncludePackageReferencesDuringMarkupCompilation>
    </PropertyGroup>
    ```
 
## Documentation
 
Refer to the following topic for more information: 
* [View Models Generated at Compile Time](https://docs.devexpress.com/WPF/402989/mvvm-framework/viewmodels/compile-time-generated-viewmodels)
 
## Example

Refer to the following GitHub example: 
* [How to: Use View Models Generated at Compile Time](https://github.com/DevExpress-Examples/ViewModelGenerator-Sample)

## Release Notes

### 21.1.2 
- You can now use the View Model Code Generator in WinUI projects.	
- You can add XML comments to a field and a method. The generated property or command now copies these comments.	
- We minimized memory allocations and improved the performance. 

### 21.1.1

- You can now use Generic and Nested classes when you generate View Models.
- Use the **ImplementISupportParentViewModel** property to implement [ISupportParentViewModel](https://docs.devexpress.com/WPF/17449/mvvm-framework/viewmodels/viewmodel-relationships-isupportparentviewmodel).
- Nullable annotation is supported.
- You can create attributes with custom arguments and apply these attributes to a field. A generated View Model now copies them. 
