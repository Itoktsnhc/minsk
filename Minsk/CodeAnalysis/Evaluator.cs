using System;
using Minsk.CodeAnalysis.Syntax;

namespace Minsk.CodeAnalysis
{
    public class Evaluator
    {
        private readonly ExpressionSyntax _node;

        public Evaluator(ExpressionSyntax node)
        {
            _node = node;
        }

        public int Evaluate()
        {
            return EvaluateExpression(_node);
        }

        private int EvaluateExpression(ExpressionSyntax node)
        {
            switch (node)
            {
                case LiteralExpressionSyntax n:
                    return (int) n.LiteralToken.Value;
                case UnaryExpressionSyntax u:
                {
                    var operand = EvaluateExpression(u.Operand);
                    if (u.OperatorToken.Kind == SyntaxKind.MinusToken)
                    {
                        return -operand;
                    }

                    if (u.OperatorToken.Kind == SyntaxKind.PlusToken)
                    {
                        return operand;
                    }

                    throw new Exception($"Unexpected unary token {u.OperatorToken.Kind}");
                }
                case BinaryExpressionSyntax b:
                {
                    var left = EvaluateExpression(b.Left);
                    var right = EvaluateExpression(b.Right);

                    return b.OperatorToken.Kind switch
                    {
                        SyntaxKind.PlusToken => left + right,
                        SyntaxKind.MinusToken => left - right,
                        SyntaxKind.StarToken => left * right,
                        SyntaxKind.SlashToken => left / right,
                        _ => throw new Exception($"Unexpected binary operator {b.OperatorToken.Kind}")
                    };
                }
                case ParenthesizedExpressionSyntax p:
                    return EvaluateExpression(p.Expression);
            }

            throw new Exception($"Unexpected node {node.Kind}");
        }
    }
}