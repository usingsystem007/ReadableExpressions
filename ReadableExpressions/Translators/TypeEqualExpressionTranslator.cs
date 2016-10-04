namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;

    internal class TypeEqualExpressionTranslator : ExpressionTranslatorBase
    {
        private static readonly MethodInfo _reduceTypeEqualMethod =
            typeof(TypeBinaryExpression)
                .GetNonPublicInstanceMethods()
                .FirstOrDefault(m => m.Name == "ReduceTypeEqual");

        public TypeEqualExpressionTranslator()
            : base(ExpressionType.TypeEqual)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            if (_reduceTypeEqualMethod == null)
            {
                return FallbackTranslation(expression, context);
            }

            var reducedTypeEqualExpression = (Expression)_reduceTypeEqualMethod.Invoke(expression, null);
            var translated = context.Translate(reducedTypeEqualExpression).Unterminated();

            return translated;
        }

        private static string FallbackTranslation(Expression expression, TranslationContext context)
        {
            var typeBinary = (TypeBinaryExpression)expression;
            var operand = context.Translate(typeBinary.Expression);

            return $"({operand} TypeOf {typeBinary.TypeOperand.GetFriendlyName()})";
        }
    }
}