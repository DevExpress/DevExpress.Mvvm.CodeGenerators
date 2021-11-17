; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/master/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 21.1.0

### New Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|-------
DXCG0001 | DevExpress.Mvvm.CodeGenerators | error | Cannot generate the View Model
DXCG0002 | DevExpress.Mvvm.CodeGenerators | error | The base View Model class cannot be declared within a class
DXCG0003 | DevExpress.Mvvm.CodeGenerators | error | Cannot find the DevExpress.Mvvm assembly
DXCG0004 | DevExpress.Mvvm.CodeGenerators | error | The property name is invalid
DXCG0005 | DevExpress.Mvvm.CodeGenerators | error | Cannot find the On[Property]Changed or On[Property]Changing method
DXCG0006 | DevExpress.Mvvm.CodeGenerators | error | The method’s signature is invalid
DXCG0007 | DevExpress.Mvvm.CodeGenerators | error | Cannot find the CanExecute method
DXCG0008 | DevExpress.Mvvm.CodeGenerators | error | Cannot find Raise methods
DXCG0009 | DevExpress.Mvvm.CodeGenerators | error | Cannot generate the generic View Model
DXCG1001 | DevExpress.Mvvm.CodeGenerators | warning | The class contains two suitable methods

## Release 21.1.1

### Removed Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|-------
DXCG0002 | DevExpress.Mvvm.CodeGenerators | error | The base View Model class cannot be declared within a class
DXCG0009 | DevExpress.Mvvm.CodeGenerators | error | Cannot generate the generic View Model

## Release 21.2.1

### New Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
DXCG0010 | DevExpress.Mvvm.CodeGenerators | error | Class contains two GenerateViewModelAttribute
DXCG0011 | DevExpress.Mvvm.CodeGenerators | error | Method has Non-Nullable Argument

### Removed Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
DXCG0003 | DevExpress.Mvvm.CodeGenerators | error | Cannot find the DevExpress.Mvvm assembly
