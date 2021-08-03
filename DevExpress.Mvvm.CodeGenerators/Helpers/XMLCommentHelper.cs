using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    public class XMLCommentHelper {
        public static void AppendComment(SourceBuilder source, CSharpSyntaxNode symbol) {
            string comment = symbol.GetLeadingTrivia()
                .LastOrDefault(x => x.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
                .ToString();
            if(!string.IsNullOrEmpty(comment))
                source.Append("///").AppendLine(comment).RemoveLast(Environment.NewLine.Length);
        }
    }
}
