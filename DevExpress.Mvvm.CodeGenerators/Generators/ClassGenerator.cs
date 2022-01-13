using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DevExpress.Mvvm.CodeGenerators {
    enum ChangeEventRaiseMode { EventArgs, PropertyName }
    static class ClassGenerator {
        const string defaultUsings =
@"using System.Collections.Generic;
using System.ComponentModel;";

        public static void GenerateSourceCode(SourceBuilder source, ContextInfo contextInfo, INamedTypeSymbol classSymbol, SupportedMvvm mvvm) {
            List<IInterfaceGenerator> interfaces = new();

            INPCInfo inpcedInfo = INPCInfo.GetINPCedInfo(contextInfo, classSymbol, mvvm);
            if(inpcedInfo.HasNoImplementation())
                interfaces.Add(new INPCedInterfaceGenerator());
            bool impelementRaiseChangedMethod = inpcedInfo.ShouldImplementRaiseMethod();

            INPCInfo inpcingInfo = INPCInfo.GetINPCingInfo(contextInfo, classSymbol, mvvm);
            if(inpcingInfo.HasNoImplementation())
                interfaces.Add(new INPCingInterfaceGenerator());
            bool impelementRaiseChangingMethod = inpcingInfo.ShouldImplementRaiseMethod();

            AddAvailableInterfaces(interfaces, contextInfo, classSymbol, mvvm);

            List<ITypeSymbol> genericTypes = new();
            if(classSymbol.IsGenericType) {
                genericTypes = classSymbol.TypeArguments.ToList();
            }

            Dictionary<string, TypeKind> outerClasses = ClassHelper.GetOuterClasses(classSymbol);

            source = GenerateHeader(source, classSymbol, interfaces,
                impelementRaiseChangedMethod ? inpcedInfo.RaiseMethodImplementation : null,
                impelementRaiseChangingMethod ? inpcingInfo.RaiseMethodImplementation : null,
                genericTypes, outerClasses, mvvm, contextInfo.Compilation);


            bool needStaticChangedEventArgs = inpcedInfo.HasRaiseMethodWithEventArgsParameter || impelementRaiseChangedMethod;
            bool needStaticChangingEventArgs = inpcingInfo.HasAttribute && (inpcingInfo.HasRaiseMethodWithEventArgsParameter || impelementRaiseChangingMethod);
            IReadOnlyList<string> propertyNames = GenerateProperties(source, contextInfo, classSymbol, inpcedInfo, inpcingInfo, needStaticChangedEventArgs, needStaticChangingEventArgs, mvvm);

            GenerateCommands(source, contextInfo, classSymbol, mvvm);

            EventArgsGenerator.Generate(source, needStaticChangedEventArgs, needStaticChangingEventArgs, propertyNames);

            while(source.Return != null)
                source = source.Return.AppendLine("}");
            static SourceBuilder GenerateHeader(SourceBuilder source, INamedTypeSymbol classSymbol, List<IInterfaceGenerator> interfaces, string? raiseChangedMethod, string? raiseChangingMethod, List<ITypeSymbol> genericTypes, Dictionary<string, TypeKind> outerClasses, SupportedMvvm actualMvvm, Compilation compilation) {
                source.AppendLine(defaultUsings);
                switch(actualMvvm) {
                    case SupportedMvvm.Dx:
                        source.AppendLine("using DevExpress.Mvvm;");
                        break;
                    case SupportedMvvm.Prism:
                        source.AppendLine("using System;").AppendLine("using Prism;").AppendLine("using Prism.Commands;");
                        break;
                    case SupportedMvvm.MvvmLight:
                        source.AppendLine("using GalaSoft.MvvmLight;");
                        if(ContextInfo.GetIsMvvmLightCommandWpfAvalible(compilation))
                            source.AppendLine("using GalaSoft.MvvmLight.CommandWpf;");
                        else
                            source.AppendLine("using GalaSoft.MvvmLight.Command;");
                        source.AppendLine("using GalaSoft.MvvmLight.Messaging;");
                        break;
                    case SupportedMvvm.None:
                        break;
                    default:
                        throw new InvalidEnumArgumentException();
                }
                source.AppendLine();
                source.AppendLine("#nullable enable");
                source.AppendLine();

                string @namespace = classSymbol.ContainingNamespace.ToDisplayString();
                if(@namespace != "<global namespace>") {
                    source.Append("namespace ").Append(@namespace).AppendLine(" {");
                    source = source.Tab;
                }

                foreach(KeyValuePair<string, TypeKind> outerClass in outerClasses.Reverse()) {
                    source.Append("partial ").Append(outerClass.Value.TypeToString()).Append(' ').Append(outerClass.Key).AppendLine(" {");
                    source = source.Tab;
                }
                source.Append("partial class ").Append(classSymbol.Name);
                AppendGenericArguments(source, genericTypes);
                if(interfaces.Any()) {
                    source.Append(" : ");
                    source.AppendMultipleLinesWithSeparator(interfaces.Select(@interface => @interface.GetName()), ", ");
                    source.AppendLine(" {");
                    foreach(IInterfaceGenerator @interface in interfaces)
                        @interface.AppendImplementation(source.Tab);
                    source.AppendLine();
                } else
                    source.AppendLine(" {");
                source = source.Tab;
                const string protectedModifier = "protected ";
                bool isSealed = classSymbol.IsSealed;
                if(!string.IsNullOrEmpty(raiseChangedMethod))
                    source.AppendIf(!isSealed, protectedModifier)
                          .AppendMultipleLines(raiseChangedMethod!);
                if(!string.IsNullOrEmpty(raiseChangingMethod))
                    source.AppendIf(!isSealed, protectedModifier)
                          .AppendMultipleLines(raiseChangingMethod!);
                if(!string.IsNullOrEmpty(raiseChangedMethod) || !string.IsNullOrEmpty(raiseChangingMethod))
                    source.AppendLine();
                return source;
            }
        }

            static void AppendGenericArguments(SourceBuilder source, List<ITypeSymbol> genericTypes) {
                if(genericTypes.Any()) {
                    source.Append('<');
                    source.AppendMultipleLinesWithSeparator(genericTypes.Select(type => type.ToString()), ", ");
                    source.Append('>');
                }
            }
            static IReadOnlyList<string> GenerateProperties(SourceBuilder source, ContextInfo contextInfo, INamedTypeSymbol classSymbol, INPCInfo inpcedInfo, INPCInfo inpcingInfo, bool needStaticChangedEventArgs, bool needStaticChangingEventArgs, SupportedMvvm mvvm) {
                ChangeEventRaiseMode? changedRaiseMode = needStaticChangedEventArgs
                    ? ChangeEventRaiseMode.EventArgs
                    : inpcedInfo.HasRaiseMethodWithStringParameter
                        ? ChangeEventRaiseMode.PropertyName
                        : default(ChangeEventRaiseMode?);
                ChangeEventRaiseMode? changingRaiseMode = needStaticChangingEventArgs
                    ? ChangeEventRaiseMode.EventArgs
                    : inpcingInfo.HasAttribute && inpcingInfo.HasRaiseMethodWithStringParameter
                        ? ChangeEventRaiseMode.PropertyName
                        : default(ChangeEventRaiseMode?);
                bool generateProperties = true;
                List<string> propertyNames = new();
                IEnumerable<IFieldSymbol> fieldCandidates = ClassHelper.GetFieldCandidates(classSymbol, contextInfo.GetFrameworkAttributes(mvvm).PropertyAttributeSymbol);
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
                        foreach(IFieldSymbol fieldSymbol in fieldCandidates) {
                            string? propertyName = PropertyGenerator.Generate(source, contextInfo, classSymbol, fieldSymbol, changedRaiseMode, changingRaiseMode, mvvm);
                            if(propertyName != null) {
                                propertyNames.Add(propertyName);
                            }
                        }
                }
                return propertyNames;
            }

            static void GenerateCommands(SourceBuilder source, ContextInfo contextInfo, INamedTypeSymbol classSymbol, SupportedMvvm mvvm) {
                IEnumerable<IMethodSymbol> commandCandidates = ClassHelper.GetCommandCandidates(classSymbol, contextInfo.GetFrameworkAttributes(mvvm).CommandAttributeSymbol);
                foreach(IMethodSymbol methodSymbol in commandCandidates) {
                    CommandGenerator.Generate(source, contextInfo, classSymbol, methodSymbol, mvvm);
                }
            }

            static void AddAvailableInterfaces(List<IInterfaceGenerator> interfaces, ContextInfo contextInfo, INamedTypeSymbol classSymbol, SupportedMvvm mvvm) {
            switch(mvvm) {
                case SupportedMvvm.Dx:
                    if(ClassHelper.GetImplementIDEIValue(contextInfo, classSymbol) && !ClassHelper.IsInterfaceImplementedInCurrentClass(classSymbol, contextInfo.Dx!.IDEISymbol))
                        interfaces.Add(new IDataErrorInfoGenerator());
                    if(ClassHelper.GetImplementISPVMValue(contextInfo, classSymbol, mvvm) && !ClassHelper.IsInterfaceImplemented(classSymbol, contextInfo.Dx!.ISPVMSymbol, contextInfo, mvvm))
                        interfaces.Add(new ISupportParentViewModelGenerator(ClassHelper.ContainsOnChangedMethod(classSymbol, "OnParentViewModelChanged", 1, "object")));
                    if(ClassHelper.GetImplementISSValue(contextInfo, classSymbol) && !ClassHelper.IsInterfaceImplementedInCurrentClass(classSymbol, contextInfo.Dx!.ISSSymbol))
                        interfaces.Add(new ISupportServicesGenerator(classSymbol.IsSealed));
                    break;
                case SupportedMvvm.Prism:
                    if(ClassHelper.GetImplementIAAValue(contextInfo, classSymbol) && !ClassHelper.IsInterfaceImplemented(classSymbol, contextInfo.Prism!.IAASymbol, contextInfo, mvvm))
                        interfaces.Add(new IActiveAwareGenerator(ClassHelper.ContainsOnChangedMethod(classSymbol, "OnIsActiveChanged", 0, null)));
                    break;
                case SupportedMvvm.MvvmLight:
                    if(ClassHelper.GetImplementICUValue(contextInfo, classSymbol) && !ClassHelper.IsInterfaceImplemented(classSymbol, contextInfo.MvvmLight!.ICUSymbol, contextInfo, mvvm))
                        interfaces.Add(new ICleanupGenerator(ClassHelper.ContainsOnChangedMethod(classSymbol, "OnCleanup", 0, null), classSymbol.IsSealed));
                    break;
                    case SupportedMvvm.None:
                        break;
                    default:
                        throw new InvalidEnumArgumentException();
            }
        }
    }
}
