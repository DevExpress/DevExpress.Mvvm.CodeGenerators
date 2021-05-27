using System.Collections.Generic;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    static class EventArgsGenerator {
        public static void Generate(StringBuilder source, int tabs, bool createChangedEventArgs, bool createChangingEventArgs, IEnumerable<string> propertyNames) {
            if(createChangedEventArgs)
                foreach(var propertyName in propertyNames)
                    source.AppendLine($"static PropertyChangedEventArgs {propertyName}ChangedEventArgs = new PropertyChangedEventArgs(nameof({propertyName}));".AddTabs(tabs));
            if(createChangingEventArgs)
                foreach(var propertyName in propertyNames)
                    source.AppendLine($"static PropertyChangingEventArgs {propertyName}ChangingEventArgs = new PropertyChangingEventArgs(nameof({propertyName}));".AddTabs(tabs));
        }
    }
}
