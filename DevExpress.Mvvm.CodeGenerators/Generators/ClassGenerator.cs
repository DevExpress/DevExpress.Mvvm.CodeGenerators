using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    class ClassGenerator {
        static readonly string defaultUsings =
@"using System.Collections.Generic;
using System.ComponentModel;";

        readonly string usings;
        readonly string @namespace;
        readonly string name;
        readonly string raiseChangedMethod;
        readonly string raiseChangingMethod;
        readonly EventArgsGenerator eventArgsGenerator;
        readonly List<IInterfaceGenerator> interfaces = new();
        readonly List<PropertyGenerator> properties = new();
        readonly List<CommandGenerator> commands = new();
        readonly List<ITypeSymbol> genericTypes = new();

        public ClassGenerator(ContextInfo contextInfo, INamedTypeSymbol classSymbol) {
            usings = defaultUsings;
            @namespace = classSymbol.ContainingNamespace.ToDisplayString();
            name = classSymbol.Name;

            var inpcedInfo = INPCInfo.GetINPCedInfo(contextInfo, classSymbol);
            if(inpcedInfo.HasNoImplementation())
                interfaces.Add(new INPCedInterfaceGenerator());
            var impelementRaiseChangedMethod = inpcedInfo.ShouldImplementRaiseMethod();
            if(impelementRaiseChangedMethod)
                raiseChangedMethod = inpcedInfo.RaiseMethodImplementation;

            var inpcingInfo = INPCInfo.GetINPCingInfo(contextInfo, classSymbol);
            if(inpcingInfo.HasNoImplementation())
                interfaces.Add(new INPCingInterfaceGenerator());
            var impelementRaiseChangingMethod = inpcingInfo.ShouldImplementRaiseMethod();
            if(impelementRaiseChangingMethod)
                raiseChangingMethod = inpcingInfo.RaiseMethodImplementation;

            var isMvvmAvailable = ClassHelper.IsMvvmAvailable(contextInfo.Compilation);
            var mvvmComponentsList = new List<string>();

            var implIDEI = ClassHelper.GetImplementIDEIValue(contextInfo, classSymbol);
            var implISS = ClassHelper.GetImplementISSValue(contextInfo, classSymbol);
            if(implIDEI) {
                mvvmComponentsList.Add("IDataErrorInfo");
                if(isMvvmAvailable && !ClassHelper.IsInterfaceImplemented(classSymbol, contextInfo.IDEISymbol))
                    interfaces.Add(new IDataErrorInfoGenerator());
            }
            if(implISS) {
                mvvmComponentsList.Add("ISupportServices");
                if(isMvvmAvailable && contextInfo.ISSSymbol != null && !ClassHelper.IsInterfaceImplemented(classSymbol, contextInfo.ISSSymbol))
                    interfaces.Add(new ISupportServicesGenerator());
            }

            var needStaticChangedEventArgs = inpcedInfo.HasRaiseMethodWithEventArgsParameter || impelementRaiseChangedMethod;
            var needStaticChangingEventArgs = inpcingInfo.HasRaiseMethodWithEventArgsParameter || impelementRaiseChangingMethod;
            eventArgsGenerator = new EventArgsGenerator(needStaticChangedEventArgs, needStaticChangingEventArgs);

            var raiseChangedMethodParameter = needStaticChangedEventArgs ? "eventargs" : inpcedInfo.HasRaiseMethodWithStringParameter ? "string" : string.Empty;
            var raiseChangingMethodParameter = needStaticChangingEventArgs ? "eventargs" : inpcingInfo.HasRaiseMethodWithStringParameter ? "string" : string.Empty;

            var generateProperties = true;
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
                        var property = PropertyGenerator.Create(contextInfo, classSymbol, fieldSymbol, raiseChangedMethodParameter, raiseChangingMethodParameter);
                        if(property != null) {
                            properties.Add(property);
                            eventArgsGenerator.AddEventArgs(property.PropertyName);
                        }
                    }
            }

            var commandCandidates = ClassHelper.GetCommandCandidates(classSymbol, contextInfo.CommandAttributeSymbol);
            if(commandCandidates.Any()) {
                mvvmComponentsList.Add("Commands");
                if(isMvvmAvailable)
                    foreach(var methodSymbol in commandCandidates) {
                        var command = CommandGenerator.Create(contextInfo, classSymbol, methodSymbol);
                        if(command != null)
                            commands.Add(command);
                    }
            }

            if(mvvmComponentsList.Any())
                if(isMvvmAvailable)
                    usings += Environment.NewLine + "using DevExpress.Mvvm;";
                else
                    contextInfo.Context.ReportMVVMNotAvailable(classSymbol, mvvmComponentsList);
            if(classSymbol.IsGenericType) {
                genericTypes = classSymbol.TypeArguments.ToList();
            }
        }
        public string GetSourceCode() {
            var source = new StringBuilder();
            source.AppendLine(usings);
            source.AppendLine();
            source.AppendLine("#nullable enable");
            source.AppendLine();
            source.AppendLine($"namespace {@namespace} {{");

            source.Append($"partial class {name}".AddTabs(1));
            if(genericTypes.Any())
                source.Append($"<{genericTypes.Select(type => type.ToString()).ConcatToString(", ")}>");
            if(interfaces.Any()) {
                source.AppendLine($" : {interfaces.Select(@interface => @interface.GetName()).ConcatToString(", ")} {{");
                foreach(var @interface in interfaces)
                    source.AppendLine(@interface.GetImplementation().AddTabs(2));
                source.AppendLine();
            } else
                source.AppendLine(" {");

            if(!string.IsNullOrEmpty(raiseChangedMethod))
                source.AppendLine(raiseChangedMethod.AddTabs(2));
            if(!string.IsNullOrEmpty(raiseChangingMethod))
                source.AppendLine(raiseChangingMethod.AddTabs(2));
            if(!string.IsNullOrEmpty(raiseChangedMethod) || !string.IsNullOrEmpty(raiseChangingMethod))
                source.AppendLine();

            foreach(var property in properties)
                property.GetSourceCode(source, 2);
            foreach(var command in commands)
                command.GetSourceCode(source, 2);
            eventArgsGenerator.GetSourceCode(source, 2);

            source.AppendLine("}".AddTabs(1));
            source.AppendLine("}");

            return source.ToString();
        }
    }
}
