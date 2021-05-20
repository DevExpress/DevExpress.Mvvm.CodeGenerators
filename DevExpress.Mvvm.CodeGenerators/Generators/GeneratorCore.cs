using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DevExpress.Mvvm.CodeGenerators {
    public class ViewModelGeneratorCore {
        public void Execute(GeneratorExecutionContext context) {
            if(context.SyntaxContextReceiver is not SyntaxContextReceiver receiver)
                return;

            var attributesSourceText = SourceText.From(InitializationGenerator.GetSourceCode(), Encoding.UTF8);
            var contextInfo = new ContextInfo(context);
            var generatedClasses = new HashSet<string>();
            var processedSymbols = new List<INamedTypeSymbol>();

            foreach(var classSyntax in receiver.ClassSyntaxes) {
                if(context.CancellationToken.IsCancellationRequested)
                    return;
                var classSymbol = contextInfo.Compilation.GetSemanticModel(classSyntax.SyntaxTree).GetDeclaredSymbol(classSyntax);

                if(processedSymbols.Contains(classSymbol))
                    continue;
                processedSymbols.Add(classSymbol);

                if(!classSyntax.Modifiers.Any(x => x.ValueText == "partial")) {
                    context.ReportNoPartialModifier(classSymbol);
                    continue;
                }

                var classGenerator = new ClassGenerator(contextInfo, classSymbol);
                var classSource = classGenerator.GetSourceCode();
                context.AddSource(ClassHelper.CreateFileName(classSymbol.Name, generatedClasses), SourceText.From(classSource, Encoding.UTF8));
            }
        }
    }
}
