namespace DevExpress.Mvvm.CodeGenerators {
    static partial class GeneratorDiagnostics {
        const bool isEnabledByDefault = true;
        const string idPrefix = "DXCG";
        const string category = "DevExpress.Mvvm.CodeGenerators";

        const string noPartialModifierId = idPrefix + "0001";
        const string noPartialModifierTitle = "Cannot generate the View Model";
        const string noPartialModifierMessageFormat = "The '{0}' class must be partial";

        const string classWithinClassId = idPrefix + "0002";
        const string classWithinClassTitle = "The base View Model class cannot be declared within a class";
        const string classWithinClassMessageFormat = "The '{0}' class cannot be declared within a class";

        const string mvvmNotAvailableId = idPrefix + "0003";
        const string mvvmNotAvailableTitle = "Cannot find the DevExpress.Mvvm assembly";
        const string mvvmNotAvailableMessageFormat = "Add а reference to the DevExpress.Mvvm assembly to use '{0}' in the '{1}' class";

        const string invalidPropertyNameId = idPrefix + "0004";
        const string invalidPropertyNameTitle = "The property name is invalid";
        const string invalidPropertyNameMessageFormat = "The generated '{0}' property duplicates the bindable field’s name";

        const string onChangedMethodNotFoundId = idPrefix + "0005";
        const string onChangedMethodNotFoundTitle = "Cannot find the On[Property]Changed or On[Property]Changing method";
        const string onChangedMethodNotFoundMessageFormat = "Cannot find the 'void {0}()' or 'void {0}({1})'{2}";

        const string incorrectCommandSignatureId = idPrefix + "0006";
        const string incorrectCommandSignatureTitle = "The method’s signature is invalid";
        const string incorrectCommandSignatureMessageFormat = "Cannot create a command. '{0} {1}({2})' method must return 'void' or 'System.Threading.Tasks.Task' and have one or no parameters.";

        const string canExecuteMethodNotFoundId = idPrefix + "0007";
        const string canExecuteMethodNotFoundTitle = "Cannot find the CanExecute method";
        const string canExecuteMethodNotFoundMessageFormat = "Cannot find the 'bool {0}({1})' method{2}";

        const string raiseMethodNotFoundId = idPrefix + "0008";
        const string raiseMethodNotFoundTitle = "Cannot find Raise methods";
        const string raiseMethodNotFoundMessageFormat = "Cannot find the 'void RaisePropertyChang{0}(PropertyChang{0}EventArgs)' or 'void RaisePropertyChang{0}(string)' methods";

        const string twoSuitableMethodsId = idPrefix + "1001";
        const string twoSuitableMethodsTitle = "The class contains two suitable methods";
        const string twoSuitableMethodsMessageFormat = "The '{0}' contains two suitable methods: 'void {1}()' and 'void {1}({2})'. 'void {1}({2})' is used.";
    }
}
