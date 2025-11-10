using System.Collections;
using System.Linq;
using System.Linq.Expressions;

namespace Redis.Cache.Proxy.Extensions.Internal;

internal sealed class KeyExpressionVisitor : ExpressionVisitor
{
    public List<string> Keys { get; } = [];

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (IsEnumerableContains(node, out var memberExpr, out var collectionExpr))
        {
            var propertyName = memberExpr?.Member.Name;
            var collectionValue = GetValue(collectionExpr);
            Keys.Add($"{propertyName}:In:{collectionValue}");
        }
        else if (node.Object is MemberExpression memberObjExpr)
        {
            var propertyName = memberObjExpr.Member.Name;
            var value = node.Arguments.Count > 0 ? GetValue(node.Arguments[0]) : string.Empty;
            Keys.Add($"{propertyName}:{node.Method.Name}:{value}");
        }
        else
        {
            Keys.Add(node.ToString());
        }

        foreach (var argument in node.Arguments)
        {
            Visit(argument);
        }

        if (node.Object != null)
        {
            Visit(node.Object);
        }

        return node;
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        var left = GetMemberName(node.Left);
        var rightValue = GetValue(node.Right);
        if (!string.IsNullOrEmpty(left))
        {
            Keys.Add($"{left}:{node.NodeType}:{rightValue}");
        }

        Visit(node.Left);
        Visit(node.Right);
        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression is ParameterExpression)
        {
            Keys.Add(node.Member.Name);
        }
        else
        {
            GetValue(node);
        }

        return node;
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        Keys.Add($"Unary:{node.NodeType}");
        Visit(node.Operand);
        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        Keys.Add(node.Value?.ToString() ?? "null");
        return node;
    }

    protected override Expression VisitNewArray(NewArrayExpression node)
    {
        foreach (var expression in node.Expressions)
        {
            Visit(expression);
        }

        return node;
    }

    protected override Expression VisitListInit(ListInitExpression node)
    {
        foreach (var initializer in node.Initializers)
        {
            foreach (var argument in initializer.Arguments)
            {
                Visit(argument);
            }
        }

        return node;
    }

    private static string GetMemberName(Expression expression)
    {
        return expression switch
        {
            MemberExpression memberExpression => memberExpression.Member.Name,
            UnaryExpression unaryExpression => GetMemberName(unaryExpression.Operand),
            _ => expression.ToString()
        };
    }

    private static object GetValue(Expression? expression)
    {
        if (expression is null)
        {
            return "null";
        }

        return expression switch
        {
            ConstantExpression constantExpression => FormatValue(constantExpression.Value),
            MemberExpression memberExpression => FormatValue(Expression
                .Lambda<Func<object>>(Expression.Convert(memberExpression, typeof(object)))
                .Compile()
                .Invoke()),
            NewArrayExpression newArrayExpression => $"[{string.Join(",", newArrayExpression.Expressions.Select(GetValue))}]",
            ListInitExpression listInitExpression => $"[{string.Join(",", listInitExpression.Initializers.SelectMany(i => i.Arguments.Select(GetValue)))}]",
            UnaryExpression unaryExpression => GetValue(unaryExpression.Operand),
            _ => expression.ToString()
        };
    }

    private static string FormatValue(object? value)
    {
        if (value is IEnumerable enumerable and not string)
        {
            return $"[{string.Join(",", enumerable.Cast<object>())}]";
        }

        return value?.ToString()?.Replace(" ", string.Empty) ?? "null";
    }

    private static bool IsEnumerableContains(
        MethodCallExpression methodCall,
        out MemberExpression? memberExpression,
        out Expression? collectionExpression)
    {
        memberExpression = null;
        collectionExpression = null;

        if (methodCall.Method.Name != "Contains" || methodCall.Arguments.Count is < 1 or > 2)
        {
            return false;
        }

        if (methodCall.Object != null)
        {
            collectionExpression = methodCall.Object;
            if (methodCall.Arguments[0] is MemberExpression argumentMember)
            {
                memberExpression = argumentMember;
                return true;
            }
        }
        else if (methodCall.Arguments.Count == 2)
        {
            collectionExpression = methodCall.Arguments[0];
            if (methodCall.Arguments[1] is MemberExpression argumentMember)
            {
                memberExpression = argumentMember;
                return true;
            }
        }

        return false;
    }
}
