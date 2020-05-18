using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace Minsk.Compiler
{
    static class Program
    {
        static void Main(string[] args)
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
                var expression = parser.Parse();
                Console.ForegroundColor = ConsoleColor.Red;
                PrettyPrint(expression);
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

    class SyntaxToken : SyntaxNode
    {
        public override SyntaxKind Kind { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            return Enumerable.Empty<SyntaxNode>();
        }

        public int Position { get; }
        public string Text { get; }
        public object Value { get; }

        public SyntaxToken(SyntaxKind kind, int position, string text, object value)
        {
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;
        }
    }

    public enum SyntaxKind
    {
        NumberToken,
        WhiteSpaceToken,
        PlusToken,
        MinusToken,
        OpenParenthesisToken,
        SlashToken,
        StarToken,
        CloseParenthesisToken,
        BadToken,
        EndOfFileToken,
        BinaryExpression
    }

    class Lexer
    {
        private readonly string _text;
        private int _position;

        private Char Current
        {
            get
            {
                if (_position >= _text.Length)
                {
                    return '\0';
                }

                return _text[_position];
            }
        }

        public Lexer(string text)
        {
            _text = text;
        }

        private void Next()
        {
            _position++;
        }

        public SyntaxToken NextToken()
        {
            if (_position >= _text.Length)
            {
                return new SyntaxToken(SyntaxKind.EndOfFileToken, _position, "\0", null);
            }

            //<numbers>
            //+ - * /
            //<whitespace>]
            //badToken
            //EOF
            if (char.IsDigit(Current))
            {
                var start = _position;
                while (char.IsDigit(Current))
                {
                    Next();
                }

                var length = _position - start;
                var text = _text.Substring(start, length);
                int.TryParse(text, out var value);
                return new SyntaxToken(SyntaxKind.NumberToken, start, text, value);
            }

            if (char.IsWhiteSpace(Current))
            {
                var start = _position;
                while (char.IsWhiteSpace(Current))
                {
                    Next();
                }

                var length = _position - start;
                var text = _text.Substring(start, length);
                return new SyntaxToken(SyntaxKind.WhiteSpaceToken, start, text, null);
            }

            return Current switch
            {
                '+' => new SyntaxToken(SyntaxKind.PlusToken, _position++, "+", null),
                '-' => new SyntaxToken(SyntaxKind.MinusToken, _position++, "-", null),
                '*' => new SyntaxToken(SyntaxKind.StarToken, _position++, "*", null),
                '/' => new SyntaxToken(SyntaxKind.SlashToken, _position++, "/", null),
                '(' => new SyntaxToken(SyntaxKind.OpenParenthesisToken, _position++, "(", null),
                ')' => new SyntaxToken(SyntaxKind.CloseParenthesisToken, _position++, ")", null),
                _ => new SyntaxToken(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1), null)
            };
        }
    }

    public abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }
        public abstract IEnumerable<SyntaxNode> GetChildren();
    }

    public abstract class ExpressionSyntax : SyntaxNode
    {
    }

    sealed class NumberExpressionSyntax : ExpressionSyntax
    {
        public SyntaxToken NumberToken { get; }

        public NumberExpressionSyntax(SyntaxToken numberToken)
        {
            NumberToken = numberToken;
        }

        public override SyntaxKind Kind => SyntaxKind.NumberToken;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return NumberToken;
        }
    }

    sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        public ExpressionSyntax Left { get; }
        public SyntaxToken OperationToken { get; }
        public ExpressionSyntax Right { get; }
        public BinaryExpressionSyntax SyntaxToken { get; }

        public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operationToken, ExpressionSyntax right)
        {
            Left = left;
            OperationToken = operationToken;
            Right = right;
        }

        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Left;
            yield return OperationToken;
            yield return Right;
        }
    }

    public class Parser
    {
        private SyntaxToken[] _tokens;
        private int _position;

        public Parser(string text)
        {
            var tokens = new List<SyntaxToken>();
            var lexer = new Lexer(text);
            SyntaxToken token;
            do
            {
                token = lexer.NextToken();
                if (token.Kind != SyntaxKind.WhiteSpaceToken
                    && token.Kind != SyntaxKind.BadToken)
                {
                    tokens.Add(token);
                }
            } while (token.Kind != SyntaxKind.EndOfFileToken);

            _tokens = tokens.ToArray();
        }

        private SyntaxToken Peek(int offset)
        {
            var index = _position + offset;
            if (index >= _tokens.Length)
            {
                return _tokens.Last();
            }

            return _tokens[index];
        }

        private SyntaxToken Current => Peek(0);

        private SyntaxToken ReturnCurrentAndMoveNext()
        {
            var current = Current;
            _position++;
            return current;
        }

        private SyntaxToken Match(SyntaxKind kind)
        {
            if (Current.Kind == kind)
            {
                return ReturnCurrentAndMoveNext();
            }

            return new SyntaxToken(kind, Current.Position, null, null);
        }

        public ExpressionSyntax Parse()
        {
            var root = ParsePrimaryExpression();
            while (Current.Kind == SyntaxKind.PlusToken
                   || Current.Kind == SyntaxKind.MinusToken)
            {
                var operatorToken = ReturnCurrentAndMoveNext();
                var right = ParsePrimaryExpression();
                root = new BinaryExpressionSyntax(root, operatorToken, right);
            }

            return root;
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            var numberToken = Match(SyntaxKind.NumberToken);
            return new NumberExpressionSyntax(numberToken);
        }
    }
}