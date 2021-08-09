using Microsoft.CodeAnalysis;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevExpress.Mvvm.CodeGenerators {
    class ContextInfo {
        public GeneratorExecutionContext Context { get; }
        public Compilation Compilation { get; }

        public INamedTypeSymbol ViewModelAttributeSymbol { get; }
        public INamedTypeSymbol PropertyAttributeSymbol { get; }
        public INamedTypeSymbol CommandAttributeSymbol { get; }

        public INamedTypeSymbol INPCedSymbol { get; }
        public INamedTypeSymbol INPCingSymbol { get; }
        public INamedTypeSymbol IDEISymbol { get; }
        public INamedTypeSymbol? ISPVMSymbol { get; }
        public INamedTypeSymbol? ISSSymbol { get; }
        public INamedTypeSymbol TaskSymbol { get; }
        public INamedTypeSymbol BoolSymbol { get; }

        public bool IsWinUI { get; }
        public bool IsMvvmAvailable { get; }

        public ContextInfo(GeneratorExecutionContext context, Compilation compilation) {
            Context = context;
            Compilation = compilation;

            IsMvvmAvailable = GetIsMvvmAvailable(compilation);

            ViewModelAttributeSymbol = compilation.GetTypeByMetadataName(AttributesGenerator.ViewModelAttributeFullName)!;
            PropertyAttributeSymbol = compilation.GetTypeByMetadataName(AttributesGenerator.PropertyAttributeFullName)!;
            CommandAttributeSymbol = compilation.GetTypeByMetadataName(AttributesGenerator.CommandAttributeFullName)!;

            INPCedSymbol = compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanged).FullName)!;
            INPCingSymbol = compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanging).FullName)!;
            IDEISymbol = compilation.GetTypeByMetadataName(typeof(IDataErrorInfo).FullName)!;
            ISSSymbol = compilation.GetTypeByMetadataName("DevExpress.Mvvm.ISupportServices");
            ISPVMSymbol = compilation.GetTypeByMetadataName("DevExpress.Mvvm.ISupportParentViewModel");
            TaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task")!;
            BoolSymbol = compilation.GetTypeByMetadataName("System.Boolean")!;

            IsWinUI = GetIsWinUI(compilation);
        }
        public static bool GetIsWinUI(Compilation compilation) => GetIsMvvmAvailable(compilation) && compilation.GetTypeByMetadataName("DevExpress.Mvvm.POCO.ViewModelSource") == null;
        static bool GetIsMvvmAvailable(Compilation compilation) => compilation.ReferencedAssemblyNames.Any(ai => Regex.IsMatch(ai.Name, @"DevExpress\.Mvvm(\.v\d{2}\.\d)?$"));
    }
}
