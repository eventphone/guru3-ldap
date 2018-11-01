using System.Linq.Expressions;

namespace eventphone.guru3.ldap
{
    public class LdapExtensionFilterVisitor : LdapExpressionFilterVisitor<LdapExtension>
    {
        protected override MemberExpression GetProperty(string name)
        {
            switch (name.ToLowerInvariant())
            {
                case "cn":
                case "2.5.4.3":
                case "telephonenumber":
                case "2.5.4.20":
                    return Expression.Property(ExpressionParameter, typeof(LdapExtension).GetProperty(nameof(LdapExtension.Number)));
                case "sn":
                case "2.5.4.4":
                    return Expression.Property(ExpressionParameter, typeof(LdapExtension).GetProperty(nameof(LdapExtension.Name)));
                case "l":
                case "2.5.4.7":
                    return Expression.Property(ExpressionParameter, typeof(LdapExtension).GetProperty(nameof(LdapExtension.Location)));
                default:
                    return null;
            }
        }
    }

    public class LdapEventFilterVisitor : LdapExpressionFilterVisitor<LdapEvent>
    {
        protected override MemberExpression GetProperty(string name)
        {
            switch (name.ToLowerInvariant())
            {
                case "ou":
                case "2.5.4.11":
                    return Expression.Property(ExpressionParameter, typeof(LdapEvent).GetProperty(nameof(LdapEvent.Name)));
                case "description":
                case "2.5.4.13":
                    return Expression.Property(ExpressionParameter, typeof(LdapEvent).GetProperty(nameof(LdapEvent.Description)));
                case "l":
                case "2.5.4.7":
                    return Expression.Property(ExpressionParameter, typeof(LdapEvent).GetProperty(nameof(LdapEvent.Location)));
                default:
                    return null;
            }
        }
    }
}