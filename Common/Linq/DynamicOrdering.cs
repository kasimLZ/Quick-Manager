using System.Linq.Expressions;

namespace Common.Linq
{
    internal class DynamicOrdering
    {
        // Fields
        public bool Ascending;
        public Expression Selector;
    }
}
