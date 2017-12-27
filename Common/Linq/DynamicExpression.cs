using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Common.Linq
{
    public static class DynamicExpression
    {
        // Methods
        public static Type CreateClass(IEnumerable<DynamicProperty> properties)
        {
            return ClassFactory.Instance.GetDynamicClass(properties);
        }

        public static Type CreateClass(params DynamicProperty[] properties)
        {
            return ClassFactory.Instance.GetDynamicClass(properties);
        }

        public static Expression Parse(Type resultType, string expression, params object[] values)
        {
            return new ExpressionParser(null, expression, values).Parse(resultType);
        }

        public static Expression<Func<T, S>> ParseLambda<T, S>(string expression, params object[] values)
        {
            return (Expression<Func<T, S>>)ParseLambda(typeof(T), typeof(S), expression, values);
        }

        public static LambdaExpression ParseLambda(Type itType, Type resultType, string expression, params object[] values)
        {
            ParameterExpression[] parameters = new ParameterExpression[] { Expression.Parameter(itType, "") };
            return ParseLambda(parameters, resultType, expression, values);
        }

        public static LambdaExpression ParseLambda(ParameterExpression[] parameters, Type resultType, string expression, params object[] values)
        {
            return Expression.Lambda(new ExpressionParser(parameters, expression, values).Parse(resultType), parameters);
        }
    }



}
