using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    enum ChangeEventRaiseMode { 
        EventArgs, PropertyName
    }
    static class ClassGenerator {
        static readonly string defaultUsings =
@"using System.Collections.Generic;
using System.ComponentModel;";

        public static void GenerateSourceCode(SourceBuilder source, ContextInfo contextInfo, INamedTypeSymbol classSymbol) {
            List<IInterfaceGenerator> interfaces = new();

            var inpcedInfo = INPCInfo.GetINPCedInfo(contextInfo, classSymbol);
            if(inpcedInfo.HasNoImplementation())
                interfaces.Add(new INPCedInterfaceGenerator());
            var impelementRaiseChangedMethod = inpcedInfo.ShouldImplementRaiseMethod();

            var inpcingInfo = INPCInfo.GetINPCingInfo(contextInfo, classSymbol);
            if(inpcingInfo.HasNoImplementation())
                interfaces.Add(new INPCingInterfaceGenerator());
            var impelementRaiseChangingMethod = inpcingInfo.ShouldImplementRaiseMethod();

            var mvvmComponentsList = new List<string>();

            var implIDEI = ClassHelper.GetImplementIDEIValue(contextInfo, classSymbol);
            var implISS = ClassHelper.GetImplementISSValue(contextInfo, classSymbol);
            var implISPVM = ClassHelper.GetImplementISPVMValue(contextInfo, classSymbol);
            if(implIDEI) {
                mvvmComponentsList.Add("IDataErrorInfo");
                if(contextInfo.IsMvvmAvailable && !ClassHelper.IsInterfaceImplementedInCurrentClass(classSymbol, contextInfo.IDEISymbol))
                    interfaces.Add(new IDataErrorInfoGenerator());
            }
            if(implISPVM) {
                mvvmComponentsList.Add("ISupportParentViewModel");
                var shouldGenerateChangedMethod = ClassHelper.ShouldGenerateISPVMChangedMethod(classSymbol);
                if(contextInfo.IsMvvmAvailable && contextInfo.ISPVMSymbol != null && !ClassHelper.IsInterfaceImplemented(classSymbol, contextInfo.ISPVMSymbol, contextInfo))
                    interfaces.Add(new ISupportParentViewModelGenerator(shouldGenerateChangedMethod));
            }
            if(implISS) {
                mvvmComponentsList.Add("ISupportServices");
                if(contextInfo.IsMvvmAvailable && contextInfo.ISSSymbol != null && !ClassHelper.IsInterfaceImplementedInCurrentClass(classSymbol, contextInfo.ISSSymbol))
                    interfaces.Add(new ISupportServicesGenerator());
            }

            List<ITypeSymbol> genericTypes = new();
            if(classSymbol.IsGenericType) {
                genericTypes = classSymbol.TypeArguments.ToList();
            }

            var outerClasses = ClassHelper.GetOuterClasses(classSymbol);

            source = GenerateHeader(classSymbol, interfaces, 
                impelementRaiseChangedMethod ? inpcedInfo.RaiseMethodImplementation : null, 
                impelementRaiseChangingMethod ? inpcingInfo.RaiseMethodImplementation : null, 
                genericTypes, outerClasses, source, 
                addDevExpressUsing: contextInfo.IsMvvmAvailable);

            
            var needStaticChangedEventArgs = inpcedInfo.HasRaiseMethodWithEventArgsParameter || impelementRaiseChangedMethod;
            var needStaticChangingEventArgs = inpcingInfo.HasAttribute && (inpcingInfo.HasRaiseMethodWithEventArgsParameter || impelementRaiseChangingMethod);
            var propertyNames = GenerateProperties(contextInfo, classSymbol, inpcedInfo, inpcingInfo, needStaticChangedEventArgs, needStaticChangingEventArgs, source);

            GenerateCommands(contextInfo, classSymbol, contextInfo.CommandAttributeSymbol, contextInfo.IsMvvmAvailable, source, out bool hasCommands);
            if(hasCommands)
                mvvmComponentsList.Add("Commands");

            EventArgsGenerator.Generate(source, needStaticChangedEventArgs, needStaticChangingEventArgs, propertyNames);

            while(source.Return != null)
                source = source.Return.AppendLine("}");

            if(mvvmComponentsList.Any())
                if(!contextInfo.IsMvvmAvailable)
                    contextInfo.Context.ReportMVVMNotAvailable(classSymbol, mvvmComponentsList);
        }

        static SourceBuilder GenerateHeader(INamedTypeSymbol classSymbol, List<IInterfaceGenerator> interfaces, string raiseChangedMethod, string raiseChangingMethod, List<ITypeSymbol> genericTypes, Dictionary<string, TypeKind> outerClasses, SourceBuilder source, bool addDevExpressUsing) {
            source.AppendLine(defaultUsings);
            if(addDevExpressUsing)
                source.AppendLine("using DevExpress.Mvvm;");
            source.AppendLine();
            source.AppendLine("#nullable enable");
            source.AppendLine();

            string @namespace = classSymbol.ContainingNamespace.ToDisplayString();
            if(@namespace != "<global namespace>") {
                source.Append("namespace ").Append(@namespace).AppendLine(" {");
                source = source.Tab;
            }

            foreach(var outerClass in outerClasses.Reverse()) {
                source.Append("partial ").Append(outerClass.Value.TypeToString()).Append(' ').Append(outerClass.Key).AppendLine(" {");
                source = source.Tab;
            }
            source.Append("partial class ").Append(classSymbol.Name);
            AppendGenericArguments(source, genericTypes);
            if(interfaces.Any()) {
                source.Append(" : ");
                source.AppendMultipleLinesWithSeparator(interfaces.Select(@interface => @interface.GetName()), ", ");
                source.AppendLine(" {");
                foreach(var @interface in interfaces)
                    @interface.AppendImplementation(source.Tab);
                source.AppendLine();
            } else
                source.AppendLine(" {");
            source = source.Tab;
            if(!string.IsNullOrEmpty(raiseChangedMethod))
                source.AppendMultipleLines(raiseChangedMethod);
            if(!string.IsNullOrEmpty(raiseChangingMethod))
                source.AppendMultipleLines(raiseChangingMethod);
            if(!string.IsNullOrEmpty(raiseChangedMethod) || !string.IsNullOrEmpty(raiseChangingMethod))
                source.AppendLine();
            return source;
        }

        static void AppendGenericArguments(SourceBuilder source, List<ITypeSymbol> genericTypes) {
            if(genericTypes.Any()) {
                source.Append('<');
                source.AppendMultipleLinesWithSeparator(genericTypes.Select(type => type.ToString()), ", ");
                source.Append('>');
            }
        }
        static IReadOnlyList<string> GenerateProperties(ContextInfo contextInfo, INamedTypeSymbol classSymbol, INPCInfo inpcedInfo, INPCInfo inpcingInfo, bool needStaticChangedEventArgs, bool needStaticChangingEventArgs, SourceBuilder source) {
            var changedRaiseMode = needStaticChangedEventArgs 
                ? ChangeEventRaiseMode.EventArgs 
                : inpcedInfo.HasRaiseMethodWithStringParameter 
                    ? ChangeEventRaiseMode.PropertyName 
                    : default(ChangeEventRaiseMode?);
            var changingRaiseMode = needStaticChangingEventArgs 
                ? ChangeEventRaiseMode.EventArgs 
                : inpcingInfo.HasAttribute && inpcingInfo.HasRaiseMethodWithStringParameter 
                    ? ChangeEventRaiseMode.PropertyName 
                    : default(ChangeEventRaiseMode?);
            var generateProperties = true;
            List<string> propertyNames = new();
            var fieldCandidates = ClassHelper.GetFieldCandidates(classSymbol, contextInfo.PropertyAttributeSymbol);
            if(fieldCandidates.Any()) {
                if(changedRaiseMode == null) {
                    contextInfo.Context.ReportRaiseMethodNotFound(classSymbol, "ed");
                    generateProperties = false;
                }
                if(inpcingInfo.HasAttribute && changingRaiseMode == null) {
                    contextInfo.Context.ReportRaiseMethodNotFound(classSymbol, "ing");
                    generateProperties = false;
                }
                if(generateProperties)
                    foreach(var fieldSymbol in fieldCandidates) {
                        var propertyName = PropertyGenerator.Generate(source, contextInfo, classSymbol, fieldSymbol, changedRaiseMode, changingRaiseMode);
                        if(propertyName != null) {
                            propertyNames.Add(propertyName);
                        }
                    }
            }
            return propertyNames;
        }

        static void GenerateCommands(ContextInfo contextInfo, INamedTypeSymbol classSymbol, INamedTypeSymbol commandAttributeSymbol, bool isMvvmAvailable, SourceBuilder source, out bool hasCommands) {
            var commandCandidates = ClassHelper.GetCommandCandidates(classSymbol, contextInfo.CommandAttributeSymbol);
            hasCommands = commandCandidates.Any();
            if(isMvvmAvailable) {
                foreach(var methodSymbol in commandCandidates) {
                    CommandGenerator.Generate(source, contextInfo, classSymbol, methodSymbol);
                }
            }
        }
    }
}
