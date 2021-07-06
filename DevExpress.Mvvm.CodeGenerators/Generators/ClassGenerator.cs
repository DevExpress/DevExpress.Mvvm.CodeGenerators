using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    static class ClassGenerator {
        static readonly string defaultUsings =
@"using System.Collections.Generic;
using System.ComponentModel;";


        public static string GetSourceCode(ContextInfo contextInfo, INamedTypeSymbol classSymbol) {
            List<IInterfaceGenerator> interfaces = new();

            var inpcedInfo = INPCInfo.GetINPCedInfo(contextInfo, classSymbol);
            if(inpcedInfo.HasNoImplementation())
                interfaces.Add(new INPCedInterfaceGenerator());
            var impelementRaiseChangedMethod = inpcedInfo.ShouldImplementRaiseMethod();

            var inpcingInfo = INPCInfo.GetINPCingInfo(contextInfo, classSymbol);
            if(inpcingInfo.HasNoImplementation())
                interfaces.Add(new INPCingInterfaceGenerator());
            var impelementRaiseChangingMethod = inpcingInfo.ShouldImplementRaiseMethod();

            var isMvvmAvailable = ClassHelper.IsMvvmAvailable(contextInfo.Compilation);
            var mvvmComponentsList = new List<string>();

            var implIDEI = ClassHelper.GetImplementIDEIValue(contextInfo, classSymbol);
            var implISS = ClassHelper.GetImplementISSValue(contextInfo, classSymbol);
            var implISPVM = ClassHelper.GetImplementISPVMValue(contextInfo, classSymbol);
            if(implIDEI) {
                mvvmComponentsList.Add("IDataErrorInfo");
                if(isMvvmAvailable && !ClassHelper.IsInterfaceImplemented(classSymbol, contextInfo.IDEISymbol))
                    interfaces.Add(new IDataErrorInfoGenerator());
            }
            if(implISPVM) {
                mvvmComponentsList.Add("ISupportParentViewModel");
                var shouldGenerateChangedMethod = ClassHelper.ShouldGenerateISPVMChangedMethod(classSymbol);
                if(isMvvmAvailable && contextInfo.ISPVMSymbol != null && !ClassHelper.IsInterfaceImplemented(classSymbol, contextInfo.ISPVMSymbol))
                    interfaces.Add(new ISupportParentViewModelGenerator(shouldGenerateChangedMethod));
            }
            if(implISS) {
                mvvmComponentsList.Add("ISupportServices");
                if(isMvvmAvailable && contextInfo.ISSSymbol != null && !ClassHelper.IsInterfaceImplemented(classSymbol, contextInfo.ISSSymbol))
                    interfaces.Add(new ISupportServicesGenerator());
            }
            var commandCandidates = ClassHelper.GetCommandCandidates(classSymbol, contextInfo.CommandAttributeSymbol);
            if(commandCandidates.Any())
                mvvmComponentsList.Add("Commands");

            List<ITypeSymbol> genericTypes = new();
            if(classSymbol.IsGenericType) {
                genericTypes = classSymbol.TypeArguments.ToList();
            }

            var outerClasses = ClassHelper.GetOuterClasses(classSymbol);

            var source = new StringBuilder();
            int tabs = 0;
            GenerateHeader(classSymbol, interfaces, 
                impelementRaiseChangedMethod ? inpcedInfo.RaiseMethodImplementation : null, 
                impelementRaiseChangingMethod ? inpcingInfo.RaiseMethodImplementation : null, 
                genericTypes, outerClasses, source, ref tabs, 
                addDevExpressUsing: mvvmComponentsList.Any() && isMvvmAvailable);

            var needStaticChangedEventArgs = inpcedInfo.HasRaiseMethodWithEventArgsParameter || impelementRaiseChangedMethod;
            var needStaticChangingEventArgs = inpcingInfo.HasAttribute && (inpcingInfo.HasRaiseMethodWithEventArgsParameter || impelementRaiseChangingMethod);
            var propertyNames = GenerateProperties(contextInfo, classSymbol, inpcedInfo, inpcingInfo, needStaticChangedEventArgs, needStaticChangingEventArgs, source, tabs);

            GenerateCommands(contextInfo, classSymbol, commandCandidates, isMvvmAvailable, source, tabs);

            EventArgsGenerator.Generate(source, tabs, needStaticChangedEventArgs, needStaticChangingEventArgs, propertyNames);

            while(tabs-- > 0)
                source.AppendLine("}".AddTabs(tabs));

            if(mvvmComponentsList.Any())
                if(!isMvvmAvailable)
                    contextInfo.Context.ReportMVVMNotAvailable(classSymbol, mvvmComponentsList);

            return source.ToString();
        }

        static void GenerateHeader(INamedTypeSymbol classSymbol, List<IInterfaceGenerator> interfaces, string raiseChangedMethod, string raiseChangingMethod, List<ITypeSymbol> genericTypes, Dictionary<string, TypeKind> outerClasses, StringBuilder source, ref int tabs, bool addDevExpressUsing) {
            source.AppendLine(defaultUsings);
            if(addDevExpressUsing)
                source.AppendLine("using DevExpress.Mvvm;");
            source.AppendLine();
            source.AppendLine("#nullable enable");
            source.AppendLine();

            string @namespace = classSymbol.ContainingNamespace.ToDisplayString();
            if(@namespace != "<global namespace>") {
                source.AppendLine($"namespace {@namespace} {{");
                tabs++;
            }

            foreach(var outerClass in outerClasses.Reverse()) {
                source.AppendLine($"partial {outerClass.Value.TypeToString()} {outerClass.Key}".AddTabs(tabs) + " {");
                tabs++;
            }
            source.Append($"partial class {classSymbol.Name}".AddTabs(tabs));
            if(genericTypes.Any())
                source.Append($"<{genericTypes.Select(type => type.ToString()).ConcatToString(", ")}>");
            if(interfaces.Any()) {
                source.AppendLine($" : {interfaces.Select(@interface => @interface.GetName()).ConcatToString(", ")} {{");
                foreach(var @interface in interfaces)
                    source.AppendLine(@interface.GetImplementation().AddTabs(tabs + 1));
                source.AppendLine();
            } else
                source.AppendLine(" {");
            tabs++;
            if(!string.IsNullOrEmpty(raiseChangedMethod))
                source.AppendLine(raiseChangedMethod.AddTabs(tabs));
            if(!string.IsNullOrEmpty(raiseChangingMethod))
                source.AppendLine(raiseChangingMethod.AddTabs(tabs));
            if(!string.IsNullOrEmpty(raiseChangedMethod) || !string.IsNullOrEmpty(raiseChangingMethod))
                source.AppendLine();
        }

        static IReadOnlyList<string> GenerateProperties(ContextInfo contextInfo, INamedTypeSymbol classSymbol, INPCInfo inpcedInfo, INPCInfo inpcingInfo, bool needStaticChangedEventArgs, bool needStaticChangingEventArgs, StringBuilder source, int tabs) {
            var raiseChangedMethodParameter = needStaticChangedEventArgs ? "eventargs" : inpcedInfo.HasRaiseMethodWithStringParameter ? "string" : string.Empty;
            var raiseChangingMethodParameter = needStaticChangingEventArgs ? "eventargs" : inpcingInfo.HasAttribute && inpcingInfo.HasRaiseMethodWithStringParameter ? "string" : string.Empty;
            var generateProperties = true;
            List<string> propertyNames = new();
            var fieldCandidates = ClassHelper.GetFieldCandidates(classSymbol, contextInfo.PropertyAttributeSymbol);
            if(fieldCandidates.Any()) {
                if(string.IsNullOrEmpty(raiseChangedMethodParameter)) {
                    contextInfo.Context.ReportRaiseMethodNotFound(classSymbol, "ed");
                    generateProperties = false;
                }
                if(inpcingInfo.HasAttribute && string.IsNullOrEmpty(raiseChangingMethodParameter)) {
                    contextInfo.Context.ReportRaiseMethodNotFound(classSymbol, "ing");
                    generateProperties = false;
                }
                if(generateProperties)
                    foreach(var fieldSymbol in fieldCandidates) {
                        var propertyName = PropertyGenerator.Generate(source, tabs, contextInfo, classSymbol, fieldSymbol, raiseChangedMethodParameter, raiseChangingMethodParameter);
                        if(propertyName != null) {
                            propertyNames.Add(propertyName);
                        }
                    }
            }
            return propertyNames;
        }

        static void GenerateCommands(ContextInfo contextInfo, INamedTypeSymbol classSymbol, IEnumerable<IMethodSymbol> commandCandidates, bool isMvvmAvailable, StringBuilder source, int tabs) {
            if(isMvvmAvailable) {
                foreach(var methodSymbol in commandCandidates) {
                    CommandGenerator.Generate(source, tabs, contextInfo, classSymbol, methodSymbol);
                }
            }
        }
    }
}
