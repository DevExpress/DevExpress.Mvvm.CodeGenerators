using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    public class XMLCommentHelper {
        public static void AppendComment(SourceBuilder source, CSharpSyntaxNode symbol) {
            SyntaxTrivia commentTrivia = symbol.GetLeadingTrivia()
                .LastOrDefault(x => x.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia));
            if(commentTrivia.IsKind(SyntaxKind.None))
                return;
            source.AppendMultipleLines(commentTrivia.ToFullString(), trimLeadingWhiteSpace: true);
        }
    }
}
