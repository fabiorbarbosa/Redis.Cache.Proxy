using System.Collections;
using System.Linq.Expressions;

namespace Redis.Cache.Proxy.CrossCutting.Shared.Helpers;

internal class KeyExpressionVisitorHelper : ExpressionVisitor
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
            object value = node.Arguments.Count > 0 ? GetValue(node.Arguments[0]) : "";
            Keys.Add($"{propertyName}:{node.Method.Name}:{value}");
        }
        else
        {
            Keys.Add(node.ToString());
        }

        foreach (var arg in node.Arguments)
        {
            Visit(arg);
        }
        if (node.Object != null)
        {
            Visit(node.Object);
        }
        return node;
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        string left = GetMemberName(node.Left);
        object rightValue = GetValue(node.Right);
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
            Keys.Add($"{node.Member.Name}");
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
        foreach (var e in node.Expressions)
        {
            Visit(e);
        }
        return node;
    }

    protected override Expression VisitListInit(ListInitExpression node)
    {
        foreach (var init in node.Initializers)
        {
            foreach (var arg in init.Arguments)
            {
                Visit(arg);
            }
        }
        return node;
    }

    private static string GetMemberName(Expression expr)
    {
        if (expr is MemberExpression memberExpr)
        {
            return memberExpr.Member.Name;
        }
        if (expr is UnaryExpression unaryExpr)
        {
            return GetMemberName(unaryExpr.Operand);
        }
        return expr.ToString();
    }

    private static object GetValue(Expression expr)
    {
        if (expr is ConstantExpression constExpr)
        {
            return FormatValue(constExpr.Value);
        }
        if (expr is MemberExpression memberExpr)
        {
            var objectMember = Expression.Convert(memberExpr, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            return FormatValue(getterLambda.Compile().Invoke());
        }
        if (expr is NewArrayExpression newArrayExpr)
        {
            var values = newArrayExpr.Expressions.Select(GetValue);
            return $"[{string.Join(",", values)}]";
        }
        if (expr is ListInitExpression listInitExpr)
        {
            var values = listInitExpr.Initializers.SelectMany(i => i.Arguments.Select(GetValue));
            return $"[{string.Join(",", values)}]";
        }
        if (expr is UnaryExpression unaryExpr)
        {
            return GetValue(unaryExpr.Operand);
        }
        return expr.ToString();
    }

    private static string FormatValue(object value)
    {
        if (value is IEnumerable enumerable && !(value is string))
        {
            return $"[{string.Join(",", enumerable.Cast<object>())}]";
        }
        return value?.ToString()?.Replace(" ", "") ?? "null";
    }

    private static bool IsEnumerableContains(MethodCallExpression methodCall, out MemberExpression? memberExpr, out Expression? collectionExpr)
    {
        memberExpr = null;
        collectionExpr = null;
        if (methodCall.Method.Name == "Contains" && methodCall.Arguments.Count == 1)
        {
            if (methodCall.Object != null)
            {
                collectionExpr = methodCall.Object;
                if (methodCall.Arguments[0] is MemberExpression argMember)
                {
                    memberExpr = argMember;
                    return true;
                }
            }
            else if (methodCall.Arguments.Count == 2)
            {
                collectionExpr = methodCall.Arguments[0];
                if (methodCall.Arguments[1] is MemberExpression argMember)
                {
                    memberExpr = argMember;
                    return true;
                }
            }
        }
        return false;
    }
}