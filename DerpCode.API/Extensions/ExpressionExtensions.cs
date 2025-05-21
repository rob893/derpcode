using System;
using System.Linq.Expressions;

namespace DerpCode.API.Extensions;

/// <summary>
/// Extension methods for working with Expression trees.
/// </summary>
public static class ExpressionExtensions
{
    /// <summary>
    /// Composes two lambda expressions by replacing the parameter of the inner expression with the body of the outer expression.
    /// </summary>
    /// <typeparam name="TOuter">The input type of the outer expression.</typeparam>
    /// <typeparam name="TInner">The output type of the outer expression and input type of the inner expression.</typeparam>
    /// <typeparam name="TResult">The output type of the inner expression.</typeparam>
    /// <param name="outer">The outer expression to compose.</param>
    /// <param name="inner">The inner expression to compose.</param>
    /// <returns>A new lambda expression that maps directly from TOuter to TResult.</returns>
    /// <exception cref="ArgumentNullException">Thrown when outer or inner is null.</exception>
    public static Expression<Func<TOuter, TResult>> Apply<TOuter, TInner, TResult>(this Expression<Func<TOuter, TInner>> outer, Expression<Func<TInner, TResult>> inner)
    {
        ArgumentNullException.ThrowIfNull(outer);
        ArgumentNullException.ThrowIfNull(inner);

        return Expression.Lambda<Func<TOuter, TResult>>(inner.Body.ReplaceParameter(inner.Parameters[0], outer.Body), outer.Parameters);
    }

    /// <summary>
    /// Provides a more fluent API for composing lambda expressions.
    /// </summary>
    /// <typeparam name="TInner">The input type of the inner expression.</typeparam>
    /// <typeparam name="TResult">The output type of the inner expression.</typeparam>
    /// <typeparam name="TOuter">The input type of the outer expression.</typeparam>
    /// <param name="inner">The inner expression to compose.</param>
    /// <param name="outer">The outer expression to compose.</param>
    /// <returns>A new lambda expression that maps directly from TOuter to TResult.</returns>
    public static Expression<Func<TOuter, TResult>> ApplyTo<TInner, TResult, TOuter>(this Expression<Func<TInner, TResult>> inner, Expression<Func<TOuter, TInner>> outer)
    {
        return outer.Apply(inner);
    }

    /// <summary>
    /// Replaces a parameter in an expression with another expression.
    /// </summary>
    /// <param name="expression">The expression to modify.</param>
    /// <param name="source">The parameter to replace.</param>
    /// <param name="target">The expression to replace the parameter with.</param>
    /// <returns>A new expression with the parameter replaced.</returns>
    public static Expression ReplaceParameter(this Expression expression, ParameterExpression source, Expression target)
    {
        return new ParameterReplacer(source, target).Visit(expression);
    }

    private sealed class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression source;

        private readonly Expression target;

        public ParameterReplacer(ParameterExpression source, Expression target)
        {
            this.source = source;
            this.target = target;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == this.source ? this.target : node;
        }
    }
}