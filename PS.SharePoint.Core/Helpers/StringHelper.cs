using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PS.SharePoint.Core.Helpers
{
    internal class StringHelper
    {
        /// <summary>
        /// Returns the name of the property.
        /// When used like NameOf(x => x.PropName) return "PropName"
        /// </summary>
        public static string NameOf<TModel, TProperty>(Expression<Func<TModel, TProperty>> lambda)
        {
            var body = lambda.Body;
            var unaryExpression = body as UnaryExpression;
            var expression = (unaryExpression != null) ? unaryExpression.Operand as MemberExpression : body as MemberExpression;

            return expression == null ? null : expression.Member.Name;
        }
    }
}
