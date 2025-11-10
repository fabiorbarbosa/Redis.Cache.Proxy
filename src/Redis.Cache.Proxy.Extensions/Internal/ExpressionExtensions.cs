using System.Linq;
using System.Linq.Expressions;

namespace Redis.Cache.Proxy.Extensions.Internal;

internal static class ExpressionExtensions
{
    public static string GetKeyExpression(this LambdaExpression expression)
    {
        if (expression is null)
        {
            throw new ArgumentNullException(nameof(expression));
        }

        var parameterName = expression.Parameters[0].Type.Name;
        var visitor = new KeyExpressionVisitor();
        visitor.Visit(expression.Body);

        var cleanKeys = visitor.Keys
            .Select(static key => key.Replace(" ", string.Empty))
            .Distinct()
            .ToList();

        return $"{parameterName}:{string.Join(":", cleanKeys)}";
    }
}
