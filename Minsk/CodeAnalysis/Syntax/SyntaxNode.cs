using System.Collections.Generic;

namespace Minsk.CodeAnalysis.Syntax
{
    abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }

        public abstract IEnumerable<SyntaxNode> GetChildren();
    }
}