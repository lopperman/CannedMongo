using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CS.Common.Interfaces;

namespace CS.Common.Extensions
{
    public class BuildPropNames<T> : IBuildPropNames<T>
    {
        private List<string> _propertyPropertyNames = new List<string>();

        public void Clear()
        {
            _propertyPropertyNames.Clear();
        }

        //TODO:  Paul/Daniel using this, don't delete.  Look at a single method to take multiple lambda expression props
        public IBuildPropNames<T> Add<PN>(Expression<Func<T, PN>> expression)
        {
            MemberExpression memberExpression = expression.Body as MemberExpression;

            if (!_propertyPropertyNames.Contains(Resolve(memberExpression)))
            {
                _propertyPropertyNames.Add(Resolve(memberExpression));
            }

            return this;
        }

        public string AddSingle<PN>(Expression<Func<T, PN>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            return Resolve(memberExpression);
        }

        public IEnumerable<string> PropertyNames
        {
            get { return _propertyPropertyNames; }
        }

        public string[] PropertyNamesArray
        {
            get { return PropertyNames.ToArray(); }
        }

        private static string Resolve(Expression expression)
        {
            MemberExpression memberExpression = expression as MemberExpression;
            if (memberExpression != null)
            {
                var part = Resolve(memberExpression.Expression);
                if (part != string.Empty)
                {
                    return string.Concat(part, ".", memberExpression.Member.Name);
                }
                else
                {
                    return memberExpression.Member.Name;
                }
            }
            MethodCallExpression methodCallExpression = expression as MethodCallExpression;
            if (methodCallExpression != null && methodCallExpression.Method.DeclaringType == typeof(System.Linq.Enumerable) && methodCallExpression.Method.Name == "Single")
            {
                return Resolve(methodCallExpression.Arguments[0]);
            }
            else
            {
                return String.Empty;
            }

        }
    }
}
