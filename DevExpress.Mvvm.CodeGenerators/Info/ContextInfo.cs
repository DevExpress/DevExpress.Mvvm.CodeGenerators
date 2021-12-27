using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevExpress.Mvvm.CodeGenerators {
    class FrameworkAttributes {
        public INamedTypeSymbol? ViewModelAttributeSymbol { get; }
        public INamedTypeSymbol? PropertyAttributeSymbol { get; }
        public INamedTypeSymbol? CommandAttributeSymbol { get; }
        public FrameworkAttributes(Compilation compilation, SupportedMvvm mvvm) {
            string attributeNamespace = mvvm switch {
                SupportedMvvm.None or SupportedMvvm.Dx => InitializationGenerator.DxNamespace,
                SupportedMvvm.Prism => InitializationGenerator.PrismNamespace,
                _ => throw new InvalidOperationException()
            };
            ViewModelAttributeSymbol = compilation.GetTypeByMetadataName($"{attributeNamespace}.GenerateViewModelAttribute")!;
            PropertyAttributeSymbol = compilation.GetTypeByMetadataName($"{attributeNamespace}.GeneratePropertyAttribute")!;
            CommandAttributeSymbol = compilation.GetTypeByMetadataName($"{attributeNamespace}.GenerateCommandAttribute")!;
        }
    }
    class DXFrameworkAttributes : FrameworkAttributes {
        public INamedTypeSymbol IDEISymbol { get; }
        public INamedTypeSymbol ISPVMSymbol { get; }
        public INamedTypeSymbol ISSSymbol { get; }
        public DXFrameworkAttributes(Compilation compilation) 
            : base(compilation, SupportedMvvm.Dx) {
            IDEISymbol = compilation.GetTypeByMetadataName(typeof(IDataErrorInfo).FullName)!;
            ISSSymbol = compilation.GetTypeByMetadataName("DevExpress.Mvvm.ISupportServices")!;
            ISPVMSymbol = compilation.GetTypeByMetadataName("DevExpress.Mvvm.ISupportParentViewModel")!;
        }
    }
    class PrismFrameworkAttributes : FrameworkAttributes {
        public INamedTypeSymbol IAASymbol { get; }
        public PrismFrameworkAttributes(Compilation compilation) 
            : base(compilation, SupportedMvvm.Prism) {
            IAASymbol = compilation.GetTypeByMetadataName("Prism.IActiveAware")!;
        }
    }
    class ContextInfo {
        public GeneratorExecutionContext Context { get; }
        public Compilation Compilation { get; }

        public DXFrameworkAttributes? Dx { get; }
        public PrismFrameworkAttributes? Prism { get; }

        public INamedTypeSymbol INPCedSymbol { get; }
        public INamedTypeSymbol INPCingSymbol { get; }
        public INamedTypeSymbol TaskSymbol { get; }
        public INamedTypeSymbol BoolSymbol { get; }

        public bool IsWinUI { get; }
        public List<SupportedMvvm> AvailableMvvm { get; }

        public ContextInfo(GeneratorExecutionContext context, Compilation compilation) {
            Context = context;
            Compilation = compilation;

            AvailableMvvm = GetAvailableMvvm(compilation);

            if(AvailableMvvm.Contains(SupportedMvvm.Dx) || AvailableMvvm.Contains(SupportedMvvm.None))
                Dx = new DXFrameworkAttributes(Compilation);
            if(AvailableMvvm.Contains(SupportedMvvm.Prism))
                Prism = new PrismFrameworkAttributes(Compilation);

            INPCedSymbol = compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanged).FullName)!;
            INPCingSymbol = compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanging).FullName)!;

            TaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task")!;
            BoolSymbol = compilation.GetTypeByMetadataName("System.Boolean")!;

            IsWinUI = GetIsWinUI(compilation);
        }

        public FrameworkAttributes GetFrameworkAttributes(SupportedMvvm mvvm) => mvvm switch {
            SupportedMvvm.None or SupportedMvvm.Dx => Dx!,
            SupportedMvvm.Prism => Prism!,
            _ => throw new InvalidOperationException()
        };

        public static bool GetIsWinUI(Compilation compilation) => GetIsDxMvvmAvailable(compilation) && compilation.GetTypeByMetadataName("DevExpress.Mvvm.POCO.ViewModelSource") == null;
        static bool GetIsDxMvvmAvailable(Compilation compilation) => compilation.ReferencedAssemblyNames.Any(ai => Regex.IsMatch(ai.Name, @"DevExpress\.(WinUI.)?Mvvm(\.v\d{2}\.\d)?$"));
        static bool GetIsPrismAvailable(Compilation compilation) => compilation.GetTypeByMetadataName("Prism.Commands.DelegateCommand") != null;
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
    }
    internal enum SupportedMvvm {
        None = 0,
        Dx = 1,
        Prism = 2
    }
}
