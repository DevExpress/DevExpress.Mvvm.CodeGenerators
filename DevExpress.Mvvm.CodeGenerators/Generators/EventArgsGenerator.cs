using System.Collections.Generic;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    class EventArgsGenerator {
        readonly bool createChangedEventArgs;
        readonly bool createChangingEventArgs;
        readonly List<string> propertyNameList = new();

        public EventArgsGenerator(bool createChangedEventArgs, bool createChangingEventArgs) =>
            (this.createChangedEventArgs, this.createChangingEventArgs) = (createChangedEventArgs, createChangingEventArgs);
        public void AddEventArgs(string propertyName) => propertyNameList.Add(propertyName);
        public void GetSourceCode(StringBuilder source, int tabs) {
            if(createChangedEventArgs)
                foreach(var propertyName in propertyNameList)
                    source.AppendLine($"static PropertyChangedEventArgs {propertyName}ChangedEventArgs = new PropertyChangedEventArgs(nameof({propertyName}));".AddTabs(tabs));
            if(createChangingEventArgs)
                foreach(var propertyName in propertyNameList)
                    source.AppendLine($"static PropertyChangingEventArgs {propertyName}ChangingEventArgs = new PropertyChangingEventArgs(nameof({propertyName}));".AddTabs(tabs));
        }
    }
}
