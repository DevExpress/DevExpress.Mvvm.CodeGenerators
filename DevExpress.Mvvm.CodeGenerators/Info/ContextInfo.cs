using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevExpress.Mvvm.CodeGenerators {
    class ContextInfo {
        public GeneratorExecutionContext Context { get; }
        public Compilation Compilation { get; }

        public INamedTypeSymbol DxViewModelAttributeSymbol { get; }
        public INamedTypeSymbol PrismViewModelAttributeSymbol { get; }

        public INamedTypeSymbol? ViewModelAttributeSymbol { get; private set; }
        public INamedTypeSymbol? PropertyAttributeSymbol { get; private set; }
        public INamedTypeSymbol? CommandAttributeSymbol { get; private set; }

        public INamedTypeSymbol INPCedSymbol { get; }
        public INamedTypeSymbol INPCingSymbol { get; }
        public INamedTypeSymbol IDEISymbol { get; }
        public INamedTypeSymbol? ISPVMSymbol { get; }
        public INamedTypeSymbol? ISSSymbol { get; }
        public INamedTypeSymbol TaskSymbol { get; }
        public INamedTypeSymbol BoolSymbol { get; }
        public INamedTypeSymbol? IAASymbol { get; }

        public bool IsWinUI { get; }
        public List<SupportedMvvm> AvailableMvvm { get; }
        public SupportedMvvm ActualMvvm { get; set; }

        public ContextInfo(GeneratorExecutionContext context, Compilation compilation) {
            Context = context;
            Compilation = compilation;
            
            DxViewModelAttributeSymbol = compilation.GetTypeByMetadataName($"{InitializationGenerator.DxNamespace}.GenerateViewModelAttribute")!;
            PrismViewModelAttributeSymbol = compilation.GetTypeByMetadataName($"{InitializationGenerator.PrismNamespace}.GenerateViewModelAttribute")!;
            
            AvailableMvvm = GetAvailableMvvm(compilation);

            INPCedSymbol = compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanged).FullName)!;
            INPCingSymbol = compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanging).FullName)!;
            IDEISymbol = compilation.GetTypeByMetadataName(typeof(IDataErrorInfo).FullName)!;
            ISSSymbol = compilation.GetTypeByMetadataName("DevExpress.Mvvm.ISupportServices");
            ISPVMSymbol = compilation.GetTypeByMetadataName("DevExpress.Mvvm.ISupportParentViewModel");

            IAASymbol = compilation.GetTypeByMetadataName("Prism.IActiveAware");

            TaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task")!;
            BoolSymbol = compilation.GetTypeByMetadataName("System.Boolean")!;

            IsWinUI = GetIsWinUI(compilation);
        }

        public void SetMvvm(SupportedMvvm mvvm) {
            ActualMvvm = mvvm;
            string attributeNamespace = GetNamespase(mvvm);
            ViewModelAttributeSymbol = Compilation.GetTypeByMetadataName($"{attributeNamespace}.GenerateViewModelAttribute")!;
            PropertyAttributeSymbol = Compilation.GetTypeByMetadataName($"{attributeNamespace}.GeneratePropertyAttribute")!;
            CommandAttributeSymbol = Compilation.GetTypeByMetadataName($"{attributeNamespace}.GenerateCommandAttribute")!;
        }

        string GetNamespase(SupportedMvvm mvvm) => mvvm switch {
            SupportedMvvm.None => InitializationGenerator.DxNamespace,
            SupportedMvvm.Dx => InitializationGenerator.DxNamespace,
            SupportedMvvm.Prism => InitializationGenerator.PrismNamespace,
            _ => string.Empty
        };


        public static bool GetIsWinUI(Compilation compilation) => GetIsDxMvvmAvailable(compilation) && compilation.GetTypeByMetadataName("DevExpress.Mvvm.POCO.ViewModelSource") == null;
        static bool GetIsDxMvvmAvailable(Compilation compilation) => compilation.ReferencedAssemblyNames.Any(ai => Regex.IsMatch(ai.Name, @"DevExpress\.Mvvm(\.v\d{2}\.\d)?$"));
        static bool GetIsPrismAvailable(Compilation compilation) => compilation.GetTypeByMetadataName("Prism.Commands.DelegateCommand") != null;  //ReferencedAssemblyNames.Any(ai => Regex.IsMatch(ai.Name, @"Prism\..*(Wpf|Core)"));
        public static List<SupportedMvvm> GetAvailableMvvm(Compilation compilation) {
            List<SupportedMvvm> available = new();
            if(GetIsDxMvvmAvailable(compilation))
                available.Add(SupportedMvvm.Dx);
            if(GetIsPrismAvailable(compilation))
                available.Add(SupportedMvvm.Prism);
            if(!available.Any())
                available.Add(SupportedMvvm.None);
            return available;
        }
        //public void SetActualMvvm(INamedTypeSymbol classSymbol) {
        //    //var viewModelAttribute = Compilation.GetType
        //}
    }
    internal enum SupportedMvvm {
        None = 0,
        Dx = 1,
        Prism = 2
    }
}
