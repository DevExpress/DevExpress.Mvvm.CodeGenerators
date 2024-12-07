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
        public static IEnumerable<INamedTypeSymbol> GetParents(this INamedTypeSymbol typeSymbol) {
            for(INamedTypeSymbol parent = typeSymbol.BaseType!; parent != null; parent = parent.BaseType!) {
                yield return parent;
            }
        }

        #endregion

        #region String
        public static string ConcatToString(this IEnumerable<string> source, string separator) => string.Join(separator, source);
        public static string FirstToUpperCase(this string str) => string.IsNullOrEmpty(str) ? str : $"{str.Substring(0, 1).ToUpper()}{str.Substring(1)}";
        #endregion
        public static string TypeToString(this TypeKind type) => type == TypeKind.Structure ? "struct" : type.ToString().ToLower();

        public static string BoolToStringValue(this bool val) => val ? "true" : "false";

        internal static string ToStringValue(this RaiseMethodPrefix val) {
            return val switch {
                RaiseMethodPrefix.On => "On",
                RaiseMethodPrefix.Raise => "Raise",
                _ => throw new InvalidOperationException(),
            };
        }

        internal static RaiseMethodPrefix GetRasiePrefix(this SupportedMvvm mvvm) {
            return mvvm switch {
                SupportedMvvm.None or SupportedMvvm.Dx or SupportedMvvm.Prism or SupportedMvvm.MvvmLight => RaiseMethodPrefix.Raise,
                SupportedMvvm.MvvmToolkit => RaiseMethodPrefix.On,
                _ => throw new InvalidOperationException()
            };
        }
    }
}
