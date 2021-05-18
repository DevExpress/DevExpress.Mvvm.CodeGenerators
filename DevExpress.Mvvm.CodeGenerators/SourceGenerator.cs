using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    [Generator]
    public class ViewModelGenerator : ISourceGenerator {
        public void Initialize(GeneratorInitializationContext context) {
            var attributesSourceText = SourceText.From(InitializationGenerator.GetSourceCode(), Encoding.UTF8);
            context.RegisterForPostInitialization((i) => i.AddSource(ClassHelper.CreateFileName("Attributes"), attributesSourceText));
            context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
        }

        public void Execute(GeneratorExecutionContext context) {
            new Generators.GeneratorCore().Execute(context);
        }
    }

    class SyntaxContextReceiver : ISyntaxContextReceiver {
        readonly List<ClassDeclarationSyntax> classSyntaxes = new();
        public IEnumerable<ClassDeclarationSyntax> ClassSyntaxes { get => classSyntaxes.ToArray(); }

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
                    //System.Diagnostics.Debugger.Launch();
            if(context.Node is ClassDeclarationSyntax classDeclarationSyntax) {
                var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
                if(!AttributeHelper.HasAttribute(classSymbol, context.SemanticModel.Compilation.GetTypeByMetadataName(AttributesGenerator.ViewModelAttributeFullName)))
                    return;
                classSyntaxes.Add(classDeclarationSyntax);
            }
        }
    }
}
