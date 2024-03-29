﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using zivillian.ldap;

namespace eventphone.guru3.ldap
{
    public abstract class LdapFilterDBVisitor : LdapFilterVisitor
    {
        protected static readonly MethodInfo LikeFunction = typeof(DbFunctionsExtensions).GetMethod(nameof(DbFunctionsExtensions.Like),
            new[] {typeof(DbFunctions), typeof(string), typeof(string)});

        protected static readonly MethodInfo IsNullOrEmptyFunction = typeof(String).GetMethod(nameof(String.IsNullOrEmpty));

        protected static readonly MethodInfo _stringCompare = typeof(string).GetRuntimeMethod(nameof(string.Compare), new[] { typeof(string), typeof(string) });
    }

    public abstract class LdapExpressionFilterVisitor<T> : LdapFilterDBVisitor
    {
        public Expression<Func<T, bool>> Filter { get; private set; }

        private readonly Stack<Expression> _inner = new Stack<Expression>();
        protected readonly ParameterExpression ExpressionParameter;

        public LdapExpressionFilterVisitor()
        {
            ExpressionParameter = Expression.Parameter(typeof(T), "x");
        }

        public override void Visit(LdapFilter filter)
        {
            base.Visit(filter);
            Filter = Expression.Lambda<Func<T, bool>>(_inner.Peek(), ExpressionParameter);
        }

        protected override void VisitAnd(LdapAndFilter filter)
        {
            var size = _inner.Count + 1;
            base.VisitAnd(filter);
            while (_inner.Count > size)
            {
                _inner.Push(Expression.AndAlso(_inner.Pop(), _inner.Pop()));
            }
        }

        protected override void VisitOr(LdapOrFilter filter)
        {
            var size = _inner.Count + 1;
            base.VisitOr(filter);
            while (_inner.Count > size)
            {
                _inner.Push(Expression.OrElse(_inner.Pop(), _inner.Pop()));
            }
        }

        protected override void VisitNot(LdapNotFilter filter)
        {
            base.VisitNot(filter);
            _inner.Push(Expression.Not(_inner.Pop()));
        }

        protected override void VisitApproxMatch(LdapApproxMatchFilter filter)
        {
            base.VisitApproxMatch(filter);
            _inner.Push(Expression.Constant(false));
        }

        protected override void VisitEquality(LdapEqualityFilter filter)
        {
            base.VisitEquality(filter);
            if (filter.Assertion.Attribute.Options.Count > 0)
            {
                _inner.Push(Expression.Constant(false));
                return;
            }

            var attribute = filter.Assertion.Attribute.Oid;
            var property = GetProperty(attribute);
            if (property == null)
            {
                _inner.Push(Expression.Constant(false));
                return;
            }
            
            var value = Encoding.UTF8.GetString(filter.Assertion.Value.Span);
            _inner.Push(Expression.Equal(property, Expression.Constant(value)));
        }

        protected override void VisitExtensibleMatch(LdapExtensibleMatchFilter filter)
        {
            base.VisitExtensibleMatch(filter);
            _inner.Push(Expression.Constant(false));
        }

        protected override void VisitGreaterOrEqual(LdapGreaterOrEqualFilter filter)
        {
            base.VisitGreaterOrEqual(filter);
            if (filter.Assertion.Attribute.Options.Count > 0)
            {
                _inner.Push(Expression.Constant(false));
                return;
            }

            var attribute = filter.Assertion.Attribute.Oid;
            var property = GetProperty(attribute);
            if (property == null)
            {
                _inner.Push(Expression.Constant(false));
                return;
            }
            
            var value = Encoding.UTF8.GetString(filter.Assertion.Value.Span);
            if (!property.Type.IsAssignableFrom(typeof(string)))
            {
                throw new NotImplementedException("we have only string properties");
            }
            var compare = Expression.Call(null, _stringCompare, property, Expression.Constant(value));
            _inner.Push(Expression.GreaterThanOrEqual(compare, Expression.Constant(0)));
        }

        protected override void VisitLessOrEqual(LdapLessOrEqualFilter filter)
        {
            base.VisitLessOrEqual(filter);
            if (filter.Assertion.Attribute.Options.Count > 0)
            {
                _inner.Push(Expression.Constant(false));
                return;
            }
            var attribute = filter.Assertion.Attribute.Oid;
            var property = GetProperty(attribute);
            if (property == null)
            {
                _inner.Push(Expression.Constant(false));
                return;
            }
            
            var value = Encoding.UTF8.GetString(filter.Assertion.Value.Span);
            if (!property.Type.IsAssignableFrom(typeof(string)))
            {
                throw new NotImplementedException("we have only string properties");
            }
            var compare = Expression.Call(null, _stringCompare, property, Expression.Constant(value));
            _inner.Push(Expression.LessThanOrEqual(compare, Expression.Constant(0)));
        }

        protected override void VisitPresent(LdapPresentFilter filter)
        {
            if (filter.Attribute.Options.Count > 0)
            {
                _inner.Push(Expression.Constant(false));
            }
            var attribute = filter.Attribute.Oid.ToLowerInvariant();
            switch (attribute)
            {
                case "objectclass":
                case "2.5.4.0":
                    _inner.Push(Expression.Constant(true));
                    break;
                default:
                    var property = GetProperty(attribute);
                    if (property == null)
                    {
                        _inner.Push(Expression.Constant(false));
                    }
                    else
                    {
                        _inner.Push(Expression.Not(Expression.Call(null, IsNullOrEmptyFunction, property)));
                    }
                    break;
            }
        }

        protected override void VisitSubstring(LdapSubstringFilter filter)
        {
            base.VisitSubstring(filter);
            if (filter.Attribute.Options.Count > 0)
            {
                _inner.Push(Expression.Constant(false));
                return;
            }

            var property = GetProperty(filter.Attribute.Oid);
            if (property == null)
            {
                _inner.Push(Expression.Constant(false));
                return;
            }
            var like = new StringBuilder();
            if (filter.StartsWith != null)
            {
                var value = Encoding.UTF8.GetString(filter.StartsWith.Value.Span);
                like.Append(value);
            }
            like.Append('%');
            if (filter.Contains != null)
            {
                foreach (var contains in filter.Contains)
                {
                    var value = Encoding.UTF8.GetString(contains.Span);
                    like.Append(value).Append('%');
                }
            }
            if (filter.EndsWith != null)
            {
                var value = Encoding.UTF8.GetString(filter.EndsWith.Value.Span);
                like.Append(value);
            }

            _inner.Push(Expression.Call(null, LikeFunction, Expression.Constant(null, typeof(DbFunctions)), property, Expression.Constant(like.ToString())));
        }

        protected abstract MemberExpression GetProperty(string name);
    }
}