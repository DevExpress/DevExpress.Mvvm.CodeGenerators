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
            //Debugger.Launch();
            //var traceInfo = new TraceInfo();
            //StartExecute(ref traceInfo);
            if(context.SyntaxContextReceiver is not SyntaxContextReceiver receiver)
                return;

            Compilation compilation = context.Compilation;
            foreach(SupportedMvvm mvvm in ContextInfo.GetAvailableMvvm(compilation))
                AddAttributeFile(context, mvvm, ref compilation);

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
                bool hasDxAttribute = AttributeHelper.HasAttribute(classSymbol, contextInfo.Dx?.ViewModelAttributeSymbol);
                bool hasPrismAttribute = AttributeHelper.HasAttribute(classSymbol, contextInfo.Prism?.ViewModelAttributeSymbol);
                bool hasMvvmLightAttribute = AttributeHelper.HasAttribute(classSymbol, contextInfo.MvvmLight?.ViewModelAttributeSymbol);
                bool hasMvvmToolkitAttribute = AttributeHelper.HasAttribute(classSymbol, contextInfo.MvvmToolkit?.ViewModelAttributeSymbol);

                int mvvmCount = 0;
                if(hasDxAttribute) mvvmCount++;
                if(hasPrismAttribute) mvvmCount++;
                if(hasMvvmLightAttribute) mvvmCount++;

                if(mvvmCount > 1) {
                    context.ReportMoreThanOneGenerateViewModelAttributes(classSymbol);
                    continue;
                }

                SupportedMvvm mvvm;
                if(hasDxAttribute) {
                    if(contextInfo.AvailableMvvm.Contains(SupportedMvvm.Dx))
                        mvvm = SupportedMvvm.Dx;
                    else
                        mvvm = SupportedMvvm.None;
                } else if(hasPrismAttribute)
                    mvvm = SupportedMvvm.Prism;
                else if(hasMvvmLightAttribute)
                    mvvm = SupportedMvvm.MvvmLight;
                else if(hasMvvmToolkitAttribute)
                    mvvm = SupportedMvvm.MvvmToolkit;
                else continue;

                if(processedSymbols.Contains(classSymbol))
                    continue;
                processedSymbols.Add(classSymbol);

                if(!classSyntax.Modifiers.Any(x => x.ValueText == "partial")) {
                    context.ReportNoPartialModifier(classSymbol);
                    continue;
                }

                ClassGenerator.GenerateSourceCode(sourceBuilder, contextInfo, classSymbol, mvvm);
                string classSource = source.ToString();
                source.Clear();
                context.AddSource(ClassHelper.CreateFileName(classSymbol.Name, generatedClasses), SourceText.From(classSource, Encoding.UTF8));
                generatedCount++;
            }
            //EndExecute(traceInfo, generatedCount);
        }
        static void AddAttributeFile(GeneratorExecutionContext context, SupportedMvvm mvvm, ref Compilation compilation) {
            SourceText attributesSourceText = SourceText.From(InitializationGenerator.GetSourceCode(mvvm, ContextInfo.GetIsWinUI(context.Compilation)), Encoding.UTF8);
            SyntaxTree attributesSyntaxTree = CSharpSyntaxTree.ParseText(attributesSourceText, (CSharpParseOptions)context.ParseOptions);
            compilation = compilation.AddSyntaxTrees(attributesSyntaxTree);
            context.AddSource(ClassHelper.CreateFileName(mvvm.ToString() + "Attributes"), attributesSourceText);
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
