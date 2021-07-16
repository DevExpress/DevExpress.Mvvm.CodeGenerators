using System.Collections.Generic;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    static class EventArgsGenerator {
        public static void Generate(StringBuilder source, int tabs, bool createChangedEventArgs, bool createChangingEventArgs, IEnumerable<string> propertyNames) {
            if(createChangedEventArgs)
                foreach(var propertyName in propertyNames) {
                    source
                        .AppendTabs(tabs)
                        .Append("static PropertyChangedEventArgs ")
                        .Append(propertyName)
                        .Append("ChangedEventArgs = new PropertyChangedEventArgs(nameof(")
                        .Append(propertyName)
                        .AppendLine("));");
                }
            if(createChangingEventArgs)
                foreach(var propertyName in propertyNames) {
                    source
                        .AppendTabs(tabs)
                        .Append("static PropertyChangingEventArgs ")
                        .Append(propertyName)
                        .Append("ChangingEventArgs = new PropertyChangingEventArgs(nameof(")
                        .Append(propertyName)
                        .AppendLine("));");

                }
        }
    }
}
