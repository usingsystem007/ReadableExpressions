﻿namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Reflection;
    using Extensions;
    using Interfaces;
    using NetStandardPolyfills;
    using static MethodTranslationHelpers;

    /// <summary>
    /// An <see cref="ITranslatable"/> for a method definition, including accessibility, scope, generic
    /// arguments and method arguments.
    /// </summary>
    public class MethodDefinitionTranslation : ITranslatable
    {
        private readonly string _accessibility;
        private readonly string _modifiers;
        private readonly TypeNameTranslation _returnTypeTranslation;
        private readonly TypeNameTranslation _declaringTypeNameTranslation;
        private readonly string _methodName;
        private readonly ITranslatable[] _genericArgumentTranslations;
        private readonly int _genericArgumentCount;
        private readonly ITranslatable _parametersTranslation;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodDefinitionTranslation"/> class for
        /// the given <paramref name="method"/>.
        /// </summary>
        /// <param name="method">
        /// The <see cref="IMethod"/> for which to create the <see cref="MethodDefinitionTranslation"/>.
        /// </param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        public MethodDefinitionTranslation(
            IMethod method,
            TranslationSettings settings)
        {
            _accessibility = GetAccessibility(method);
            _modifiers = GetModifiers(method);

            _returnTypeTranslation =
                new TypeNameTranslation(method.ReturnType, settings);

            if (method.DeclaringType != null)
            {
                _declaringTypeNameTranslation =
                    new TypeNameTranslation(method.DeclaringType, settings);
            }

            _methodName = method.Name;

            if (method.IsGenericMethod)
            {
                TranslationSize += 2;

                var genericArguments = method.GetGenericArguments();
                _genericArgumentCount = genericArguments.Length;

                _genericArgumentTranslations = new ITranslatable[_genericArgumentCount];

                for (var i = 0; ;)
                {
                    var argumentTranslation = new TypeNameTranslation(genericArguments[i], settings);

                    TranslationSize += argumentTranslation.TranslationSize;
                    FormattingSize += argumentTranslation.FormattingSize;
                    _genericArgumentTranslations[i] = argumentTranslation;

                    ++i;

                    if (i == _genericArgumentCount)
                    {
                        break;
                    }

                    TranslationSize += ", ".Length;
                }
            }
            else
            {
                _genericArgumentTranslations = Enumerable<ITranslatable>.EmptyArray;
            }

            _parametersTranslation = new ParameterSetDefinitionTranslation(method, settings);

            TranslationSize =
                _accessibility.Length +
                _modifiers.Length +
                _returnTypeTranslation.TranslationSize +
                _methodName.Length;

            var keywordFormattingSize = settings.GetKeywordFormattingSize();

            FormattingSize =
                keywordFormattingSize + // <- For modifiers
                _returnTypeTranslation.FormattingSize;

            if (_declaringTypeNameTranslation != null)
            {
                TranslationSize += _declaringTypeNameTranslation.TranslationSize + 1;
                FormattingSize += _declaringTypeNameTranslation.FormattingSize;
            }
        }

        /// <summary>
        /// Creates an <see cref="ITranslatable"/> for the given <paramref name="method"/>, handling
        /// properties and operators as well as regular methods.
        /// </summary>
        /// <param name="method">The MethodInfo for which to create the <see cref="ITranslatable"/>.</param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        /// <returns>An <see cref="ITranslatable"/> for the given <paramref name="method"/>.</returns>
        public static ITranslatable For(MethodInfo method, TranslationSettings settings)
        {
            if (method.IsPropertyGetterOrSetterCall(out var property))
            {
                return new PropertyDefinitionTranslation(property, method, settings);
            }

            if (method.IsImplicitOperator())
            {
                return new OperatorDefinitionTranslation(method, "implicit", settings);
            }

            if (method.IsExplicitOperator())
            {
                return new OperatorDefinitionTranslation(method, "explicit", settings);
            }

            return new MethodDefinitionTranslation(new BclMethodWrapper(method), settings);
        }

        /// <inheritdoc />
        public int TranslationSize { get; }

        /// <inheritdoc />
        public int FormattingSize { get; }

        /// <inheritdoc />
        public int GetIndentSize() => _parametersTranslation.GetIndentSize();

        /// <inheritdoc />
        public int GetLineCount() => _parametersTranslation.GetLineCount() + 1;

        /// <inheritdoc />
        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteKeywordToTranslation(_accessibility + _modifiers);

            _returnTypeTranslation.WriteTo(writer);
            writer.WriteSpaceToTranslation();

            if (_declaringTypeNameTranslation != null)
            {
                _declaringTypeNameTranslation.WriteTo(writer);
                writer.WriteDotToTranslation();
            }

            writer.WriteToTranslation(_methodName);

            if (_genericArgumentCount != 0)
            {
                writer.WriteToTranslation('<');

                for (var i = 0; ;)
                {
                    _genericArgumentTranslations[i].WriteTo(writer);

                    ++i;

                    if (i == _genericArgumentCount)
                    {
                        break;
                    }

                    writer.WriteToTranslation(", ");
                }

                writer.WriteToTranslation('>');
            }

            _parametersTranslation.WriteTo(writer);
        }
    }
}