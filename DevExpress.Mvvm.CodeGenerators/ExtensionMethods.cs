using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    public static class ExtensionMethods {
        #region ITypeSymbol
        public static string ToDisplayStringNullable(this ITypeSymbol typeSymbol) => 
            typeSymbol.ToDisplayString(ToNullableFlowState(typeSymbol.NullableAnnotation));
        static NullableFlowState ToNullableFlowState(NullableAnnotation nullableAnnotation) =>
            nullableAnnotation switch {
                NullableAnnotation.None => NullableFlowState.None,
                NullableAnnotation.NotAnnotated => NullableFlowState.NotNull,
                NullableAnnotation.Annotated => NullableFlowState.MaybeNull,
                _ => NullableFlowState.None
            };
        #endregion

        #region String
        public static void AppendMultipleLinesWithTabs(this StringBuilder builder, string lines, int tabs) {
            foreach((int start, int length) in new LineEnumerator(lines)) {
                builder.AppendTabs(tabs).Append(lines, start, length).AppendLine();
            }
        }
        public struct LineEnumerator {
            string text { get; set; }
            int startIndex;
            public (int start, int length) Current { get; private set; }

            public LineEnumerator(string source) {
                text = source;
                Current = default;
                startIndex = 0;
            }
            public LineEnumerator GetEnumerator() {
                return this;
            }
            public bool MoveNext() {
                if(startIndex == text.Length) return false;
                var index = text.IndexOf("\r\n", startIndex);
                if(index != -1) {
                    Current = (startIndex, index  - startIndex);
                    startIndex = index + 2; ;
                    return true;
                } else {
                    Current = (startIndex, text.Length - startIndex);
                    startIndex = text.Length;
                    return true;
                }
            }
        }

        public static void AppendLineWithTabs(this StringBuilder builder, string line, int tabs) {
            AppendTabs(builder, tabs);
            builder.AppendLine(line);
        }
        public static void AppendWithTabs(this StringBuilder builder, string line, int tabs) {
            AppendTabs(builder, tabs);
            builder.Append(line);
        }
        public static StringBuilder AppendTabs(this StringBuilder builder, int tabs) {
            for(int i = 0; i < tabs; i++) {
                builder.Append("    ");
            }
            return builder;
        }
        public static StringBuilder AppendFirstToUpperCase(this StringBuilder builder, string str) {
            return builder.Append(char.ToUpper(str[0])).Append(str, 1, str.Length - 1);
        }
        public static void AppendMultipleLinesWithSeparator(this StringBuilder builder, IEnumerable<string> lines, string separator) {
            bool appendSeparator = false;
            foreach(var line in lines) {
                if(appendSeparator)
                    builder.Append(separator);
                builder.Append(line);
                appendSeparator = true;
            }
        }

        public static string ConcatToString(this IEnumerable<string> source, string separator) => string.Join(separator, source);
        public static string FirstToLowerCase(this string str) => $"{str.Substring(0, 1).ToLower()}{str.Substring(1)}";
        public static string FirstToUpperCase(this string str) => $"{str.Substring(0, 1).ToUpper()}{str.Substring(1)}";
        #endregion
        public static string TypeToString(this TypeKind type) => type == TypeKind.Structure ? "struct" : type.ToString().ToLower();

        public static string BoolToStringValue(this bool val) => val ? "true" : "false";
    }
}
