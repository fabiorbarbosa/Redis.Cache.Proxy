using System.Linq.Expressions;

using Redis.Cache.Proxy.CrossCutting.Shared.Helpers;

namespace Redis.Cache.Proxy.CrossCutting.Shared.Extensions;

internal static class ExpressionExtension
{
    public static string GetKeyExpression(this LambdaExpression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var className = expression.Parameters[0].Type.Name;
        var visitor = new KeyExpressionVisitorHelper();
        visitor.Visit(expression.Body);

        var cleanKeys = visitor.Keys
            .Select(k => k.Replace(" ", ""))
            .Distinct()
            .ToList();

        return $"{className}:" + string.Join(":", cleanKeys);
    }
}
