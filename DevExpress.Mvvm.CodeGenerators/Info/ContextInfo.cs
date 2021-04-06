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

        public ContextInfo(GeneratorExecutionContext context, Compilation compilation) {
            Context = context;
            Compilation = compilation;

            ViewModelAttributeSymbol = compilation.GetTypeByMetadataName(AttributesGenerator.ViewModelAttributeFullName);
            PropertyAttributeSymbol = compilation.GetTypeByMetadataName(AttributesGenerator.PropertyAttributeFullName);
            CommandAttributeSymbol = compilation.GetTypeByMetadataName(AttributesGenerator.CommandAttributeFullName);

            INPCedSymbol = compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanged).FullName);
            INPCingSymbol = compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanging).FullName);
            IDEISymbol = compilation.GetTypeByMetadataName(typeof(IDataErrorInfo).FullName);
            ISSSymbol = compilation.GetTypeByMetadataName("DevExpress.Mvvm.ISupportServices");
        }
    }
}
