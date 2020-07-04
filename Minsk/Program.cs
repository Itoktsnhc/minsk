using System;
using System.Linq;
using Minsk.CodeAnalysis;
using Minsk.CodeAnalysis.Syntax;

namespace Minsk
{
    static class Program
    {
        static void Main()
        {
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    return;
                }

                var parser = new Parser(line);
                var syntaxTree = parser.Parse();
                Console.ForegroundColor = ConsoleColor.Green;
                PrettyPrint(syntaxTree.Root);
                Console.ForegroundColor = ConsoleColor.Red;
                if (syntaxTree.Diagnostics.Any())
                {
                    foreach (var errorMsg in syntaxTree.Diagnostics)
                    {
                        Console.WriteLine(errorMsg);
                    }
                }
                else
                {
                    var e = new Evaluator(syntaxTree.Root);
                    Console.WriteLine($"result is {e.Evaluate()}");
                }

                Console.ResetColor();
            }
        }

        static void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true)
        {
            var marker = isLast ? "└──" : "├──";
            Console.Write(indent);
            Console.Write(marker);
            Console.Write(node.Kind);
            if (node is SyntaxToken t && t.Value != null)
            {
                Console.Write(" ");
                Console.Write(t.Value);
            }

            Console.WriteLine();
            indent += isLast ? "    " : "|   ";
            var lastChild = node.GetChildren().LastOrDefault();
            foreach (var child in node.GetChildren())
            {
                PrettyPrint(child, indent, child == lastChild);
            }
        }
    }
}