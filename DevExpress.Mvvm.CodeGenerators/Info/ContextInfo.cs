using Microsoft.CodeAnalysis;
using System.ComponentModel;

namespace DevExpress.Mvvm.CodeGenerators {
    struct ContextInfo {
        public GeneratorExecutionContext Context { get; }
        public Compilation Compilation { get; }

        public INamedTypeSymbol ViewModelAttributeSymbol { get; }
        public INamedTypeSymbol PropertyAttributeSymbol { get; }
        public INamedTypeSymbol CommandAttributeSymbol { get; }

        public INamedTypeSymbol INPCedSymbol { get; }
        public INamedTypeSymbol INPCingSymbol { get; }
        public INamedTypeSymbol IDEISymbol { get; }
        public INamedTypeSymbol ISPVMSymbol { get; }
        public INamedTypeSymbol ISSSymbol { get; }

        public bool IsWinUI { get; }

        public ContextInfo(GeneratorExecutionContext context, Compilation compilation) {
            Context = context;
            Compilation = compilation;

            ViewModelAttributeSymbol = compilation.GetTypeByMetadataName(AttributesGenerator.ViewModelAttributeFullName);
            PropertyAttributeSymbol = compilation.GetTypeByMetadataName(AttributesGenerator.PropertyAttributeFullName);
            CommandAttributeSymbol = compilation.GetTypeByMetadataName(AttributesGenerator.CommandAttributeFullName);

            INPCedSymbol = compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanged).FullName);
            INPCingSymbol = compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanging).FullName);
            IDEISymbol = compilation.GetTypeByMetadataName(typeof(IDataErrorInfo).FullName);
            ISSSymbol = GetISSSymbol(compilation);
            ISPVMSymbol = compilation.GetTypeByMetadataName("DevExpress.Mvvm.ISupportParentViewModel");

            IsWinUI = GetIsWinUI(compilation);
        }
        public static bool GetIsWinUI(Compilation compilation) {
            return GetISSSymbol(compilation) != null && compilation.GetTypeByMetadataName("DevExpress.Mvvm.POCO.ViewModelSource") == null;
        }
        public static INamedTypeSymbol GetISSSymbol(Compilation compilation) {
            return compilation.GetTypeByMetadataName("DevExpress.Mvvm.ISupportServices");
        }
    }
}
