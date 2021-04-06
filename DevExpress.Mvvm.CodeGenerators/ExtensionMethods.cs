using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public static string ConcatToString(this IEnumerable<string> source, string separator) => string.Join(separator, source);
        public static string AddTabs(this string text, int tabs) {
            var indent = new string(' ', tabs * 4);
            return text.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                       .Select(str => string.IsNullOrEmpty(str) ? string.Empty : indent + str)
                       .ConcatToString(Environment.NewLine);
        }
        public static string FirstToLowerCase(this string str) => $"{str.Substring(0, 1).ToLower()}{str.Substring(1)}";
        public static string FirstToUpperCase(this string str) => $"{str.Substring(0, 1).ToUpper()}{str.Substring(1)}";
        #endregion
    }
}
