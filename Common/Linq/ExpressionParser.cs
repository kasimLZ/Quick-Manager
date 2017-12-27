using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Common.Linq
{
    internal class ExpressionParser
    {
        private char ch;
        private IDictionary<string, object> externals;
        private static readonly Expression falseLiteral = Expression.Constant(false);
        private ParameterExpression it;
        private static readonly string keywordIif = "iif";
        private static readonly string keywordIt = "it";
        private static readonly string keywordNew = "new";
        private static Dictionary<string, object> keywords;
        private Dictionary<Expression, string> literals;
        private static readonly Expression nullLiteral = Expression.Constant(null);
        private static readonly Type[] predefinedTypes = new Type[] {
        typeof(object), typeof(bool), typeof(char), typeof(string), typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(DateTime),
        typeof(TimeSpan), typeof(Guid), typeof(Math), typeof(Convert)
     };
        private Dictionary<string, object> symbols;
        private string text;
        private int textLen;
        private int textPos;
        private Token token;
        private static readonly Expression trueLiteral = Expression.Constant(true);

        
        public ExpressionParser(ParameterExpression[] parameters, string expression, object[] values)
        {
            if (keywords == null)
            {
                keywords = CreateKeywords();
            }
            symbols = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            literals = new Dictionary<Expression, string>();
            if (parameters != null)
            {
                ProcessParameters(parameters);
            }
            if (values != null)
            {
                ProcessValues(values);
            }
            text = expression ?? throw new ArgumentNullException("expression");
            textLen = this.text.Length;
            SetTextPos(0);
            NextToken();
        }

        private static void AddInterface(List<Type> types, Type type)
        {
            if (!types.Contains(type))
            {
                types.Add(type);
                foreach (Type type2 in type.GetInterfaces())
                {
                    AddInterface(types, type2);
                }
            }
        }

        private void AddSymbol(string name, object value)
        {
            if (symbols.ContainsKey(name))
            {
                object[] args = new object[] { name };
                throw ParseError("The identifier '{0}' was defined more than once", args);
            }
            symbols.Add(name, value);
        }

        private void CheckAndPromoteOperand(Type signatures, string opName, ref Expression expr, int errorPos)
        {
            MethodBase base2;
            Expression[] args = new Expression[] { expr };
            if (FindMethod(signatures, "F", false, args, out base2) != 1)
            {
                object[] objArray1 = new object[] { opName, GetTypeName(args[0].Type) };
                throw ParseError(errorPos, "Operator '{0}' incompatible with operand type '{1}'", objArray1);
            }
            expr = args[0];
        }

        private void CheckAndPromoteOperands(Type signatures, string opName, ref Expression left, ref Expression right, int errorPos)
        {
            Expression[] args = new Expression[] { left, right };
            if (FindMethod(signatures, "F", false, args, out MethodBase base2) != 1)
            {
                throw IncompatibleOperandsError(opName, left, right, errorPos);
            }
            left = args[0];
            right = args[1];
        }

        private static int CompareConversions(Type s, Type t1, Type t2)
        {
            if (t1 != t2)
            {
                if (s == t1)
                {
                    return 1;
                }
                if (s == t2)
                {
                    return -1;
                }
                bool flag = IsCompatibleWith(t1, t2);
                bool flag2 = IsCompatibleWith(t2, t1);
                if (flag && !flag2)
                {
                    return 1;
                }
                if (flag2 && !flag)
                {
                    return -1;
                }
                if (IsSignedIntegralType(t1) && IsUnsignedIntegralType(t2))
                {
                    return 1;
                }
                if (IsSignedIntegralType(t2) && IsUnsignedIntegralType(t1))
                {
                    return -1;
                }
            }
            return 0;
        }

        private static Dictionary<string, object> CreateKeywords()
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "true", trueLiteral },
                { "false", falseLiteral },
                { "null", nullLiteral },
                { keywordIt, keywordIt },
                { keywordIif, keywordIif },
                { keywordNew, keywordNew }
            };
            foreach (Type type in predefinedTypes) dictionary.Add(type.Name, type);
            return dictionary;
        }

        private Expression CreateLiteral(object value, string text)
        {
            ConstantExpression key = Expression.Constant(value);
            literals.Add(key, text);
            return key;
        }

        private int FindBestMethod(IEnumerable<MethodBase> methods, Expression[] args, out MethodBase method)
        {
            MethodData[] applicable = methods.Select(m => new MethodData { MethodBase = m, Parameters = m.GetParameters() }).Where(m => IsApplicable(m, args)).ToArray();
            if (applicable.Length > 1)
            {
                applicable = (from m in applicable
                              where applicable.All(delegate (MethodData n)
                              {
                                  if (m != n)
                                  {
                                      return IsBetterThan(args, m, n);
                                  }
                                  return true;
                              })
                              select m).ToArray();
            }
            if (applicable.Length == 1)
            {
                MethodData data = applicable[0];
                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = data.Args[i];
                }
                method = data.MethodBase;
            }
            else
            {
                method = null;
            }
            return applicable.Length;
        }

        private static Type FindGenericType(Type generic, Type type)
        {
            while ((type != null) && (type != typeof(object)))
            {
                if (type.IsGenericType && (type.GetGenericTypeDefinition() == generic))
                {
                    return type;
                }
                if (generic.IsInterface)
                {
                    foreach (Type type2 in type.GetInterfaces())
                    {
                        Type type3 = FindGenericType(generic, type2);
                        if (type3 != null)
                        {
                            return type3;
                        }
                    }
                }
                type = type.BaseType;
            }
            return null;
        }

        private int FindIndexer(Type type, Expression[] args, out MethodBase method)
        {
            using (IEnumerator<Type> enumerator = SelfAndBaseTypes(type).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MemberInfo[] defaultMembers = enumerator.Current.GetDefaultMembers();
                    if (defaultMembers.Length != 0)
                    {
                        IEnumerable<MethodBase> methods = defaultMembers.OfType<PropertyInfo>().Select(p => (MethodBase)p.GetGetMethod()).Where(m => m != null);
                        int num = this.FindBestMethod(methods, args, out method);
                        if (num != 0)
                        {
                            return num;
                        }
                    }
                }
            }
            method = null;
            return 0;
        }

        private int FindMethod(Type type, string methodName, bool staticAccess, Expression[] args, out MethodBase method)
        {
            BindingFlags bindingAttr = (BindingFlags.Public | BindingFlags.DeclaredOnly) | (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
            using (IEnumerator<Type> enumerator = SelfAndBaseTypes(type).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MemberInfo[] source = enumerator.Current.FindMembers(MemberTypes.Method, bindingAttr, Type.FilterNameIgnoreCase, methodName);
                    int num = this.FindBestMethod(source.Cast<MethodBase>(), args, out method);
                    if (num != 0)
                    {
                        return num;
                    }
                }
            }
            method = null;
            return 0;
        }

        private MemberInfo FindPropertyOrField(Type type, string memberName, bool staticAccess)
        {
            BindingFlags bindingAttr = (BindingFlags.Public | BindingFlags.DeclaredOnly) | (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
            using (IEnumerator<Type> enumerator = SelfAndBaseTypes(type).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MemberInfo[] infoArray = enumerator.Current.FindMembers(MemberTypes.Property | MemberTypes.Field, bindingAttr, Type.FilterNameIgnoreCase, memberName);
                    if (infoArray.Length != 0)
                    {
                        return infoArray[0];
                    }
                }
            }
            return null;
        }

        private Expression GenerateAdd(Expression left, Expression right) => 
            ((left.Type == typeof(string)) && (right.Type == typeof(string))) ? GenerateStaticMethodCall("Concat", left, right) : Expression.Add(left, right);

        private Expression GenerateConditional(Expression test, Expression expr1, Expression expr2, int errorPos)
        {
            if (test.Type != typeof(bool))
            {
                throw ParseError(errorPos, "The first expression must be of type 'Boolean'", new object[0]);
            }
            if (expr1.Type != expr2.Type)
            {
                Expression expression = (expr2 != nullLiteral) ? PromoteExpression(expr1, expr2.Type, true) : null;
                Expression expression2 = (expr1 != nullLiteral) ? PromoteExpression(expr2, expr1.Type, true) : null;
                if ((expression == null) || (expression2 != null))
                {
                    if ((expression2 == null) || (expression != null))
                    {
                        string str = (expr1 != nullLiteral) ? expr1.Type.Name : "null";
                        string str2 = (expr2 != nullLiteral) ? expr2.Type.Name : "null";
                        if ((expression != null) && (expression2 != null))
                        {
                            throw ParseError(errorPos, "Both of the types '{0}' and '{1}' convert to the other", new object[] { str, str2 });
                        }
                        throw ParseError(errorPos, "Neither of the types '{0}' and '{1}' converts to the other", new object[] { str, str2 });
                    }
                    expr2 = expression2;
                }
                else
                {
                    expr1 = expression;
                }
            }
            return Expression.Condition(test, expr1, expr2);
        }

        private Expression GenerateConversion(Expression expr, Type type, int errorPos)
        {
            Type type2 = expr.Type;
            if (type2 == type)
            {
                return expr;
            }
            if (type2.IsValueType && type.IsValueType)
            {
                if ((IsNullableType(type2) || IsNullableType(type)) && (GetNonNullableType(type2) == GetNonNullableType(type)))
                {
                    return Expression.Convert(expr, type);
                }
                if (((IsNumericType(type2) || IsEnumType(type2)) && IsNumericType(type)) || IsEnumType(type))
                {
                    return Expression.ConvertChecked(expr, type);
                }
            }
            if ((type2.IsAssignableFrom(type) || type.IsAssignableFrom(type2)) || (type2.IsInterface || type.IsInterface))
            {
                return Expression.Convert(expr, type);
            }
            object[] args = new object[] { GetTypeName(type2), GetTypeName(type) };
            throw this.ParseError(errorPos, "A value of type '{0}' cannot be converted to type '{1}'", args);
        }

        private Expression GenerateEqual(Expression left, Expression right) => Expression.Equal(left, right);

        private Expression GenerateGreaterThan(Expression left, Expression right) => 
            left.Type == typeof(string) ? Expression.GreaterThan(GenerateStaticMethodCall("Compare", left, right), Expression.Constant(0)) : Expression.GreaterThan(left, right);

        private Expression GenerateGreaterThanEqual(Expression left, Expression right) => 
            left.Type == typeof(string) ? Expression.GreaterThanOrEqual(GenerateStaticMethodCall("Compare", left, right), Expression.Constant(0)) : Expression.GreaterThanOrEqual(left, right);

        private Expression GenerateLessThan(Expression left, Expression right)
        {
            if (left.Type == typeof(string))
            {
                return Expression.LessThan(this.GenerateStaticMethodCall("Compare", left, right), Expression.Constant(0));
            }
            return Expression.LessThan(left, right);
        }

        private Expression GenerateLessThanEqual(Expression left, Expression right)
        {
            if (left.Type == typeof(string))
            {
                return Expression.LessThanOrEqual(this.GenerateStaticMethodCall("Compare", left, right), Expression.Constant(0));
            }
            return Expression.LessThanOrEqual(left, right);
        }

        private Expression GenerateNotEqual(Expression left, Expression right)
        {
            return Expression.NotEqual(left, right);
        }

        private Expression GenerateStaticMethodCall(string methodName, Expression left, Expression right)
        {
            Expression[] arguments = new Expression[] { left, right };
            return Expression.Call(null, this.GetStaticMethod(methodName, left, right), arguments);
        }

        private Expression GenerateStringConcat(Expression left, Expression right)
        {
            Type[] types = new Type[] { typeof(object), typeof(object) };
            Expression[] arguments = new Expression[] { left, right };
            return Expression.Call(null, typeof(string).GetMethod("Concat", types), arguments);
        }

        private Expression GenerateSubtract(Expression left, Expression right)
        {
            return Expression.Subtract(left, right);
        }

        private string GetIdentifier()
        {
            this.ValidateToken(TokenId.Identifier, "Identifier expected");
            string text = this.token.text;
            if ((text.Length > 1) && (text[0] == '@'))
            {
                text = text.Substring(1);
            }
            return text;
        }

        private static Type GetNonNullableType(Type type)
        {
            if (!IsNullableType(type))
            {
                return type;
            }
            return type.GetGenericArguments()[0];
        }

        private static int GetNumericTypeKind(Type type)
        {
            type = GetNonNullableType(type);
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Char:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        return 1;

                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                        return 2;

                    case TypeCode.Byte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return 3;
                }
            }
            return 0;
        }

        private MethodInfo GetStaticMethod(string methodName, Expression left, Expression right) => left.Type.GetMethod(methodName, new Type[] { left.Type, right.Type });

        private static string GetTypeName(Type type)
        {
            Type nonNullableType = GetNonNullableType(type);
            string name = nonNullableType.Name;
            if (type != nonNullableType)
            {
                name = name + "?";
            }
            return name;
        }

        private Exception IncompatibleOperandsError(string opName, Expression left, Expression right, int pos) =>
            ParseError(pos, "Operator '{0}' incompatible with operand types '{1}' and '{2}'", new object[] { opName, GetTypeName(left.Type), GetTypeName(right.Type) });

        private bool IsApplicable(MethodData method, Expression[] args)
        {
            if (method.Parameters.Length != args.Length)
            {
                return false;
            }
            Expression[] expressionArray = new Expression[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                ParameterInfo info = method.Parameters[i];
                if (info.IsOut)
                {
                    return false;
                }
                Expression expression = this.PromoteExpression(args[i], info.ParameterType, false);
                if (expression == null)
                {
                    return false;
                }
                expressionArray[i] = expression;
            }
            method.Args = expressionArray;
            return true;
        }

        private static bool IsBetterThan(Expression[] args, MethodData m1, MethodData m2)
        {
            bool flag = false;
            for (int i = 0; i < args.Length; i++)
            {
                int num2 = CompareConversions(args[i].Type, m1.Parameters[i].ParameterType, m2.Parameters[i].ParameterType);
                if (num2 < 0)
                {
                    return false;
                }
                if (num2 > 0)
                {
                    flag = true;
                }
            }
            return flag;
        }

        private static bool IsCompatibleWith(Type source, Type target)
        {
            if (source == target)
            {
                return true;
            }
            if (!target.IsValueType)
            {
                return target.IsAssignableFrom(source);
            }
            Type nonNullableType = GetNonNullableType(source);
            Type type = GetNonNullableType(target);
            if ((nonNullableType == source) || (type != target))
            {
                TypeCode code = nonNullableType.IsEnum ? TypeCode.Object : Type.GetTypeCode(nonNullableType);
                TypeCode code2 = type.IsEnum ? TypeCode.Object : Type.GetTypeCode(type);
                switch (code)
                {
                    case TypeCode.SByte:
                        switch (code2)
                        {
                            case TypeCode.SByte:
                            case TypeCode.Int16:
                            case TypeCode.Int32:
                            case TypeCode.Int64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                return true;
                        }
                        break;

                    case TypeCode.Byte:
                        switch (code2)
                        {
                            case TypeCode.Byte:
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                return true;
                        }
                        break;

                    case TypeCode.Int16:
                        switch (code2)
                        {
                            case TypeCode.Int16:
                            case TypeCode.Int32:
                            case TypeCode.Int64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                return true;
                        }
                        break;

                    case TypeCode.UInt16:
                        switch (code2)
                        {
                            case TypeCode.UInt16:
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                return true;
                        }
                        break;

                    case TypeCode.Int32:
                        switch (code2)
                        {
                            case TypeCode.Int32:
                            case TypeCode.Int64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                return true;
                        }
                        break;

                    case TypeCode.UInt32:
                        switch (code2)
                        {
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                return true;
                        }
                        break;

                    case TypeCode.Int64:
                        switch (code2)
                        {
                            case TypeCode.Int64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                return true;
                        }
                        break;

                    case TypeCode.UInt64:
                        switch (code2)
                        {
                            case TypeCode.UInt64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                return true;
                        }
                        break;

                    case TypeCode.Single:
                        if ((code2 != TypeCode.Single) && (code2 != TypeCode.Double))
                        {
                            break;
                        }
                        return true;

                    default:
                        if (nonNullableType == type)
                        {
                            return true;
                        }
                        break;
                }
            }
            return false;
        }

        private static bool IsEnumType(Type type) => GetNonNullableType(type).IsEnum;

        private static bool IsNullableType(Type type) => (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));

        private static bool IsNumericType(Type type) => (GetNumericTypeKind(type) > 0);

        private static bool IsPredefinedType(Type type)
        {
            Type[] predefinedTypes = ExpressionParser.predefinedTypes;
            for (int i = 0; i < predefinedTypes.Length; i++)
            {
                if (predefinedTypes[i] == type)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsSignedIntegralType(Type type) => (GetNumericTypeKind(type) == 2);

        private static bool IsUnsignedIntegralType(Type type) => (GetNumericTypeKind(type) == 3);

        private void NextChar()
        {
            if (textPos < this.textLen)
            {
                textPos++;
            }
            ch = (textPos < textLen) ? text[textPos] : '\0';
        }

        private void NextToken()
        {
            TokenId closeBracket;
            while (char.IsWhiteSpace(this.ch))
            {
                this.NextChar();
            }
            int textPos = this.textPos;
            switch (this.ch)
            {
                case ']':
                    this.NextChar();
                    closeBracket = TokenId.CloseBracket;
                    break;

                case '|':
                    NextChar();
                    if (this.ch == '|')
                    {
                        NextChar();
                        closeBracket = TokenId.DoubleBar;
                    }
                    else
                    {
                        closeBracket = TokenId.Bar;
                    }
                    break;

                case '!':
                    NextChar();
                    if (this.ch != '=')
                    {
                        closeBracket = TokenId.Exclamation;
                    }
                    else
                    {
                        NextChar();
                        closeBracket = TokenId.ExclamationEqual;
                    }
                    break;

                case '"':
                case '\'':
                    {
                        char ch = this.ch;
                        do
                        {
                            NextChar();
                            while ((this.textPos < textLen) && (this.ch != ch))
                            {
                                NextChar();
                            }
                            if (this.textPos == this.textLen)
                            {
                                throw ParseError(this.textPos, "Unterminated string literal", new object[0]);
                            }
                            NextChar();
                        }
                        while (this.ch == ch);
                        closeBracket = TokenId.StringLiteral;
                        break;
                    }
                case '%':
                    NextChar();
                    closeBracket = TokenId.Percent;
                    break;

                case '&':
                    NextChar();
                    if (this.ch != '&')
                    {
                        closeBracket = TokenId.Amphersand;
                    }
                    else
                    {
                        NextChar();
                        closeBracket = TokenId.DoubleAmphersand;
                    }
                    break;

                case '(':
                    NextChar();
                    closeBracket = TokenId.OpenParen;
                    break;

                case ')':
                    NextChar();
                    closeBracket = TokenId.CloseParen;
                    break;

                case '*':
                    NextChar();
                    closeBracket = TokenId.Asterisk;
                    break;

                case '+':
                    NextChar();
                    closeBracket = TokenId.Plus;
                    break;

                case ',':
                    NextChar();
                    closeBracket = TokenId.Comma;
                    break;

                case '-':
                    NextChar();
                    closeBracket = TokenId.Minus;
                    break;

                case '.':
                    NextChar();
                    closeBracket = TokenId.Dot;
                    break;

                case '/':
                    NextChar();
                    closeBracket = TokenId.Slash;
                    break;

                case ':':
                    NextChar();
                    closeBracket = TokenId.Colon;
                    break;

                case '<':
                    NextChar();
                    if (this.ch != '=')
                    {
                        if (ch == '>')
                        {
                            NextChar();
                            closeBracket = TokenId.LessGreater;
                        }
                        else
                        {
                            closeBracket = TokenId.LessThan;
                        }
                    }
                    else
                    {
                        NextChar();
                        closeBracket = TokenId.LessThanEqual;
                    }
                    break;

                case '=':
                    NextChar();
                    if (this.ch != '=')
                    {
                        closeBracket = TokenId.Equal;
                    }
                    else
                    {
                        NextChar();
                        closeBracket = TokenId.DoubleEqual;
                    }
                    break;

                case '>':
                    NextChar();
                    if (this.ch != '=')
                    {
                        closeBracket = TokenId.GreaterThan;
                    }
                    else
                    {
                        NextChar();
                        closeBracket = TokenId.GreaterThanEqual;
                    }
                    break;

                case '?':
                    NextChar();
                    closeBracket = TokenId.Question;
                    break;

                case '[':
                    NextChar();
                    closeBracket = TokenId.OpenBracket;
                    break;

                default:
                    if ((char.IsLetter(this.ch) || (this.ch == '@')) || (this.ch == '_'))
                    {
                        do
                        {
                            NextChar();
                        }
                        while (char.IsLetterOrDigit(ch) || (ch == '_'));
                        closeBracket = TokenId.Identifier;
                    }
                    else if (char.IsDigit(this.ch))
                    {
                        closeBracket = TokenId.IntegerLiteral;
                        do
                        {
                            NextChar();
                        }
                        while (char.IsDigit(ch));
                        if (ch == '.')
                        {
                            closeBracket = TokenId.RealLiteral;
                            NextChar();
                            ValidateDigit();
                            do
                            {
                                NextChar();
                            }
                            while (char.IsDigit(ch));
                        }
                        if ((ch == 'E') || (ch == 'e'))
                        {
                            closeBracket = TokenId.RealLiteral;
                            NextChar();
                            if ((ch == '+') || (ch == '-'))
                            {
                                NextChar();
                            }
                            ValidateDigit();
                            do
                            {
                                NextChar();
                            }
                            while (char.IsDigit(ch));
                        }
                        if ((ch == 'F') || (ch == 'f'))
                        {
                            NextChar();
                        }
                    }
                    else if (this.textPos == this.textLen)
                    {
                        closeBracket = TokenId.End;
                    }
                    else
                    {
                        throw this.ParseError(this.textPos, "Syntax error '{0}'", new object[] { ch });
                    }
                    break;
            }
           token.id = closeBracket;
           token.text = text.Substring(textPos, this.textPos - textPos);
           token.pos = textPos;
        }

        public Expression Parse(Type resultType)
        {
            int pos = token.pos;
            Expression expr = this.ParseExpression();
            if ((resultType != null) && ((expr = PromoteExpression(expr, resultType, true)) == null))
            {
                throw ParseError(pos, "Expression of type '{0}' expected", new object[] { GetTypeName(resultType) });
            }
            ValidateToken(TokenId.End, "Syntax error");
            return expr;
        }

        private Expression ParseAdditive()
        {
            Expression left = ParseMultiplicative();
            while (((this.token.id == TokenId.Plus) || (this.token.id == TokenId.Minus)) || (this.token.id == TokenId.Amphersand))
            {
                Token token = this.token;
                NextToken();
                Expression right = ParseMultiplicative();
                switch (token.id)
                {
                    case TokenId.Amphersand:
                        goto Label_00C1;

                    case TokenId.Plus:
                        if ((left.Type == typeof(string)) || (right.Type == typeof(string)))
                        {
                            goto Label_00C1;
                        }
                        this.CheckAndPromoteOperands(typeof(IAddSignatures), token.text, ref left, ref right, token.pos);
                        left = GenerateAdd(left, right);
                        break;

                    case TokenId.Minus:
                        CheckAndPromoteOperands(typeof(ISubtractSignatures), token.text, ref left, ref right, token.pos);
                        left = GenerateSubtract(left, right);
                        break;
                }
                continue;
                Label_00C1:
                left = GenerateStringConcat(left, right);
            }
            return left;
        }

        private Expression ParseAggregate(Expression instance, Type elementType, string methodName, int errorPos)
        {
            Type[] typeArray;
            ParameterExpression it = this.it;
            ParameterExpression expression2 = Expression.Parameter(elementType, "");
            this.it = expression2;
            Expression[] args = this.ParseArgumentList();
            this.it = it;
            if (FindMethod(typeof(IEnumerableSignatures), methodName, false, args, out MethodBase base2) != 1)
            {
                throw ParseError(errorPos, "No applicable aggregate method '{0}' exists", new object[] { methodName });
            }
            if ((base2.Name == "Min") || (base2.Name == "Max"))
            {
                typeArray = new Type[] { elementType, args[0].Type };
            }
            else
            {
                typeArray = new Type[] { elementType };
            }
            if (args.Length == 0)
            {
                args = new Expression[] { instance };
            }
            else
            {
                Expression[] expressionArray2 = new Expression[2];
                expressionArray2[0] = instance;
                ParameterExpression[] parameters = new ParameterExpression[] { expression2 };
                expressionArray2[1] = Expression.Lambda(args[0], parameters);
                args = expressionArray2;
            }
            return Expression.Call(typeof(Enumerable), base2.Name, typeArray, args);
        }

        private Expression[] ParseArgumentList()
        {
            ValidateToken(TokenId.OpenParen, "'(' expected");
            NextToken();
            ValidateToken(TokenId.CloseParen, "')' or ',' expected");
            NextToken();
            return ((this.token.id != TokenId.CloseParen) ? ParseArguments() : new Expression[0]);
        }

        private Expression[] ParseArguments()
        {
            List<Expression> list = new List<Expression>();
            while (true)
            {
                list.Add(ParseExpression());
                if (token.id != TokenId.Comma)
                {
                    break;
                }
                NextToken();
            }
            return list.ToArray();
        }

        private Expression ParseComparison()
        {
            Expression left = ParseAdditive();
            while ((((this.token.id == TokenId.Equal) || (this.token.id == TokenId.DoubleEqual)) || ((this.token.id == TokenId.ExclamationEqual) || (this.token.id == TokenId.LessGreater))) || (((this.token.id == TokenId.GreaterThan) || (this.token.id == TokenId.GreaterThanEqual)) || ((this.token.id == TokenId.LessThan) || (this.token.id == TokenId.LessThanEqual))))
            {
                Token token = this.token;
                NextToken();
                Expression right = ParseAdditive();
                bool flag = (((token.id == TokenId.Equal) || (token.id == TokenId.DoubleEqual)) || (token.id == TokenId.ExclamationEqual)) || (token.id == TokenId.LessGreater);
                if (flag && ((!left.Type.IsValueType && !right.Type.IsValueType) || ((left.Type == typeof(Guid)) && (right.Type == typeof(Guid)))))
                {
                    if (left.Type != right.Type)
                    {
                        if (!left.Type.IsAssignableFrom(right.Type))
                        {
                            if (!right.Type.IsAssignableFrom(left.Type))
                            {
                                throw IncompatibleOperandsError(token.text, left, right, token.pos);
                            }
                            left = Expression.Convert(left, right.Type);
                        }
                        else
                        {
                            right = Expression.Convert(right, left.Type);
                        }
                    }
                }
                else if (IsEnumType(left.Type) || IsEnumType(right.Type))
                {
                    if (left.Type != right.Type)
                    {
                        Expression expression3 = PromoteExpression(right, left.Type, true);
                        if (expression3 == null)
                        {
                            expression3 = PromoteExpression(left, right.Type, true);
                            left = expression3 ?? throw IncompatibleOperandsError(token.text, left, right, token.pos);
                        }
                        else
                        {
                            right = expression3;
                        }
                    }
                }
                else
                {
                    CheckAndPromoteOperands(flag ? typeof(IEqualitySignatures) : typeof(IRelationalSignatures), token.text, ref left, ref right, token.pos);
                }
                switch (token.id)
                {
                    case TokenId.LessThan:
                        left = GenerateLessThan(left, right);
                        break;

                    case TokenId.Equal:
                    case TokenId.DoubleEqual:
                        left = GenerateEqual(left, right);
                        break;

                    case TokenId.GreaterThan:
                        left = GenerateGreaterThan(left, right);
                        break;

                    case TokenId.ExclamationEqual:
                    case TokenId.LessGreater:
                        left = GenerateNotEqual(left, right);
                        break;

                    case TokenId.LessThanEqual:
                        left = GenerateLessThanEqual(left, right);
                        break;

                    case TokenId.GreaterThanEqual:
                        left = GenerateGreaterThanEqual(left, right);
                        break;
                }
            }
            return left;
        }

        private Expression ParseElementAccess(Expression expr)
        {
            int pos = token.pos;
           ValidateToken(TokenId.OpenBracket, "'(' expected");
           NextToken();
            Expression[] args = ParseArguments();
            ValidateToken(TokenId.CloseBracket, "']' or ',' expected");
            NextToken();
            if (expr.Type.IsArray)
            {
                if ((expr.Type.GetArrayRank() != 1) || (args.Length != 1))
                {
                    throw ParseError(pos, "Indexing of multi-dimensional arrays is not supported", new object[0]);
                }
                Expression index = PromoteExpression(args[0], typeof(int), true);
                if (index == null)
                {
                    throw ParseError(pos, "Array index must be an integer expression", new object[0]);
                }
                return Expression.ArrayIndex(expr, index);
            }
            switch (FindIndexer(expr.Type, args, out MethodBase base2))
            {
                case 0:
                    {
                        throw ParseError(pos, "No applicable indexer exists in type '{0}'", new object[] { GetTypeName(expr.Type) });
                    }
                case 1:
                    return Expression.Call(expr, (MethodInfo)base2, args);
            }
            throw ParseError(pos, "Ambiguous invocation of indexer in type '{0}'", new object[] { GetTypeName(expr.Type) });
        }

        private static object ParseEnum(string name, Type type)
        {
            if (type.IsEnum)
            {
                MemberInfo[] infoArray = type.FindMembers(MemberTypes.Field, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, Type.FilterNameIgnoreCase, name);
                if (infoArray.Length != 0)
                {
                    return ((FieldInfo)infoArray[0]).GetValue(null);
                }
            }
            return null;
        }

        private Exception ParseError(string format, params object[] args) => ParseError(this.token.pos, format, args);

        private Exception ParseError(int pos, string format, params object[] args) => new ParseException(string.Format(CultureInfo.CurrentCulture, format, args), pos);

        private Expression ParseExpression()
        {
            int pos = token.pos;
            Expression test = ParseLogicalOr();
            if (token.id == TokenId.Question)
            {
                NextToken();
                ValidateToken(TokenId.Colon, "':' expected");
                NextToken();
                test = GenerateConditional(test, ParseExpression(), ParseExpression(), pos);
            }
            return test;
        }

        private Expression ParseIdentifier()
        {
            ValidateToken(TokenId.Identifier);
            if (keywords.TryGetValue(token.text, out object obj2))
            {
                if (obj2 is Type)
                {
                    return ParseTypeAccess((Type)obj2);
                }
                if (obj2 == keywordIt)
                {
                    return ParseIt();
                }
                if (obj2 == keywordIif)
                {
                    return ParseIif();
                }
                if (obj2 == keywordNew)
                {
                    return ParseNew();
                }
                NextToken();
                return (Expression)obj2;
            }
            if (symbols.TryGetValue(token.text, out obj2) || ((externals != null) && externals.TryGetValue(token.text, out obj2)))
            {
                Expression expression = obj2 as Expression;
                if (expression == null)
                {
                    expression = Expression.Constant(obj2);
                }
                else
                {
                    if (expression is LambdaExpression lambda)
                    {
                        return ParseLambdaInvocation(lambda);
                    }
                }
                NextToken();
                return expression;
            }
            if (it != null)
            {
                return ParseMemberAccess(null, it);
            }
            object[] args = new object[] { token.text };
            throw ParseError("Unknown identifier '{0}'", args);
        }

        private Expression ParseIif()
        {
            NextToken();
            if (ParseArgumentList().Length != 3)
            {
                throw ParseError(token.pos, "The 'iif' function requires three arguments", new object[0]);
            }
            return GenerateConditional(ParseArgumentList()[0], ParseArgumentList()[1], ParseArgumentList()[2], token.pos);
        }

        private Expression ParseIntegerLiteral()
        {
            ValidateToken(TokenId.IntegerLiteral);
            string text = token.text;
            if (text[0] != '-')
            {
                if (!ulong.TryParse(text, out ulong num))
                {
                    object[] args = new object[] { text };
                    throw this.ParseError("Invalid integer literal '{0}'", args);
                }
                NextToken();
                if (num <= 0x7fffffffL)
                {
                    return CreateLiteral((int)num, text);
                }
                if (num <= 0xffffffffL)
                {
                    return CreateLiteral((uint)num, text);
                }
                if (num <= 0x7fffffffffffffffL)
                {
                    return CreateLiteral((long)num, text);
                }
                return CreateLiteral(num, text);
            }
            if (!long.TryParse(text, out long num2))
            {
                throw ParseError("Invalid integer literal '{0}'", new object[] { text });
            }
            NextToken();
            if ((num2 >= -2147483648L) && (num2 <= 0x7fffffffL))
            {
                return CreateLiteral((int)num2, text);
            }
            return CreateLiteral(num2, text);
        }

        private Expression ParseIt()
        {
            if (it == null)
            {
                throw ParseError("No 'it' is in scope", new object[0]);
            }
            NextToken();
            return it;
        }

        private Expression ParseLambdaInvocation(LambdaExpression lambda)
        {
            NextToken();
            if (FindMethod(lambda.Type, "Invoke", false, ParseArgumentList(), out MethodBase base2) != 1)
            {
                throw ParseError(token.pos, "Argument list incompatible with lambda expression", new object[0]);
            }
            return Expression.Invoke(lambda, ParseArgumentList());
        }

        private Expression ParseLogicalAnd()
        {
            Expression left = ParseComparison();
            while ((this.token.id == TokenId.DoubleAmphersand) || TokenIdentifierIs("and"))
            {
                Token token = this.token;
                NextToken();
                Expression right = this.ParseComparison();
                CheckAndPromoteOperands(typeof(ILogicalSignatures), token.text, ref left, ref right, token.pos);
                left = Expression.AndAlso(left, right);
            }
            return left;
        }

        private Expression ParseLogicalOr()
        {
            Expression left = ParseLogicalAnd();
            while ((this.token.id == TokenId.DoubleBar) || this.TokenIdentifierIs("or"))
            {
                Token token = this.token;
                NextToken();
                Expression right = ParseLogicalAnd();
                CheckAndPromoteOperands(typeof(ILogicalSignatures), token.text, ref left, ref right, token.pos);
                left = Expression.OrElse(left, right);
            }
            return left;
        }

        private Expression ParseMemberAccess(Type type, Expression instance)
        {
            if (instance != null)
            {
                type = instance.Type;
            }
            int pos = token.pos;
            string identifier = this.GetIdentifier();
            NextToken();
            if (token.id == TokenId.OpenParen)
            {
                if ((instance != null) && (type != typeof(string)))
                {
                    Type type2 = FindGenericType(typeof(IEnumerable<>), type);
                    if (type2 != null)
                    {
                        return ParseAggregate(instance, type2.GetGenericArguments()[0], identifier, pos);
                    }
                }
                Expression[] args = ParseArgumentList();
                switch (FindMethod(type, identifier, instance == null, args, out MethodBase base2))
                {
                    case 0:
                        {
                            throw ParseError(pos, "No applicable method '{0}' exists in type '{1}'", new object[] { identifier, GetTypeName(type) });
                        }
                    case 1:
                        {
                            MethodInfo method = (MethodInfo)base2;
                            if (!IsPredefinedType(method.DeclaringType))
                            {
                                throw ParseError(pos, "Methods on type '{0}' are not accessible", new object[] { GetTypeName(method.DeclaringType) });
                            }
                            if (method.ReturnType == typeof(void))
                            {
                                throw ParseError(pos, "Method '{0}' in type '{1}' does not return a value", new object[] { identifier, GetTypeName(method.DeclaringType) });
                            }
                            return Expression.Call(instance, method, args);
                        }
                }
                throw ParseError(pos, "Ambiguous invocation of method '{0}' in type '{1}'", new object[] { identifier, GetTypeName(type) });
            }
            MemberInfo info2 = this.FindPropertyOrField(type, identifier, instance == null);
            if (info2 == null)
            {
                throw this.ParseError(pos, "No property or field '{0}' exists in type '{1}'", new object[] { identifier, GetTypeName(type) });
            }
            if (info2 is PropertyInfo)
            {
                return Expression.Property(instance, (PropertyInfo)info2);
            }
            return Expression.Field(instance, (FieldInfo)info2);
        }

        private Expression ParseMultiplicative()
        {
            Expression left = ParseUnary();
            while (((this.token.id == TokenId.Asterisk) || (this.token.id == TokenId.Slash)) || ((this.token.id == TokenId.Percent) || TokenIdentifierIs("mod")))
            {
                Token token = this.token;
                NextToken();
                Expression right = ParseUnary();
                CheckAndPromoteOperands(typeof(IArithmeticSignatures), token.text, ref left, ref right, token.pos);
                switch (token.id)
                {
                    case TokenId.Identifier:
                    case TokenId.Percent:
                        goto Label_0072;

                    case TokenId.Asterisk:
                        left = Expression.Multiply(left, right);
                        break;

                    case TokenId.Slash:
                        left = Expression.Divide(left, right);
                        break;
                }
                continue;
                Label_0072:
                left = Expression.Modulo(left, right);
            }
            return left;
        }

        private Expression ParseNew()
        {
            int num;
            string identifier;
            NextToken();
            ValidateToken(TokenId.OpenParen, "'(' expected");
            NextToken();
            List<DynamicProperty> properties = new List<DynamicProperty>();
            List<Expression> list2 = new List<Expression>();
            Label_0025:
            num = token.pos;
            Expression item = ParseExpression();
            if (TokenIdentifierIs("as"))
            {
                NextToken();
                identifier = GetIdentifier();
                NextToken();
            }
            else
            {
                MemberExpression expression2 = item as MemberExpression;
                if (expression2 == null)
                {
                    throw ParseError(num, "Expression is missing an 'as' clause", new object[0]);
                }
                identifier = expression2.Member.Name;
            }
            list2.Add(item);
            properties.Add(new DynamicProperty(identifier, item.Type));
            if (token.id == TokenId.Comma)
            {
                NextToken();
                goto Label_0025;
            }
            ValidateToken(TokenId.CloseParen, "')' or ',' expected");
            NextToken();
            Type type = DynamicExpression.CreateClass(properties);
            MemberBinding[] bindings = new MemberBinding[properties.Count];
            for (int i = 0; i < bindings.Length; i++)
            {
                bindings[i] = Expression.Bind(type.GetProperty(properties[i].Name), list2[i]);
            }
            return Expression.MemberInit(Expression.New(type), bindings);
        }

        private static object ParseNumber(string text, Type type)
        {
            switch (Type.GetTypeCode(GetNonNullableType(type)))
            {
                case TypeCode.SByte:
                    sbyte num;
                    if (!sbyte.TryParse(text, out num))
                    {
                        break;
                    }
                    return num;

                case TypeCode.Byte:
                    byte num2;
                    if (!byte.TryParse(text, out num2))
                    {
                        break;
                    }
                    return num2;

                case TypeCode.Int16:
                    short num3;
                    if (!short.TryParse(text, out num3))
                    {
                        break;
                    }
                    return num3;

                case TypeCode.UInt16:
                    ushort num4;
                    if (!ushort.TryParse(text, out num4))
                    {
                        break;
                    }
                    return num4;

                case TypeCode.Int32:
                    int num5;
                    if (!int.TryParse(text, out num5))
                    {
                        break;
                    }
                    return num5;

                case TypeCode.UInt32:
                    uint num6;
                    if (!uint.TryParse(text, out num6))
                    {
                        break;
                    }
                    return num6;

                case TypeCode.Int64:
                    long num7;
                    if (!long.TryParse(text, out num7))
                    {
                        break;
                    }
                    return num7;

                case TypeCode.UInt64:
                    ulong num8;
                    if (!ulong.TryParse(text, out num8))
                    {
                        break;
                    }
                    return num8;

                case TypeCode.Single:
                    float num9;
                    if (!float.TryParse(text, out num9))
                    {
                        break;
                    }
                    return num9;

                case TypeCode.Double:
                    double num10;
                    if (!double.TryParse(text, out num10))
                    {
                        break;
                    }
                    return num10;

                case TypeCode.Decimal:
                    decimal num11;
                    if (!decimal.TryParse(text, out num11))
                    {
                        break;
                    }
                    return num11;
            }
            return null;
        }

        public IEnumerable<DynamicOrdering> ParseOrdering()
        {
            Expression expression;
            List<DynamicOrdering> list = new List<DynamicOrdering>();
            Label_0006:
            expression = ParseExpression();
            bool flag = true;
            if (TokenIdentifierIs("asc") || TokenIdentifierIs("ascending"))
            {
                NextToken();
            }
            else if (TokenIdentifierIs("desc") || TokenIdentifierIs("descending"))
            {
                NextToken();
                flag = false;
            }
            DynamicOrdering item = new DynamicOrdering
            {
                Selector = expression,
                Ascending = flag
            };
            list.Add(item);
            if (token.id == TokenId.Comma)
            {
                NextToken();
                goto Label_0006;
            }
            ValidateToken(TokenId.End, "Syntax error");
            return list;
        }

        private Expression ParseParenExpression()
        {
            ValidateToken(TokenId.OpenParen, "'(' expected");
            NextToken();
            ValidateToken(TokenId.CloseParen, "')' or operator expected");
            NextToken();
            return ParseExpression();
        }

        private Expression ParsePrimary()
        {
            Expression instance = ParsePrimaryStart();
            Label_0007:
            while (token.id == TokenId.Dot)
            {
                NextToken();
                instance = ParseMemberAccess(null, instance);
            }
            if (this.token.id == TokenId.OpenBracket)
            {
                instance = ParseElementAccess(instance);
                goto Label_0007;
            }
            return instance;
        }

        private Expression ParsePrimaryStart()
        {
            switch (token.id)
            {
                case TokenId.Identifier:
                    return ParseIdentifier();

                case TokenId.StringLiteral:
                    return ParseStringLiteral();

                case TokenId.IntegerLiteral:
                    return ParseIntegerLiteral();

                case TokenId.RealLiteral:
                    return ParseRealLiteral();

                case TokenId.OpenParen:
                    return ParseParenExpression();
            }
            throw ParseError("Expression expected", new object[0]);
        }

        private Expression ParseRealLiteral()
        {
            ValidateToken(TokenId.RealLiteral);
            string text = token.text;
            object obj2 = null;
            switch (text[text.Length - 1])
            {
                case 'F':
                case 'f':
                    float num;
                    if (float.TryParse(text.Substring(0, text.Length - 1), out num))
                    {
                        obj2 = num;
                    }
                    break;

                default:
                    double num2;
                    if (double.TryParse(text, out num2))
                    {
                        obj2 = num2;
                    }
                    break;
            }
            if (obj2 == null)
            {
                object[] args = new object[] { text };
                throw ParseError("Invalid real literal '{0}'", args);
            }
            this.NextToken();
            return CreateLiteral(obj2, text);
        }

        private Expression ParseStringLiteral()
        {
            ValidateToken(TokenId.StringLiteral);
            char ch = token.text[0];
            string text = token.text.Substring(1, token.text.Length - 2);
            int startIndex = 0;
            while (true)
            {
                int index = text.IndexOf(ch, startIndex);
                if (index < 0)
                {
                    break;
                }
                text = text.Remove(index, 1);
                startIndex = index + 1;
            }
            if (ch == '\'')
            {
                if (text.Length != 1)
                {
                    throw ParseError("Character literal must contain exactly one character", new object[0]);
                }
                NextToken();
                return CreateLiteral(text[0], text);
            }
            NextToken();
            return this.CreateLiteral(text, text);
        }

        private Expression ParseTypeAccess(Type type)
        {
            int pos = token.pos;
            this.NextToken();
            if (token.id == TokenId.Question)
            {
                if (!type.IsValueType || IsNullableType(type))
                {
                    throw ParseError(pos, "Type '{0}' has no nullable form", new object[] { GetTypeName(type) });
                }
                type = typeof(Nullable<>).MakeGenericType(new Type[] { type });
                NextToken();
            }
            if (token.id == TokenId.OpenParen)
            {
                Expression[] expressionArray = ParseArgumentList();
                switch (FindBestMethod(type.GetConstructors(), expressionArray, out MethodBase base2))
                {
                    case 0:
                        {
                            if (expressionArray.Length == 1)
                            {
                                return GenerateConversion(expressionArray[0], type, pos);
                            }
                            throw ParseError(pos, "No matching constructor in type '{0}'", new object[] { GetTypeName(type) });
                        }
                    case 1:
                        return Expression.New((ConstructorInfo)base2, expressionArray);
                }
                throw ParseError(pos, "Ambiguous invocation of '{0}' constructor", new object[] { GetTypeName(type) });
            }
            ValidateToken(TokenId.Dot, "'.' or '(' expected");
            NextToken();
            return ParseMemberAccess(type, null);
        }

        private Expression ParseUnary()
        {
            if (((this.token.id != TokenId.Minus) && (this.token.id != TokenId.Exclamation)) && !TokenIdentifierIs("not"))
            {
                return ParsePrimary();
            }
            Token token = this.token;
            NextToken();
            if ((token.id == TokenId.Minus) && ((this.token.id == TokenId.IntegerLiteral) || (this.token.id == TokenId.RealLiteral)))
            {
                this.token.text = "-" + this.token.text;
                this.token.pos = token.pos;
                return ParsePrimary();
            }
            Expression expr = this.ParseUnary();
            if (token.id == TokenId.Minus)
            {
                CheckAndPromoteOperand(typeof(INegationSignatures), token.text, ref expr, token.pos);
                return Expression.Negate(expr);
            }
            CheckAndPromoteOperand(typeof(INotSignatures), token.text, ref expr, token.pos);
            return Expression.Not(expr);
        }

        private void ProcessParameters(ParameterExpression[] parameters)
        {
            foreach (ParameterExpression expression in parameters)
            {
                if (!string.IsNullOrEmpty(expression.Name))
                {
                    AddSymbol(expression.Name, expression);
                }
            }
            if ((parameters.Length == 1) && string.IsNullOrEmpty(parameters[0].Name))
            {
                it = parameters[0];
            }
        }

        private void ProcessValues(object[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                object obj2 = values[i];
                if ((i == (values.Length - 1)) && (obj2 is IDictionary<string, object>))
                {
                    externals = (IDictionary<string, object>)obj2;
                }
                else
                {
                    AddSymbol("@" + i.ToString(CultureInfo.InvariantCulture), obj2);
                }
            }
        }

        private Expression PromoteExpression(Expression expr, Type type, bool exact)
        {
            if (expr.Type != type)
            {
                if (expr is ConstantExpression key)
                {
                    if (key == nullLiteral)
                    {
                        if (!type.IsValueType || IsNullableType(type))
                        {
                            return Expression.Constant(null, type);
                        }
                    }
                    else
                    {
                        if (literals.TryGetValue(key, out string str))
                        {
                            Type nonNullableType = GetNonNullableType(type);
                            object obj2 = null;
                            switch (Type.GetTypeCode(key.Type))
                            {
                                case TypeCode.Int32:
                                case TypeCode.UInt32:
                                case TypeCode.Int64:
                                case TypeCode.UInt64:
                                    obj2 = ParseNumber(str, nonNullableType);
                                    break;

                                case TypeCode.Double:
                                    if (nonNullableType == typeof(decimal))
                                    {
                                        obj2 = ParseNumber(str, nonNullableType);
                                    }
                                    break;

                                case TypeCode.String:
                                    obj2 = ParseEnum(str, nonNullableType);
                                    break;
                            }
                            if (obj2 != null)
                            {
                                return Expression.Constant(obj2, type);
                            }
                        }
                    }
                }
                if (!IsCompatibleWith(expr.Type, type))
                {
                    return null;
                }
                if (type.IsValueType | exact)
                {
                    return Expression.Convert(expr, type);
                }
            }
            return expr;
        }
        
        private static IEnumerable<Type> SelfAndBaseClasses(Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }

        private static IEnumerable<Type> SelfAndBaseTypes(Type type)
        {
            if (type.IsInterface)
            {
                List<Type> types = new List<Type>();
                AddInterface(types, type);
                return types;
            }
            return SelfAndBaseClasses(type);
        }

        private void SetTextPos(int pos)
        {
            textPos = pos;
            ch = (textPos < textLen) ? text[textPos] : '\0';
        }

        private bool TokenIdentifierIs(string id) => ((token.id == TokenId.Identifier) && string.Equals(id, token.text, StringComparison.OrdinalIgnoreCase));

        private void ValidateDigit()
        {
            if (!char.IsDigit(ch))
            {
                throw ParseError(textPos, "Digit expected", new object[0]);
            }
        }

        private void ValidateToken(TokenId t)
        {
            if (token.id != t)
            {
                throw ParseError("Syntax error", new object[0]);
            }
        }

        private void ValidateToken(TokenId t, string errorMessage)
        {
            if (token.id != t)
            {
                throw ParseError(errorMessage, new object[0]);
            }
        }
        

        private interface IAddSignatures : IArithmeticSignatures
        {
            void F(DateTime x, TimeSpan y);
            void F(DateTime? x, TimeSpan? y);
            void F(TimeSpan? x, TimeSpan? y);
            void F(TimeSpan x, TimeSpan y);
        }

        private interface IArithmeticSignatures
        {
            void F(decimal x, decimal y);
            void F(double x, double y);
            void F(int x, int y);
            void F(long x, long y);
            void F(decimal? x, decimal? y);
            void F(double? x, double? y);
            void F(int? x, int? y);
            void F(long? x, long? y);
            void F(float? x, float? y);
            void F(uint? x, uint? y);
            void F(ulong? x, ulong? y);
            void F(float x, float y);
            void F(uint x, uint y);
            void F(ulong x, ulong y);
        }

        private interface IEnumerableSignatures
        {
            void All(bool predicate);
            void Any();
            void Any(bool predicate);
            void Average(decimal selector);
            void Average(double selector);
            void Average(int selector);
            void Average(long selector);
            void Average(decimal? selector);
            void Average(double? selector);
            void Average(int? selector);
            void Average(long? selector);
            void Average(float? selector);
            void Average(float selector);
            void Count();
            void Count(bool predicate);
            void Max(object selector);
            void Min(object selector);
            void Sum(decimal selector);
            void Sum(double selector);
            void Sum(int selector);
            void Sum(long selector);
            void Sum(decimal? selector);
            void Sum(double? selector);
            void Sum(int? selector);
            void Sum(long? selector);
            void Sum(float? selector);
            void Sum(float selector);
            void Where(bool predicate);
        }

        private interface IEqualitySignatures : IRelationalSignatures, IArithmeticSignatures
        {
            void F(bool x, bool y);
            void F(bool? x, bool? y);
        }

        private interface ILogicalSignatures
        {
            void F(bool x, bool y);
            void F(bool? x, bool? y);
        }

        private interface INegationSignatures
        {
            void F(decimal x);
            void F(double x);
            void F(int x);
            void F(long x);
            void F(decimal? x);
            void F(double? x);
            void F(int? x);
            void F(long? x);
            void F(float? x);
            void F(float x);
        }

        private interface INotSignatures
        {
            void F(bool x);
            void F(bool? x);
        }

        private interface IRelationalSignatures : IArithmeticSignatures
        {
            void F(char x, char y);
            void F(DateTime x, DateTime y);
            void F(char? x, char? y);
            void F(DateTime? x, DateTime? y);
            void F(TimeSpan? x, TimeSpan? y);
            void F(string x, string y);
            void F(TimeSpan x, TimeSpan y);
        }

        private interface ISubtractSignatures : IAddSignatures, IArithmeticSignatures
        {
            
            void F(DateTime x, DateTime y);
            void F(DateTime? x, DateTime? y);
        }

        private class MethodData
        {
            
            public Expression[] Args;
            public MethodBase MethodBase;
            public ParameterInfo[] Parameters;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Token
        {
            public TokenId id;
            public string text;
            public int pos;
        }

        private enum TokenId
        {
            Unknown,
            End,
            Identifier,
            StringLiteral,
            IntegerLiteral,
            RealLiteral,
            Exclamation,
            Percent,
            Amphersand,
            OpenParen,
            CloseParen,
            Asterisk,
            Plus,
            Comma,
            Minus,
            Dot,
            Slash,
            Colon,
            LessThan,
            Equal,
            GreaterThan,
            Question,
            OpenBracket,
            CloseBracket,
            Bar,
            ExclamationEqual,
            DoubleAmphersand,
            LessThanEqual,
            LessGreater,
            DoubleEqual,
            GreaterThanEqual,
            DoubleBar
        }

    }
}
