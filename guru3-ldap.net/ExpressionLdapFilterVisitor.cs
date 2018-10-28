﻿using System.Linq.Expressions;
using zivillian.ldap.Attributes;

namespace eventphone.guru3.ldap
{
    public class LdapExtensionFilterVisitor : LdapExpressionFilterVisitor<LdapExtension>
    {
        protected override MemberExpression GetProperty(string name)
        {
            switch (name.ToLowerInvariant())
            {
                case"cn":
                    return Expression.Property(ExpressionParameter, typeof(LdapExtension).GetProperty(nameof(LdapExtension.Number)));
                case"sn":
                    return Expression.Property(ExpressionParameter, typeof(LdapExtension).GetProperty(nameof(LdapExtension.Name)));
                case"l":
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