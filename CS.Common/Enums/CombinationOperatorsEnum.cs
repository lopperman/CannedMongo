using System.Linq.Expressions;

namespace CS.Common.Enums
{
    public enum CombinationOperatorsEnum
    {
        Or = ExpressionType.Or,
        And = ExpressionType.And,
        Xor = ExpressionType.ExclusiveOr,
    }

}
