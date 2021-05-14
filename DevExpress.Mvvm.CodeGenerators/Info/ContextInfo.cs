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
        public INamedTypeSymbol ISSSymbol { get; }

        public ContextInfo(GeneratorExecutionContext context) {
            Context = context;
            Compilation = context.Compilation;

            ViewModelAttributeSymbol = Compilation.GetTypeByMetadataName(AttributesGenerator.ViewModelAttributeFullName);
            PropertyAttributeSymbol = Compilation.GetTypeByMetadataName(AttributesGenerator.PropertyAttributeFullName);
            CommandAttributeSymbol = Compilation.GetTypeByMetadataName(AttributesGenerator.CommandAttributeFullName);

            INPCedSymbol = Compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanged).FullName);
            INPCingSymbol = Compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanging).FullName);
            IDEISymbol = Compilation.GetTypeByMetadataName(typeof(IDataErrorInfo).FullName);
            ISSSymbol = Compilation.GetTypeByMetadataName("DevExpress.Mvvm.ISupportServices");
        }
    }
}
