using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    static class ViewModelGeneratorCore {
        ref struct TraceInfo {
            public long WorkingSet;
            public Stopwatch StopWatch;
        }
        public static void Execute(GeneratorExecutionContext context) {
            //var traceInfo = new TraceInfo();
            //StartExecute(ref traceInfo);
            if(context.SyntaxContextReceiver is not SyntaxContextReceiver receiver)
                return;

            SourceText attributesSourceText = SourceText.From(InitializationGenerator.GetSourceCode(ContextInfo.GetIsWinUI(context.Compilation)), Encoding.UTF8);
            Compilation compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(attributesSourceText));
            context.AddSource(ClassHelper.CreateFileName("Attributes"), attributesSourceText);

            ContextInfo contextInfo = new ContextInfo(context, compilation);
            HashSet<string> generatedClasses = new HashSet<string>();
            List<INamedTypeSymbol> processedSymbols = new List<INamedTypeSymbol>();

            StringBuilder source = new StringBuilder();
            SourceBuilder sourceBuilder = new SourceBuilder(source);
            int generatedCount = 0;
            foreach(ClassDeclarationSyntax classSyntax in receiver.ClassSyntaxes) {
                if(context.CancellationToken.IsCancellationRequested)
                    break;
                if(classSyntax.AttributeLists.Count == 0)
                    continue;
                INamedTypeSymbol classSymbol = contextInfo.Compilation.GetSemanticModel(classSyntax.SyntaxTree).GetDeclaredSymbol(classSyntax)!;
                if(!AttributeHelper.HasAttribute(classSymbol, contextInfo.ViewModelAttributeSymbol))
                    continue;

                if(processedSymbols.Contains(classSymbol))
                    continue;
                processedSymbols.Add(classSymbol);

                if(!classSyntax.Modifiers.Any(x => x.ValueText == "partial")) {
                    context.ReportNoPartialModifier(classSymbol);
                    continue;
                }

                ClassGenerator.GenerateSourceCode(sourceBuilder, contextInfo, classSymbol);
                string classSource = source.ToString();
                source.Clear();
                context.AddSource(ClassHelper.CreateFileName(classSymbol.Name, generatedClasses), SourceText.From(classSource, Encoding.UTF8));
                generatedCount++;
            }
            //EndExecute(traceInfo, generatedCount);
        }

        [Conditional("DEBUG")]
        static void StartExecute(ref TraceInfo info) {
            info.StopWatch = new Stopwatch();
            info.StopWatch.Start();
            info.WorkingSet = Process.GetCurrentProcess().WorkingSet64;
        }
        [Conditional("DEBUG")]
        static void EndExecute(TraceInfo info, int generatedCount) {
            long workingSetDifference = Process.GetCurrentProcess().WorkingSet64 - info.WorkingSet;
            Debug.WriteLine($"MVVM Generator: {generatedCount} classes generated in {info.StopWatch.ElapsedMilliseconds} ms. Working set increased: {workingSetDifference}");
        }
    }
}
