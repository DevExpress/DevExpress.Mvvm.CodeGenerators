# View Model Code Generator

The DevExpress MVVM Framework includes a [source generator](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.md) that produces boilerplate code for your View Models at compile time. You need to define a stub View Model class that defines the required logic. Our MVVM Framework analyzes your implementation and applied attributes to generate the final View Model class with all required boilerplate code.

## Prerequisites

Your project should meet the following requirements:
- C# 9+ (VB is not supported)
- .NET Framework v4.6.1+ or .NET Core v3.0+
- Visual Studio v16.9.0+

## Prepare Your Project

Prepare your project as outlined below to enable support for View Models generated at compile time:

1. Add a reference to the **DevExpress.Mvvm.v21.1**+ or install the [DevExpress.Mvvm](https://www.nuget.org/packages/DevExpressMvvm/) NuGet package.  
2. Install the **DevExpress.Mvvm.CodeGenerators** NuGet package in your project.
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
 ## See also
Refer to the following topic for more information:
- [View Models generated at compile time](https://docs.devexpress.com/WPF/402989/mvvm-framework/viewmodels/compile-time-generated-viewmodels)
