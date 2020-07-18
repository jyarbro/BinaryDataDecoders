﻿using BinaryDataDecoders.ExpressionCalculator.Evaluators;
using BinaryDataDecoders.ExpressionCalculator.Optimizers;
using BinaryDataDecoders.ExpressionCalculator.Parser;
using BinaryDataDecoders.ExpressionCalculator.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace BinaryDataDecoders.ExpressionCalculator.Expressions
{
    public static class ExpressionBaseExtensions
    {
        public static ExpressionBase<T> Optimize<T>(this ExpressionBase<T> expression)
            where T : struct, IComparable<T>, IEquatable<T> =>
                new ExpressionOptimizationProvider<T>().Optimize(expression);

        public static IDictionary<string, T> EmptySet<T>()
            where T : struct, IComparable<T>, IEquatable<T> =>
                new Dictionary<string, T>();

        public static IEnumerable<ExpressionBase<T>> GetAllExpressions<T>(this ExpressionBase<T> expression)
            where T : struct, IComparable<T>, IEquatable<T>
        {
            yield return expression;

            if (expression is InnerExpression<T> inner)
            {
                var subs = GetAllExpressions(inner.Expression);
                foreach (var sub in subs)
                    yield return sub;
            }
            else if (expression is UnaryOperatorExpression<T> unary)
            {
                var subs = GetAllExpressions(unary.Operand);
                foreach (var sub in subs)
                    yield return sub;
            }
            else if (expression is BinaryOperatorExpression<T> binary)
            {
                var subs = GetAllExpressions(binary.Left).Concat(
                           GetAllExpressions(binary.Right)
                           );
                foreach (var sub in subs)
                    yield return sub;
            }
        }

        public static IEnumerable<string> GetDistinctVariableNames<T>(this ExpressionBase<T> expression)
            where T : struct, IComparable<T>, IEquatable<T> =>
            expression.GetAllExpressions()
                      .OfType<VariableExpression<T>>()
                      .Select(s => s.Name)
                      .Distinct();

        //
        public static IDictionary<string, T> GenerateTestValues<T>(this ExpressionBase<T> expression, int scale = 4, bool includeNegatives = false, int places = 2)
            where T : struct, IComparable<T>, IEquatable<T>
        {
            var evaluator = ExpressionEvaluatorFactory.Create<T>();

            var variableNames = expression.GetDistinctVariableNames();
            var rand = new Random();

            var variables = new Dictionary<string, T>();
            foreach (var variableName in variableNames)
            {
                var randomValue = Math.Round(rand.NextDouble() * Math.Pow(10, scale) * (includeNegatives && rand.Next() % 2 == 0 ? -1 : 1), places);
                var value = evaluator.GetValue(randomValue);
                variables.Add(variableName, value);
            }
            return variables;
        }

        public static ExpressionBase<T> ParseAsExpression<T>(this string input)
            where T : struct, IComparable<T>, IEquatable<T> =>
            new ExpressionParser<T>().Parse(input);

        public static ExpressionBase<T> ReplaceVariables<T>(this ExpressionBase<T> expression, IEnumerable<(string input, string output)> variables)
            where T : struct, IComparable<T>, IEquatable<T> =>
            new ExpressionVariableReplacementVistor<T>().Process(expression, variables);
    }
}
