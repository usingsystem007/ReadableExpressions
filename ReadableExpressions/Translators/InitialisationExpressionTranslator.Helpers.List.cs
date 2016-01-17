﻿namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal partial class InitialisationExpressionTranslator
    {
        private class ListInitExpressionHelper : InitExpressionHelperBase<ListInitExpression, NewExpression>
        {
            protected override NewExpression GetNewExpression(ListInitExpression expression)
            {
                return expression.NewExpression;
            }

            protected override bool ConstructorIsParameterless(NewExpression newExpression)
            {
                return !newExpression.Arguments.Any();
            }

            protected override IEnumerable<string> GetInitialisations(
                ListInitExpression expression,
                IExpressionTranslatorRegistry translatorRegistry)
            {
                return expression.Initializers
                    .Select(initialisation =>
                    {
                        var listAddCall = MethodCallExpressionTranslator.GetMethodCall(
                            initialisation.AddMethod,
                            initialisation.Arguments,
                            translatorRegistry);

                        return listAddCall;
                    });
            }
        }
    }
}