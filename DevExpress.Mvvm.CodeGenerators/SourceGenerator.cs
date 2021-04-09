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
        public void Initialize(GeneratorInitializationContext context) =>
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

        public void Execute(GeneratorExecutionContext context) {
            if(context.SyntaxReceiver is not SyntaxReceiver receiver)
                return;

            var attributesSourceText = SourceText.From(InitializationGenerator.GetSourceCode(), Encoding.UTF8);
            context.AddSource(CreateFileName("Attributes"), attributesSourceText);
            var compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(attributesSourceText));
            var contextInfo = new ContextInfo(context, compilation);

            var generatedClasses = new List<string>();
            foreach(var classSyntax in receiver.ClassSyntaxes) {
                var classSymbol = contextInfo.Compilation.GetSemanticModel(classSyntax.SyntaxTree).GetDeclaredSymbol(classSyntax);

                if(generatedClasses.Contains(classSymbol.Name))
                    continue;

                if(!AttributeHelper.HasAttribute(classSymbol, contextInfo.ViewModelAttributeSymbol))
                    continue;

                if(classSymbol.IsGenericType) {
                    context.ReportGenericViewModel(classSymbol);
                    continue;
                }

                if(!classSyntax.Modifiers.Any(x => x.ValueText == "partial")) {
                    context.ReportNoPartialModifier(classSymbol);
                    continue;
                }

                if(!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default)) {
                    context.ReportClassWithinClass(classSymbol);
                    continue;
                }

                var classGenerator = new ClassGenerator(contextInfo, classSymbol);
                var classSource = classGenerator.GetSourceCode();
                context.AddSource(CreateFileName(classSymbol.Name), SourceText.From(classSource, Encoding.UTF8));
                generatedClasses.Add(classSymbol.Name);
            }
        }

        string CreateFileName(string prefix) => $"{prefix}_DXGenerator.cs";
    }

    class SyntaxReceiver : ISyntaxReceiver {
        readonly List<ClassDeclarationSyntax> classSyntaxes = new();
        public IEnumerable<ClassDeclarationSyntax> ClassSyntaxes { get => classSyntaxes.ToArray(); }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode) {
            if(syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
                classSyntaxes.Add(classDeclarationSyntax);
        }
    }
}
