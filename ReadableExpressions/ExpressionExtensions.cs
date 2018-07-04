﻿namespace AgileObjects.ReadableExpressions
{
    using System;
    using System.Linq.Expressions;
    using Translators;

    /// <summary>
    /// Provides the Expression translation extension method.
    /// </summary>
    public static class ExpressionExtensions
    {
        private static readonly ExpressionTranslatorRegistry _translatorRegistry = new ExpressionTranslatorRegistry();

        /// <summary>
        /// Translates the given <paramref name="expression"/> to source-code string.
        /// </summary>
        /// <param name="expression">The Expression to translate.</param>
        /// <param name="configuration">The configuration to use for the translation, if required.</param>
        /// <returns>The translated <paramref name="expression"/>.</returns>
        public static string ToReadableString(
            this Expression expression,
            Func<TranslationSettings, TranslationSettings> configuration = null)
        {
            #if NET35
            var linqExpression = LinqExpressionToDlrExpressionConverter.Convert(expression);
            #else
            var linqExpression = expression;
            #endif

            return _translatorRegistry
                .Translate(linqExpression, configuration)?
                .WithoutUnindents();
        }
    }
}
