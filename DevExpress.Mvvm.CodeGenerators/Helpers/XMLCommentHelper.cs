using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace DevExpress.Mvvm.CodeGenerators {
    public class XMLCommentHelper {
        public static void AppendComment(SourceBuilder source, CSharpSyntaxNode symbol) {
            SyntaxTrivia commentTrivia = symbol.GetLeadingTrivia()
                .LastOrDefault(x => x.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                                    x.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));
            if(commentTrivia.IsKind(SyntaxKind.None))
                return;
            source.AppendMultipleLines(commentTrivia.ToFullString(), trimLeadingWhiteSpace: true);
        }
    }
}
