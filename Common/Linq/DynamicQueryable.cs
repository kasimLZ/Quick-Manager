using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Common.Linq
{
    public static class DynamicQueryable
    {
        // Methods
        public static bool Any(this IQueryable source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Type[] typeArguments = new Type[] { source.ElementType };
            Expression[] arguments = new Expression[] { source.Expression };
            return (bool)source.Provider.Execute(Expression.Call(typeof(Queryable), "Any", typeArguments, arguments));
        }

        public static int Count(this IQueryable source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Type[] typeArguments = new Type[] { source.ElementType };
            Expression[] arguments = new Expression[] { source.Expression };
            return (int)source.Provider.Execute(Expression.Call(typeof(Queryable), "Count", typeArguments, arguments));
        }

        public static IQueryable GroupBy(this IQueryable source, string keySelector, string elementSelector, params object[] values)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            if (elementSelector == null)
            {
                throw new ArgumentNullException("elementSelector");
            }
            LambdaExpression expression = DynamicExpression.ParseLambda(source.ElementType, null, keySelector, values);
            LambdaExpression expression2 = DynamicExpression.ParseLambda(source.ElementType, null, elementSelector, values);
            Type[] typeArguments = new Type[] { source.ElementType, expression.Body.Type, expression2.Body.Type };
            Expression[] arguments = new Expression[] { source.Expression, Expression.Quote(expression), Expression.Quote(expression2) };
            return source.Provider.CreateQuery(Expression.Call(typeof(Queryable), "GroupBy", typeArguments, arguments));
        }

        public static IQueryable OrderBy(this IQueryable source, string ordering, params object[] values)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (ordering == null)
            {
                throw new ArgumentNullException("ordering");
            }
            ParameterExpression[] parameters = new ParameterExpression[] { Expression.Parameter(source.ElementType, "") };
            Expression expression = source.Expression;
            string str = "OrderBy";
            string str2 = "OrderByDescending";
            foreach (DynamicOrdering ordering2 in new ExpressionParser(parameters, ordering, values).ParseOrdering())
            {
                Type[] typeArguments = new Type[] { source.ElementType, ordering2.Selector.Type };
                Expression[] arguments = new Expression[] { expression, Expression.Quote(Expression.Lambda(ordering2.Selector, parameters)) };
                expression = Expression.Call(typeof(Queryable), ordering2.Ascending ? str : str2, typeArguments, arguments);
                str = "ThenBy";
                str2 = "ThenByDescending";
            }
            return source.Provider.CreateQuery(expression);
        }
        

        public static IQueryable Select(this IQueryable source, string selector, params object[] values)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }
            LambdaExpression expression = DynamicExpression.ParseLambda(source.ElementType, null, selector, values);
            Type[] typeArguments = new Type[] { source.ElementType, expression.Body.Type };
            Expression[] arguments = new Expression[] { source.Expression, Expression.Quote(expression) };
            return source.Provider.CreateQuery(Expression.Call(typeof(Queryable), "Select", typeArguments, arguments));
        }

        public static IQueryable Skip(this IQueryable source, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Type[] typeArguments = new Type[] { source.ElementType };
            Expression[] arguments = new Expression[] { source.Expression, Expression.Constant(count) };
            return source.Provider.CreateQuery(Expression.Call(typeof(Queryable), "Skip", typeArguments, arguments));
        }

        public static IQueryable Take(this IQueryable source, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Type[] typeArguments = new Type[] { source.ElementType };
            Expression[] arguments = new Expression[] { source.Expression, Expression.Constant(count) };
            return source.Provider.CreateQuery(Expression.Call(typeof(Queryable), "Take", typeArguments, arguments));
        }

        public static IQueryable Where(this IQueryable source, string predicate, params object[] values)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            LambdaExpression expression = DynamicExpression.ParseLambda(source.ElementType, typeof(bool), predicate, values);
            Type[] typeArguments = new Type[] { source.ElementType };
            Expression[] arguments = new Expression[] { source.Expression, Expression.Quote(expression) };
            return source.Provider.CreateQuery(Expression.Call(typeof(Queryable), "Where", typeArguments, arguments));
        }
    }


}
