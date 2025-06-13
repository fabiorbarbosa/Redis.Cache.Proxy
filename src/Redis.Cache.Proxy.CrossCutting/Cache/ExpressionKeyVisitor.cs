using System.Linq.Expressions;

namespace Redis.Cache.Proxy.CrossCutting.Cache;

internal class ExpressionKeyVisitor : ExpressionVisitor
{
    private readonly List<string> _parts = [];

    protected override Expression VisitBinary(BinaryExpression node)
    {
        if (node.NodeType == ExpressionType.Equal)
        {
            var member = node.Left as MemberExpression;
            var constant = node.Right as ConstantExpression;

            if (member != null && constant != null)
                _parts.Add($"{member.Member.Name}={constant.Value}");
        }
        return base.VisitBinary(node);
    }

    public string GetKey() => string.Join(";", _parts);
}