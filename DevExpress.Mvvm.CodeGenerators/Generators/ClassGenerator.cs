using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Mvvm.CodeGenerators {
    enum ChangeEventRaiseMode { EventArgs, PropertyName }
    static class ClassGenerator {
        const string defaultUsings =
@"using System.Collections.Generic;
using System.ComponentModel;";

        public static void GenerateSourceCode(SourceBuilder source, ContextInfo contextInfo, INamedTypeSymbol classSymbol) {
            List<IInterfaceGenerator> interfaces = new();

            INPCInfo inpcedInfo = INPCInfo.GetINPCedInfo(contextInfo, classSymbol);
            if(inpcedInfo.HasNoImplementation())
                interfaces.Add(new INPCedInterfaceGenerator());
            bool impelementRaiseChangedMethod = inpcedInfo.ShouldImplementRaiseMethod();

            INPCInfo inpcingInfo = INPCInfo.GetINPCingInfo(contextInfo, classSymbol);
            if(inpcingInfo.HasNoImplementation())
                interfaces.Add(new INPCingInterfaceGenerator());
            bool impelementRaiseChangingMethod = inpcingInfo.ShouldImplementRaiseMethod();

            List<string> mvvmComponentsList = new();

            bool implIDEI = ClassHelper.GetImplementIDEIValue(contextInfo, classSymbol);
            bool implISS = ClassHelper.GetImplementISSValue(contextInfo, classSymbol);
            bool implISPVM = ClassHelper.GetImplementISPVMValue(contextInfo, classSymbol);
            if(implIDEI) {
                mvvmComponentsList.Add("IDataErrorInfo");
                if(contextInfo.IsMvvmAvailable && !ClassHelper.IsInterfaceImplementedInCurrentClass(classSymbol, contextInfo.IDEISymbol))
                    interfaces.Add(new IDataErrorInfoGenerator());
            }
            if(implISPVM) {
                mvvmComponentsList.Add("ISupportParentViewModel");
                if(contextInfo.IsMvvmAvailable && !ClassHelper.IsInterfaceImplemented(classSymbol, contextInfo.ISPVMSymbol!, contextInfo)) {
                    bool shouldGenerateChangedMethod = ClassHelper.ContainISPVMChangedMethod(classSymbol);
                    interfaces.Add(new ISupportParentViewModelGenerator(shouldGenerateChangedMethod));
                }
            }
            if(implISS) {
                mvvmComponentsList.Add("ISupportServices");
                if(contextInfo.IsMvvmAvailable && !ClassHelper.IsInterfaceImplementedInCurrentClass(classSymbol, contextInfo.ISSSymbol!))
                    interfaces.Add(new ISupportServicesGenerator(classSymbol.IsSealed));
            }

            List<ITypeSymbol> genericTypes = new();
            if(classSymbol.IsGenericType) {
                genericTypes = classSymbol.TypeArguments.ToList();
            }

            Dictionary<string, TypeKind> outerClasses = ClassHelper.GetOuterClasses(classSymbol);

            source = GenerateHeader(source, classSymbol, interfaces,
                impelementRaiseChangedMethod ? inpcedInfo.RaiseMethodImplementation : null,
                impelementRaiseChangingMethod ? inpcingInfo.RaiseMethodImplementation : null,
                genericTypes, outerClasses, contextInfo.IsMvvmAvailable);


            bool needStaticChangedEventArgs = inpcedInfo.HasRaiseMethodWithEventArgsParameter || impelementRaiseChangedMethod;
            bool needStaticChangingEventArgs = inpcingInfo.HasAttribute && (inpcingInfo.HasRaiseMethodWithEventArgsParameter || impelementRaiseChangingMethod);
            IReadOnlyList<string> propertyNames = GenerateProperties(source, contextInfo, classSymbol, inpcedInfo, inpcingInfo, needStaticChangedEventArgs, needStaticChangingEventArgs);

            GenerateCommands(source, contextInfo, classSymbol, contextInfo.IsMvvmAvailable, out bool hasCommands);
            if(hasCommands)
                mvvmComponentsList.Add("Commands");

            EventArgsGenerator.Generate(source, needStaticChangedEventArgs, needStaticChangingEventArgs, propertyNames);

            while(source.Return != null)
                source = source.Return.AppendLine("}");

            if(mvvmComponentsList.Any())
                if(!contextInfo.IsMvvmAvailable)
                    contextInfo.Context.ReportMVVMNotAvailable(classSymbol, mvvmComponentsList);
        }

        static SourceBuilder GenerateHeader(SourceBuilder source, INamedTypeSymbol classSymbol, List<IInterfaceGenerator> interfaces, string? raiseChangedMethod, string? raiseChangingMethod, List<ITypeSymbol> genericTypes, Dictionary<string, TypeKind> outerClasses, bool addDevExpressUsing) {
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

        static void AppendGenericArguments(SourceBuilder source, List<ITypeSymbol> genericTypes) {
            if(genericTypes.Any()) {
                source.Append('<');
                source.AppendMultipleLinesWithSeparator(genericTypes.Select(type => type.ToString()), ", ");
                source.Append('>');
            }
        }
        static IReadOnlyList<string> GenerateProperties(SourceBuilder source, ContextInfo contextInfo, INamedTypeSymbol classSymbol, INPCInfo inpcedInfo, INPCInfo inpcingInfo, bool needStaticChangedEventArgs, bool needStaticChangingEventArgs) {
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
            IEnumerable<IFieldSymbol> fieldCandidates = ClassHelper.GetFieldCandidates(classSymbol, contextInfo.PropertyAttributeSymbol);
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
                        string? propertyName = PropertyGenerator.Generate(source, contextInfo, classSymbol, fieldSymbol, changedRaiseMode, changingRaiseMode);
                        if(propertyName != null) {
                            propertyNames.Add(propertyName);
                        }
                    }
            }
            return propertyNames;
        }

        static void GenerateCommands(SourceBuilder source, ContextInfo contextInfo, INamedTypeSymbol classSymbol, bool isMvvmAvailable, out bool hasCommands) {
            IEnumerable<IMethodSymbol> commandCandidates = ClassHelper.GetCommandCandidates(classSymbol, contextInfo.CommandAttributeSymbol);
            hasCommands = commandCandidates.Any();
            if(isMvvmAvailable) {
                foreach(IMethodSymbol methodSymbol in commandCandidates) {
                    CommandGenerator.Generate(source, contextInfo, classSymbol, methodSymbol);
                }
            }
        }
    }
}
