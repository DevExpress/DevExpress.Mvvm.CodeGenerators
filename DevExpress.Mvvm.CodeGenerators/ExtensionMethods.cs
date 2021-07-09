using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    static class ExtensionMethods {
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
            foreach(var line in lines.Split(new[] { Environment.NewLine }, StringSplitOptions.None)) {
                builder.AppendLineWithTabs(line, tabs);
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
        static void AppendTabs(StringBuilder builder, int tabs) {
            for(int i = 0; i < tabs; i++) {
                builder.Append("    ");
            }
        }

        public static string ConcatToString(this IEnumerable<string> source, string separator) => string.Join(separator, source);
        public static string FirstToLowerCase(this string str) => $"{str.Substring(0, 1).ToLower()}{str.Substring(1)}";
        public static string FirstToUpperCase(this string str) => $"{str.Substring(0, 1).ToUpper()}{str.Substring(1)}";
        #endregion
        public static string TypeToString(this TypeKind type) => type == TypeKind.Structure ? "struct" : type.ToString().ToLower();
    }
}
