/* ------------------------------
   Source: Light.GuardClauses 15.1.0, commit f8581a1ac6abf6aee3b656061e1254a523e00571
   Light.GuardClauses 15.1.0
   ------------------------------

License information for Light.GuardClauses

The MIT License (MIT)
Copyright (c) 2016, 2026 Kenny Pflug mailto:kenny.pflug@live.de

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using JetBrains.Annotations;
using BrilliantMessaging.GuardClauses.Exceptions;
using BrilliantMessaging.GuardClauses.ExceptionFactory;
using BrilliantMessaging.GuardClauses.FrameworkExtensions;
using NotNullAttribute = System.Diagnostics.CodeAnalysis.NotNullAttribute;

#nullable enable annotations
namespace BrilliantMessaging.GuardClauses
{
    /// <summary>
    /// The <see cref = "Check"/> class provides access to all assertions of Light.GuardClauses.
    /// </summary>
    // ReSharper disable once RedundantTypeDeclarationBody -- required for Source Code Transformation
    public static class Check
    {
        /// <summary>
        /// Checks if the specified strings are equal, using the given comparison rules.
        /// </summary>
        /// <param name = "string">The first string to compare.</param>
        /// <param name = "value">The second string to compare.</param>
        /// <param name = "comparisonType">One of the enumeration values that specifies the rules for the comparison.</param>
        /// <returns>True if the two strings are considered equal, else false.</returns>
        /// <exception cref = "ArgumentException">Thrown when <paramref name = "comparisonType"/> is no valid enum value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals(this string? @string, string? value, StringComparisonType comparisonType)
        {
            if ((int)comparisonType < 6)
            {
                return string.Equals(@string, value, (StringComparison)comparisonType);
            }

            switch (comparisonType)
            {
                case StringComparisonType.OrdinalIgnoreWhiteSpace:
                    return @string.EqualsOrdinalIgnoreWhiteSpace(value);
                case StringComparisonType.OrdinalIgnoreCaseIgnoreWhiteSpace:
                    return @string.EqualsOrdinalIgnoreCaseIgnoreWhiteSpace(value);
                default:
                    Throw.EnumValueNotDefined(comparisonType, nameof(comparisonType));
                    return false;
            }
        }

        /// <summary>
        /// Checks if the specified <paramref name = "condition"/> is true and throws an <see cref = "ArgumentException"/> in this case.
        /// </summary>
        /// <param name = "condition">The condition to be checked. The exception is thrown when it is true.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the <see cref = "ArgumentException"/> (optional).</param>
        /// <exception cref = "ArgumentException">Thrown when <paramref name = "condition"/> is true.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InvalidArgument(bool condition, string? parameterName = null, string? message = null)
        {
            if (condition)
            {
                Throw.Argument(parameterName, message);
            }
        }

        /// <summary>
        /// Checks if the specified <paramref name = "condition"/> is true and throws an <see cref = "InvalidOperationException"/> in this case.
        /// </summary>
        /// <param name = "condition">The condition to be checked. The exception is thrown when it is true.</param>
        /// <param name = "message">The message that will be passed to the <see cref = "InvalidOperationException"/> (optional).</param>
        /// <exception cref = "InvalidOperationException">Thrown when <paramref name = "condition"/> is true.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InvalidOperation(bool condition, string? message = null)
        {
            if (condition)
            {
                Throw.InvalidOperation(message);
            }
        }

        /// <summary>
        /// Checks if the specified collection is null or empty.
        /// </summary>
        /// <param name = "collection">The collection to be checked.</param>
        /// <returns>True if the collection is null or empty, else false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty([NotNullWhen(false)] this IEnumerable? collection) => collection is null || collection.Count() == 0;
        /// <summary>
        /// Checks if the specified string is null or empty.
        /// </summary>
        /// <param name = "string">The string to be checked.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty([NotNullWhen(false)] this string? @string) => string.IsNullOrEmpty(@string);
        /// <summary>
        /// Checks if the specified string is null, empty, or contains only white space.
        /// </summary>
        /// <param name = "string">The string to be checked.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? @string) => string.IsNullOrWhiteSpace(@string);
        /// <summary>
        /// Checks if the specified character is a white space character.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteSpace(this char character) => char.IsWhiteSpace(character);
        /// <summary>
        /// Ensures that values of <paramref name = "parameter"/> can be assigned to variables of
        /// <paramref name = "requiredType"/>, or otherwise throws an <see cref = "ArgumentException"/>.
        /// </summary>
        /// <param name = "parameter">The candidate type to be checked.</param>
        /// <param name = "requiredType">The type to which values of the candidate type must be assignable.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <returns>The original candidate type.</returns>
        /// <exception cref = "ArgumentException">
        /// Thrown when values of <paramref name = "parameter"/> cannot be assigned to variables of
        /// <paramref name = "requiredType"/>.
        /// </exception>
        /// <exception cref = "ArgumentNullException">
        /// Thrown when <paramref name = "parameter"/> or <paramref name = "requiredType"/> is null.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type MustBeAssignableTo([NotNull] this Type? parameter, [NotNull] Type? requiredType, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            parameter.MustNotBeNull(parameterName, message);
            requiredType.MustNotBeNull(nameof(requiredType), message);
            if (!requiredType.IsAssignableFrom(parameter))
            {
                Throw.MustBeAssignableTo(parameter, requiredType, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that <paramref name = "parameter"/> is a non-abstract class, or otherwise throws an
        /// <see cref = "ArgumentException"/>.
        /// </summary>
        /// <param name = "parameter">The type to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <returns>The original type.</returns>
        /// <exception cref = "ArgumentException">
        /// Thrown when <paramref name = "parameter"/> is not a class or is abstract.
        /// </exception>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "parameter"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type MustBeConcreteClass([NotNull] this Type? parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            parameter.MustNotBeNull(parameterName, message);
            if (!(parameter.IsClass && !parameter.IsAbstract))
            {
                Throw.MustBeConcreteClass(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is not less than the given <paramref name = "other"/> value, or otherwise throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The comparable to be checked.</param>
        /// <param name = "other">The boundary value that must be less than or equal to <paramref name = "parameter"/>.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">Thrown when the specified <paramref name = "parameter"/> is less than <paramref name = "other"/>.</exception>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "parameter"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MustBeGreaterThanOrEqualTo<T>([NotNull] this T parameter, T other, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
            where T : IComparable<T>
        {
            if (parameter.MustNotBeNullReference(parameterName, message).CompareTo(other) < 0)
            {
                Throw.MustBeGreaterThanOrEqualTo(parameter, other, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that <paramref name = "parameter"/> is within the specified range, or otherwise throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <typeparam name = "T">The type of the parameter to be checked.</typeparam>
        /// <param name = "parameter">The parameter to be checked.</param>
        /// <param name = "range">The range where <paramref name = "parameter"/> must be in-between.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">Thrown when <paramref name = "parameter"/> is not within <paramref name = "range"/>.</exception>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "parameter"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MustBeIn<T>([NotNull] this T parameter, Range<T> range, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
            where T : IComparable<T>
        {
            if (!range.IsValueWithinRange(parameter.MustNotBeNullReference(parameterName, message)))
            {
                Throw.MustBeInRange(parameter, range, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that <paramref name = "parameter"/> can be cast to <typeparamref name = "T"/> and returns the cast value, or otherwise throws a <see cref = "TypeCastException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be cast.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "TypeCastException">Thrown when <paramref name = "parameter"/> cannot be cast to <typeparamref name = "T"/>.</exception>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "parameter"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MustBeOfType<T>([NotNull, NoEnumeration] this object? parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (parameter.MustNotBeNull(parameterName, message)is T castValue)
                return castValue;
            Throw.InvalidTypeCast(parameter, typeof(T), parameterName, message);
            return default;
        }

        /// <summary>
        /// Ensures that <paramref name = "parameter"/> can be cast to <typeparamref name = "T"/> and returns the cast value, or otherwise throws your custom exception.
        /// </summary>
        /// <param name = "parameter">The value to be cast.</param>
        /// <param name = "exceptionFactory">The delegate that creates your custom exception. The <paramref name = "parameter"/> is passed to this delegate.</param>
        /// <exception cref = "Exception">Your custom exception thrown when <paramref name = "parameter"/> cannot be cast to <typeparamref name = "T"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MustBeOfType<T>([NotNull, NoEnumeration] this object? parameter, Func<object?, Exception> exceptionFactory)
        {
            if (parameter is T castValue)
                return castValue;
            Throw.CustomException(exceptionFactory, parameter);
            return default;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is positive (greater than zero), or otherwise
        /// throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// Thrown when <paramref name = "parameter"/> is zero or negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte MustBePositive(this sbyte parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (!(parameter > 0))
            {
                Throw.MustBePositive(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is positive (greater than zero), or otherwise
        /// throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// Thrown when <paramref name = "parameter"/> is zero.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte MustBePositive(this byte parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (!(parameter > 0))
            {
                Throw.MustBePositive(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is positive (greater than zero), or otherwise
        /// throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// Thrown when <paramref name = "parameter"/> is zero or negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short MustBePositive(this short parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (!(parameter > 0))
            {
                Throw.MustBePositive(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is positive (greater than zero), or otherwise
        /// throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// Thrown when <paramref name = "parameter"/> is zero.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort MustBePositive(this ushort parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (!(parameter > 0))
            {
                Throw.MustBePositive(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is positive (greater than zero), or otherwise
        /// throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// Thrown when <paramref name = "parameter"/> is zero or negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MustBePositive(this int parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (!(parameter > 0))
            {
                Throw.MustBePositive(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is positive (greater than zero), or otherwise
        /// throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// Thrown when <paramref name = "parameter"/> is zero.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint MustBePositive(this uint parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (!(parameter > 0U))
            {
                Throw.MustBePositive(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is positive (greater than zero), or otherwise
        /// throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// Thrown when <paramref name = "parameter"/> is zero or negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long MustBePositive(this long parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (!(parameter > 0L))
            {
                Throw.MustBePositive(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is positive (greater than zero), or otherwise
        /// throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// Thrown when <paramref name = "parameter"/> is zero.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong MustBePositive(this ulong parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (!(parameter > 0UL))
            {
                Throw.MustBePositive(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is positive (greater than zero), or otherwise
        /// throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// Thrown when <paramref name = "parameter"/> is zero or negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal MustBePositive(this decimal parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (!(parameter > 0m))
            {
                Throw.MustBePositive(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is positive (greater than zero), or otherwise
        /// throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// Thrown when <paramref name = "parameter"/> is zero (including negative zero), negative, or NaN.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MustBePositive(this float parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (!(parameter > 0f))
            {
                Throw.MustBePositive(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is positive (greater than zero), or otherwise
        /// throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// Thrown when <paramref name = "parameter"/> is zero (including negative zero), negative, or NaN.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double MustBePositive(this double parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (!(parameter > 0d))
            {
                Throw.MustBePositive(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is positive (greater than zero), or otherwise
        /// throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// Thrown when <paramref name = "parameter"/> is <see cref = "TimeSpan.Zero"/> or negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan MustBePositive(this TimeSpan parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (!(parameter > TimeSpan.Zero))
            {
                Throw.MustBePositive(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is positive (greater than zero) or exactly equals
        /// <see cref = "System.Threading.Timeout.InfiniteTimeSpan"/>, or otherwise throws an
        /// <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// Thrown when <paramref name = "parameter"/> is neither positive nor exactly equal to
        /// <see cref = "System.Threading.Timeout.InfiniteTimeSpan"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan MustBePositiveOrInfinite(this TimeSpan parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (!(parameter > TimeSpan.Zero || parameter == Timeout.InfiniteTimeSpan))
            {
                Throw.MustBePositiveOrInfinite(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified string is a valid URI of the supplied kind, or otherwise throws an
        /// <see cref = "InvalidUriException"/>.
        /// </summary>
        /// <param name = "parameter">The string to be checked.</param>
        /// <param name = "uriKind">The kind of URI that the string must represent.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "InvalidUriException">
        /// Thrown when <paramref name = "parameter"/> is not a valid URI of the supplied kind.
        /// </exception>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "parameter"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MustBeUri([NotNull] this string? parameter, UriKind uriKind = UriKind.RelativeOrAbsolute, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            parameter.MustNotBeNull(parameterName, message);
            if (!Uri.TryCreate(parameter, uriKind, out _))
            {
                Throw.MustBeUri(parameter, uriKind, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified string is a valid URI of the supplied kind, or otherwise throws your custom
        /// exception.
        /// </summary>
        /// <param name = "parameter">The string to be checked.</param>
        /// <param name = "uriKind">The kind of URI that the string must represent.</param>
        /// <param name = "exceptionFactory">
        /// The delegate that creates the exception to be thrown. <paramref name = "parameter"/> is passed to this delegate.
        /// </param>
        /// <exception cref = "Exception">
        /// Your custom exception thrown when <paramref name = "parameter"/> is null or not a valid URI of the supplied kind.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MustBeUri([NotNull] this string? parameter, UriKind uriKind, Func<string?, Exception> exceptionFactory)
        {
            if (parameter is null || !Uri.TryCreate(parameter, uriKind, out _))
            {
                Throw.CustomException(exceptionFactory, parameter);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified string is a valid URI of the supplied kind, or otherwise throws your custom
        /// exception.
        /// </summary>
        /// <param name = "parameter">The string to be checked.</param>
        /// <param name = "uriKind">The kind of URI that the string must represent.</param>
        /// <param name = "exceptionFactory">
        /// The delegate that creates the exception to be thrown. <paramref name = "parameter"/> and
        /// <paramref name = "uriKind"/> are passed to this delegate.
        /// </param>
        /// <exception cref = "Exception">
        /// Your custom exception thrown when <paramref name = "parameter"/> is null or not a valid URI of the supplied kind.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MustBeUri([NotNull] this string? parameter, UriKind uriKind, Func<string?, UriKind, Exception> exceptionFactory)
        {
            if (parameter is null || !Uri.TryCreate(parameter, uriKind, out _))
            {
                Throw.CustomException(exceptionFactory, parameter, uriKind);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified enum value is valid, or otherwise throws an <see cref = "EnumValueNotDefinedException"/>. An enum value
        /// is valid when the specified value is one of the constants defined in the enum, or a valid flags combination when the enum type
        /// is marked with the <see cref = "FlagsAttribute"/>.
        /// </summary>
        /// <typeparam name = "T">The type of the enum.</typeparam>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "EnumValueNotDefinedException">Thrown when <paramref name = "parameter"/> is no valid enum value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MustBeValidEnumValue<T>(this T parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
            where T : struct, Enum
        {
            if (!EnumInfo<T>.IsValidEnumValue(parameter))
            {
                Throw.EnumValueNotDefined(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that <paramref name = "parameter"/> is not equal to <paramref name = "other"/> using the default equality comparer, or otherwise throws a <see cref = "ValuesEqualException"/>.
        /// </summary>
        /// <param name = "parameter">The first value to be compared.</param>
        /// <param name = "other">The other value to be compared.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ValuesEqualException">Thrown when <paramref name = "parameter"/> and <paramref name = "other"/> are equal.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MustNotBe<T>(this T parameter, T other, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (EqualityComparer<T>.Default.Equals(parameter, other))
            {
                Throw.ValuesEqual(parameter, other, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that <paramref name = "parameter"/> is not equal to <paramref name = "other"/> using the specified equality comparer, or otherwise throws a <see cref = "ValuesEqualException"/>.
        /// </summary>
        /// <param name = "parameter">The first value to be compared.</param>
        /// <param name = "other">The other value to be compared.</param>
        /// <param name = "equalityComparer">The equality comparer used for comparing the two values.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ValuesEqualException">Thrown when <paramref name = "parameter"/> and <paramref name = "other"/> are equal.</exception>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "equalityComparer"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MustNotBe<T>(this T parameter, T other, IEqualityComparer<T> equalityComparer, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (equalityComparer.MustNotBeNull(nameof(equalityComparer), message).Equals(parameter, other))
            {
                Throw.ValuesEqual(parameter, other, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the two strings are not equal using the specified <paramref name = "comparisonType"/>, or otherwise throws a <see cref = "ValuesEqualException"/>.
        /// </summary>
        /// <param name = "parameter">The first string to be compared.</param>
        /// <param name = "other">The second string to be compared.</param>
        /// <param name = "comparisonType">The enum value specifying how the two strings should be compared.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ValuesEqualException">Thrown when <paramref name = "parameter"/> is equal to <paramref name = "other"/>.</exception>
        /// <exception cref = "ArgumentException">Thrown when <paramref name = "comparisonType"/> is not a valid value from the <see cref = "StringComparison"/> enum.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? MustNotBe(this string? parameter, string? other, StringComparison comparisonType, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (string.Equals(parameter, other, comparisonType))
            {
                Throw.ValuesEqual(parameter, other, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the two strings are not equal using the specified <paramref name = "comparisonType"/>, or otherwise throws a <see cref = "ValuesEqualException"/>.
        /// </summary>
        /// <param name = "parameter">The first string to be compared.</param>
        /// <param name = "other">The second string to be compared.</param>
        /// <param name = "comparisonType">The enum value specifying how the two strings should be compared.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ValuesEqualException">Thrown when <paramref name = "parameter"/> is equal to <paramref name = "other"/>.</exception>
        /// <exception cref = "ArgumentException">Thrown when <paramref name = "comparisonType"/> is not a valid value from the <see cref = "StringComparisonType"/> enum.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? MustNotBe(this string? parameter, string? other, StringComparisonType comparisonType, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (parameter.Equals(other, comparisonType))
            {
                Throw.ValuesEqual(parameter, other, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified parameter is not the default value, or otherwise throws an <see cref = "ArgumentNullException"/>
        /// for reference types, or an <see cref = "ArgumentDefaultException"/> for value types.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "parameter"/> is a reference type and null.</exception>
        /// <exception cref = "ArgumentDefaultException">Thrown when <paramref name = "parameter"/> is a value type and the default value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MustNotBeDefault<T>([NotNull] this T parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (default(T)is null)
            {
                if (parameter is null)
                {
                    Throw.ArgumentNull(parameterName, message);
                }

                return parameter;
            }

            if (EqualityComparer<T>.Default.Equals(parameter, default!))
            {
                Throw.ArgumentDefault(parameterName, message);
            }

#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.

            return parameter;
#pragma warning restore CS8777
        }

        /// <summary>
        /// Ensures that the specified parameter is not the default value, or otherwise throws your custom exception.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "exceptionFactory">The delegate that creates your custom exception.</param>
        /// <exception cref = "Exception">Your custom exception thrown when <paramref name = "parameter"/> is the default value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MustNotBeDefault<T>([NotNull] this T parameter, Func<Exception> exceptionFactory)
        {
            if (default(T)is null)
            {
                if (parameter is null)
                {
                    Throw.CustomException(exceptionFactory);
                }

                return parameter;
            }

            if (EqualityComparer<T>.Default.Equals(parameter, default!))
            {
                Throw.CustomException(exceptionFactory);
            }

#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.

            return parameter;
#pragma warning restore CS8777
        }

        /// <summary>
        /// Ensures that the specified <see cref = "ImmutableArray{T}"/> is not default or empty, or otherwise throws an <see cref = "Exceptions.EmptyCollectionException"/>.
        /// </summary>
        /// <param name = "parameter">The <see cref = "ImmutableArray{T}"/> to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "Exceptions.EmptyCollectionException">Thrown when <paramref name = "parameter"/> is default or empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImmutableArray<T> MustNotBeDefaultOrEmpty<T>(this ImmutableArray<T> parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (parameter.IsDefaultOrEmpty)
            {
                Throw.EmptyCollection(parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified GUID is not empty, or otherwise throws an <see cref = "EmptyGuidException"/>.
        /// </summary>
        /// <param name = "parameter">The GUID to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "EmptyGuidException">Thrown when <paramref name = "parameter"/> is an empty GUID.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid MustNotBeEmpty(this Guid parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (parameter == Guid.Empty)
            {
                Throw.EmptyGuid(parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified GUID is not empty, or otherwise throws your custom exception.
        /// </summary>
        /// <param name = "parameter">The GUID to be checked.</param>
        /// <param name = "exceptionFactory">The delegate that creates your custom exception.</param>
        /// <exception cref = "Exception">Your custom exception thrown when <paramref name = "parameter"/> is an empty GUID.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid MustNotBeEmpty(this Guid parameter, Func<Exception> exceptionFactory)
        {
            if (parameter == Guid.Empty)
            {
                Throw.CustomException(exceptionFactory);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified span is not empty, or otherwise throws an <see cref = "EmptyCollectionException"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> MustNotBeEmpty<T>(this Span<T> parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            ((ReadOnlySpan<T>)parameter).MustNotBeEmpty(parameterName, message);
            return parameter;
        }

        /// <summary>
        /// Ensures that the specified span is not empty, or otherwise throws your custom exception.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> MustNotBeEmpty<T>(this Span<T> parameter, ReadOnlySpanExceptionFactory<T> exceptionFactory)
        {
            ((ReadOnlySpan<T>)parameter).MustNotBeEmpty(exceptionFactory);
            return parameter;
        }

        /// <summary>
        /// Ensures that the specified read-only span is not empty, or otherwise throws an <see cref = "EmptyCollectionException"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> MustNotBeEmpty<T>(this ReadOnlySpan<T> parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (parameter.IsEmpty)
            {
                Throw.EmptyCollection(parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified read-only span is not empty, or otherwise throws your custom exception.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> MustNotBeEmpty<T>(this ReadOnlySpan<T> parameter, ReadOnlySpanExceptionFactory<T> exceptionFactory)
        {
            if (parameter.IsEmpty)
            {
                Throw.CustomSpanException(exceptionFactory, parameter);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified memory is not empty, or otherwise throws an <see cref = "EmptyCollectionException"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<T> MustNotBeEmpty<T>(this Memory<T> parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            ((ReadOnlySpan<T>)parameter.Span).MustNotBeEmpty(parameterName, message);
            return parameter;
        }

        /// <summary>
        /// Ensures that the specified memory is not empty, or otherwise throws your custom exception.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<T> MustNotBeEmpty<T>(this Memory<T> parameter, ReadOnlySpanExceptionFactory<T> exceptionFactory)
        {
            ((ReadOnlySpan<T>)parameter.Span).MustNotBeEmpty(exceptionFactory);
            return parameter;
        }

        /// <summary>
        /// Ensures that the specified read-only memory is not empty, or otherwise throws an <see cref = "EmptyCollectionException"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<T> MustNotBeEmpty<T>(this ReadOnlyMemory<T> parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            parameter.Span.MustNotBeEmpty(parameterName, message);
            return parameter;
        }

        /// <summary>
        /// Ensures that the specified read-only memory is not empty, or otherwise throws your custom exception.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<T> MustNotBeEmpty<T>(this ReadOnlyMemory<T> parameter, ReadOnlySpanExceptionFactory<T> exceptionFactory)
        {
            parameter.Span.MustNotBeEmpty(exceptionFactory);
            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is not less than the given <paramref name = "other"/> value, or otherwise throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The comparable to be checked.</param>
        /// <param name = "other">The boundary value that must be less than or equal to <paramref name = "parameter"/>.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">Thrown when the specified <paramref name = "parameter"/> is less than <paramref name = "other"/>.</exception>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "parameter"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MustNotBeLessThan<T>([NotNull] this T parameter, T other, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
            where T : IComparable<T>
        {
            if (parameter.MustNotBeNullReference(parameterName, message).CompareTo(other) < 0)
            {
                Throw.MustNotBeLessThan(parameter, other, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is not negative (greater than or equal to zero), or otherwise
        /// throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// Thrown when <paramref name = "parameter"/> is less than zero.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte MustNotBeNegative(this sbyte parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (!(parameter >= 0))
            {
                Throw.MustNotBeNegative(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is not negative (greater than or equal to zero), or otherwise
        /// throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// Thrown when <paramref name = "parameter"/> is less than zero.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short MustNotBeNegative(this short parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (!(parameter >= 0))
            {
                Throw.MustNotBeNegative(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is not negative (greater than or equal to zero), or otherwise
        /// throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// Thrown when <paramref name = "parameter"/> is less than zero.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MustNotBeNegative(this int parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (!(parameter >= 0))
            {
                Throw.MustNotBeNegative(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is not negative (greater than or equal to zero), or otherwise
        /// throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// Thrown when <paramref name = "parameter"/> is less than zero.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long MustNotBeNegative(this long parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (!(parameter >= 0L))
            {
                Throw.MustNotBeNegative(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is not negative (greater than or equal to zero), or otherwise
        /// throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// Thrown when <paramref name = "parameter"/> is less than zero.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal MustNotBeNegative(this decimal parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (!(parameter >= 0m))
            {
                Throw.MustNotBeNegative(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is not negative (greater than or equal to zero), or otherwise
        /// throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// Thrown when <paramref name = "parameter"/> is less than zero or NaN.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MustNotBeNegative(this float parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (!(parameter >= 0f))
            {
                Throw.MustNotBeNegative(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is not negative (greater than or equal to zero), or otherwise
        /// throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// Thrown when <paramref name = "parameter"/> is less than zero or NaN.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double MustNotBeNegative(this double parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (!(parameter >= 0d))
            {
                Throw.MustNotBeNegative(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified <paramref name = "parameter"/> is not negative (greater than or equal to zero), or otherwise
        /// throws an <see cref = "ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name = "parameter">The value to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// Thrown when <paramref name = "parameter"/> is less than <see cref = "TimeSpan.Zero"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan MustNotBeNegative(this TimeSpan parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (!(parameter >= TimeSpan.Zero))
            {
                Throw.MustNotBeNegative(parameter, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified object reference is not null, or otherwise throws an <see cref = "ArgumentNullException"/>.
        /// </summary>
        /// <param name = "parameter">The object reference to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "parameter"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MustNotBeNull<T>([NotNull, NoEnumeration] this T? parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
            where T : class
        {
            if (parameter is null)
            {
                Throw.ArgumentNull(parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified object reference is not null, or otherwise throws your custom exception.
        /// </summary>
        /// <param name = "parameter">The reference to be checked.</param>
        /// <param name = "exceptionFactory">The delegate that creates your custom exception.</param>
        /// <exception cref = "Exception">Your custom exception thrown when <paramref name = "parameter"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MustNotBeNull<T>([NotNull, NoEnumeration] this T? parameter, Func<Exception> exceptionFactory)
            where T : class
        {
            if (parameter is null)
            {
                Throw.CustomException(exceptionFactory);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the collection is not null or empty, or otherwise throws an <see cref = "EmptyCollectionException"/>.
        /// </summary>
        /// <param name = "parameter">The collection to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "EmptyCollectionException">Thrown when <paramref name = "parameter"/> has no items.</exception>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "parameter"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TCollection MustNotBeNullOrEmpty<TCollection>([NotNull] this TCollection? parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
            where TCollection : class, IEnumerable
        {
            if (parameter.Count(parameterName, message) == 0)
            {
                Throw.EmptyCollection(parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified string is not null or empty, or otherwise throws an <see cref = "ArgumentNullException"/> or <see cref = "EmptyStringException"/>.
        /// </summary>
        /// <param name = "parameter">The string to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "EmptyStringException">Thrown when <paramref name = "parameter"/> is an empty string.</exception>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "parameter"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MustNotBeNullOrEmpty([NotNull] this string? parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (parameter is null)
            {
                Throw.ArgumentNull(parameterName, message);
            }

            if (parameter.Length == 0)
            {
                Throw.EmptyString(parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified string is not null, empty, or contains only white space, or otherwise throws an <see cref = "ArgumentNullException"/>, an <see cref = "EmptyStringException"/>, or a <see cref = "WhiteSpaceStringException"/>.
        /// </summary>
        /// <param name = "parameter">The string to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "WhiteSpaceStringException">Thrown when <paramref name = "parameter"/> contains only white space.</exception>
        /// <exception cref = "EmptyStringException">Thrown when <paramref name = "parameter"/> is an empty string.</exception>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "parameter"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MustNotBeNullOrWhiteSpace([NotNull] this string? parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            parameter.MustNotBeNullOrEmpty(parameterName, message);
            foreach (var character in parameter)
            {
                if (!character.IsWhiteSpace())
                {
                    return parameter;
                }
            }

            Throw.WhiteSpaceString(parameter, parameterName, message);
            return null;
        }

        /// <summary>
        /// Ensures that the specified string is not null, empty, or contains only white space, or otherwise throws your custom exception.
        /// </summary>
        /// <param name = "parameter">The string to be checked.</param>
        /// <param name = "exceptionFactory">The delegate that creates your custom exception. <paramref name = "parameter"/> is passed to this delegate.</param>
        /// <exception cref = "Exception">Your custom exception thrown when <paramref name = "parameter"/> is null, empty, or contains only white space.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MustNotBeNullOrWhiteSpace([NotNull] this string? parameter, Func<string?, Exception> exceptionFactory)
        {
            if (parameter.IsNullOrWhiteSpace())
            {
                Throw.CustomException(exceptionFactory, parameter);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the specified parameter is not null when <typeparamref name = "T"/> is a reference type, or otherwise
        /// throws an <see cref = "ArgumentNullException"/>. PLEASE NOTICE: you should only use this assertion in generic contexts,
        /// use <see cref = "MustNotBeNull{T}(T, string, string)"/> by default.
        /// </summary>
        /// <param name = "parameter">The value to be checked for null.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ArgumentNullException">Thrown when <typeparamref name = "T"/> is a reference type and <paramref name = "parameter"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MustNotBeNullReference<T>([NotNull, NoEnumeration] this T parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (default(T) != null)
            {
                // If we end up here, parameter cannot be null
#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.

                return parameter;
#pragma warning restore CS8777
            }

            if (parameter is null)
            {
                Throw.ArgumentNull(parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the collection does not contain a null item, or otherwise throws an <see cref = "ExistingItemException"/>.
        /// </summary>
        /// <param name = "parameter">The collection to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ExistingItemException">Thrown when <paramref name = "parameter"/> contains a null item.</exception>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "parameter"/> is null.</exception>
        /// <remarks>
        /// This method inspects the collection once, stops at the first null item, runs in O(n) time, and uses constant
        /// additional space. <see cref = "IList"/> receivers are inspected by index without allocating an enumerator; other
        /// receivers are enumerated once. Empty collections succeed. Non-generic access boxes value-type items.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TCollection MustNotContainNull<TCollection>([NotNull] this TCollection? parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
            where TCollection : class, IEnumerable
        {
            var position = FindNullItem(parameter.MustNotBeNull(parameterName, message));
            if (position >= 0)
            {
                Throw.NullItem(position, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the <see cref = "ImmutableArray{T}"/> does not contain a null item, or otherwise throws an <see cref = "ExistingItemException"/>.
        /// </summary>
        /// <param name = "parameter">The immutable array to be checked.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "ExistingItemException">Thrown when <paramref name = "parameter"/> contains a null item.</exception>
        /// <remarks>
        /// This method inspects an initialized array by index without allocating an enumerator, stops at the first null
        /// item, runs in O(n) time, and uses constant additional space. Empty and default immutable arrays succeed.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImmutableArray<T> MustNotContainNull<T>(this ImmutableArray<T> parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (parameter.IsDefault)
            {
                return parameter;
            }

            for (var position = 0; position < parameter.Length; ++position)
            {
                if (parameter[position] is null)
                {
                    Throw.NullItem(position, parameterName, message);
                }
            }

            return parameter;
        }

        private static int FindNullItem(IEnumerable parameter)
        {
            if (parameter is IList list)
            {
                for (var position = 0; position < list.Count; ++position)
                {
                    if (list[position] is null)
                    {
                        return position;
                    }
                }

                return -1;
            }

            var currentPosition = 0;
            foreach (var item in parameter)
            {
                if (item is null)
                {
                    return currentPosition;
                }

                ++currentPosition;
            }

            return -1;
        }

        /// <summary>
        /// Ensures that the string does not start with the specified value, or otherwise throws a <see cref = "SubstringException"/>.
        /// </summary>
        /// <param name = "parameter">The string to be checked.</param>
        /// <param name = "value">The other string that <paramref name = "parameter"/> must not start with.</param>
        /// <param name = "comparisonType">One of the enumeration values that specifies the rules for the search (optional). The default value is <see cref = "StringComparison.CurrentCulture"/>.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "SubstringException">Thrown when <paramref name = "parameter"/> starts with <paramref name = "value"/>.</exception>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "parameter"/> or <paramref name = "value"/> is null.</exception>
        public static string MustNotStartWith([NotNull] this string? parameter, [NotNull] string value, StringComparison comparisonType = StringComparison.CurrentCulture, [CallerArgumentExpression(nameof(parameter))] string? parameterName = null, string? message = null)
        {
            if (parameter.MustNotBeNull(parameterName, message).StartsWith(value, comparisonType))
            {
                Throw.StringStartsWith(parameter, value, comparisonType, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Ensures that the span does not start with the specified value, or otherwise throws a <see cref = "SubstringException"/>.
        /// </summary>
        /// <param name = "parameter">The span to be checked.</param>
        /// <param name = "value">The other span that <paramref name = "parameter"/> must not start with.</param>
        /// <param name = "comparisonType">One of the enumeration values that specifies the rules for the search.</param>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref = "SubstringException">Thrown when <paramref name = "parameter"/> starts with <paramref name = "value"/>.</exception>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "parameter"/> or <paramref name = "value"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> MustNotStartWith(this ReadOnlySpan<char> parameter, ReadOnlySpan<char> value, StringComparison comparisonType, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
        {
            if (parameter.StartsWith(value, comparisonType))
            {
                Throw.StringStartsWith(parameter, value, comparisonType, parameterName, message);
            }

            return parameter;
        }

        /// <summary>
        /// Checks if the specified <paramref name = "condition"/> is true and throws an <see cref = "ObjectDisposedException"/> in this case.
        /// </summary>
        /// <param name = "condition">The condition to be checked. The exception is thrown when it is true.</param>
        /// <param name = "objectName">The name of the disposed object (optional).</param>
        /// <param name = "message">The message that will be passed to the <see cref = "ObjectDisposedException"/> (optional).</param>
        /// <exception cref = "ObjectDisposedException">Thrown when <paramref name = "condition"/> is true.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ObjectDisposed(bool condition, string? objectName = null, string? message = null)
        {
            if (condition)
            {
                Throw.ObjectDisposed(objectName, message);
            }
        }
    }

    /// <summary>
    /// Provides meta-information about enum values and the flag bitmask if the enum is marked with the <see cref = "FlagsAttribute"/>.
    /// Can be used to validate that an enum value is valid.
    /// </summary>
    /// <typeparam name = "T">The type of the enum.</typeparam>
    public static class EnumInfo<T>
        where T : struct, Enum
    {
        // ReSharper disable StaticMemberInGenericType
        /// <summary>
        /// Gets the value indicating whether the enum type is marked with the flags attribute.
        /// </summary>
        public static readonly bool IsFlagsEnum = typeof(T).GetCustomAttribute(Types.FlagsAttributeType) != null;
        /// <summary>
        /// Gets the flags pattern when <see cref = "IsFlagsEnum"/> is true. If the enum is not a flags enum, then 0UL is returned.
        /// </summary>
        public static readonly ulong FlagsPattern;
        private static readonly int EnumSize = Unsafe.SizeOf<T>();
        private static readonly T[] EnumConstantsArray;
        /// <summary>
        /// Gets the values of the enum as a read-only collection.
        /// </summary>
        public static ReadOnlyMemory<T> EnumConstants { get; }

        static EnumInfo()
        {
            EnumConstantsArray = (T[])Enum.GetValues(typeof(T));
            EnumConstants = new ReadOnlyMemory<T>(EnumConstantsArray);
            if (!IsFlagsEnum)
            {
                return;
            }

            for (var i = 0; i < EnumConstantsArray.Length; ++i)
            {
                var convertedValue = ConvertToUInt64(EnumConstantsArray[i]);
                FlagsPattern |= convertedValue;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsValidFlagsValue(T enumValue)
        {
            var convertedValue = ConvertToUInt64(enumValue);
            return (FlagsPattern & convertedValue) == convertedValue;
        }

        private static bool IsValidValue(T parameter)
        {
            var comparer = EqualityComparer<T>.Default;
            for (var i = 0; i < EnumConstantsArray.Length; ++i)
            {
                if (comparer.Equals(EnumConstantsArray[i], parameter))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the specified enum value is valid. This is true if either the enum is a standard enum and the enum value corresponds
        /// to one of the enum constant values or if the enum type is marked with the <see cref = "FlagsAttribute"/> and the given value
        /// is a valid combination of bits for this type.
        /// </summary>
        /// <param name = "enumValue">The enum value to be checked.</param>
        /// <returns>True if either the enum value is </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidEnumValue(T enumValue) => IsFlagsEnum ? IsValidFlagsValue(enumValue) : IsValidValue(enumValue);
        private static ulong ConvertToUInt64(T value)
        {
            switch (EnumSize)
            {
                case 1:
                    return Unsafe.As<T, byte>(ref value);
                case 2:
                    return Unsafe.As<T, ushort>(ref value);
                case 4:
                    return Unsafe.As<T, uint>(ref value);
                case 8:
                    return Unsafe.As<T, ulong>(ref value);
                default:
                    ThrowUnknownEnumSize();
                    return 0UL;
            }
        }

        private static void ThrowUnknownEnumSize() => throw new InvalidOperationException($"The enum type \"{typeof(T)}\" has an unknown size of {EnumSize}. This means that the underlying enum type is not one of the supported ones.");
    }

    /// <summary>
    /// Defines a range that can be used to check if a specified <see cref = "IComparable{T}"/> is in between it or not.
    /// </summary>
    /// <typeparam name = "T">The type that the range should be applied to.</typeparam>
    public readonly struct Range<T> : IEquatable<Range<T>> where T : IComparable<T>
    {
        /// <summary>
        /// Gets the lower boundary of the range.
        /// </summary>
        public readonly T From;
        /// <summary>
        /// Gets the upper boundary of the range.
        /// </summary>
        public readonly T To;
        /// <summary>
        /// Gets the value indicating whether the From value is included in the range.
        /// </summary>
        public readonly bool IsFromInclusive;
        /// <summary>
        /// Gets the value indicating whether the To value is included in the range.
        /// </summary>
        public readonly bool IsToInclusive;
        private readonly int _expectedLowerBoundaryResult;
        private readonly int _expectedUpperBoundaryResult;
        /// <summary>
        /// Creates a new instance of <see cref = "Range{T}"/>.
        /// </summary>
        /// <param name = "from">The lower boundary of the range.</param>
        /// <param name = "to">The upper boundary of the range.</param>
        /// <param name = "isFromInclusive">The value indicating whether <paramref name = "from"/> is part of the range.</param>
        /// <param name = "isToInclusive">The value indicating whether <paramref name = "to"/> is part of the range.</param>
        /// <exception cref = "ArgumentOutOfRangeException">Thrown when <paramref name = "to"/> is less than <paramref name = "from"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Range(T from, T to, bool isFromInclusive = true, bool isToInclusive = true)
        {
            From = from.MustNotBeNullReference(nameof(from));
            To = to.MustNotBeLessThan(from, nameof(to));
            IsFromInclusive = isFromInclusive;
            IsToInclusive = isToInclusive;
            _expectedLowerBoundaryResult = isFromInclusive ? 0 : 1;
            _expectedUpperBoundaryResult = isToInclusive ? 0 : -1;
        }

        /// <summary>
        /// Checks if the specified <paramref name = "value"/> is within range.
        /// </summary>
        /// <param name = "value">The value to be checked.</param>
        /// <returns>True if value is within range, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValueWithinRange(T value) => value.MustNotBeNullReference(nameof(value)).CompareTo(From) >= _expectedLowerBoundaryResult && value.CompareTo(To) <= _expectedUpperBoundaryResult;
        /// <summary>
        /// Use this method to create a range in a fluent style using method chaining.
        /// Defines the lower boundary as an inclusive value.
        /// </summary>
        /// <param name = "value">The value that indicates the inclusive lower boundary of the resulting range.</param>
        /// <returns>A value you can use to fluently define the upper boundary of a new range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RangeFromInfo FromInclusive(T value) => new(value, true);
        /// <summary>
        /// Use this method to create a range in a fluent style using method chaining.
        /// Defines the lower boundary as an exclusive value.
        /// </summary>
        /// <param name = "value">The value that indicates the exclusive lower boundary of the resulting range.</param>
        /// <returns>A value you can use to fluently define the upper boundary of a new range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RangeFromInfo FromExclusive(T value) => new(value, false);
        /// <summary>
        /// The nested <see cref = "RangeFromInfo"/> can be used to fluently create a <see cref = "Range{T}"/>.
        /// </summary>
        public readonly struct RangeFromInfo
        {
            private readonly T _from;
            private readonly bool _isFromInclusive;
            /// <summary>
            /// Creates a new RangeFromInfo.
            /// </summary>
            /// <param name = "from">The lower boundary of the range.</param>
            /// <param name = "isFromInclusive">The value indicating whether <paramref name = "from"/> is part of the range.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public RangeFromInfo(T from, bool isFromInclusive)
            {
                _from = from;
                _isFromInclusive = isFromInclusive;
            }

            /// <summary>
            /// Use this method to create a range in a fluent style using method chaining.
            /// Defines the upper boundary as an exclusive value.
            /// </summary>
            /// <param name = "value">The value that indicates the exclusive upper boundary of the resulting range.</param>
            /// <returns>A new range with the specified upper and lower boundaries.</returns>
            /// <exception cref = "ArgumentOutOfRangeException">
            /// Thrown when <paramref name = "value"/> is less than the lower boundary value.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Range<T> ToExclusive(T value) => new(_from, value, _isFromInclusive, false);
            /// <summary>
            /// Use this method to create a range in a fluent style using method chaining.
            /// Defines the upper boundary as an inclusive value.
            /// </summary>
            /// <param name = "value">The value that indicates the inclusive upper boundary of the resulting range.</param>
            /// <returns>A new range with the specified upper and lower boundaries.</returns>
            /// <exception cref = "ArgumentOutOfRangeException">
            /// Thrown when <paramref name = "value"/> is less than the lower boundary value.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Range<T> ToInclusive(T value) => new(_from, value, _isFromInclusive);
        }

        /// <inheritdoc/>
        public override string ToString() => $"Range from {CreateRangeDescriptionText()}";
        /// <summary>
        /// Returns either "inclusive" or "exclusive", depending on whether <see cref = "IsFromInclusive"/> is true or false.
        /// </summary>
        public string LowerBoundaryText {[MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetBoundaryText(IsFromInclusive); }
        /// <summary>
        /// Returns either "inclusive" or "exclusive", depending on whether <see cref = "IsToInclusive"/> is true or false.
        /// </summary>
        public string UpperBoundaryText {[MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetBoundaryText(IsToInclusive); }

        /// <summary>
        /// Returns a text description of this range with the following pattern: From (inclusive | exclusive) to To (inclusive | exclusive).
        /// </summary>
        public string CreateRangeDescriptionText(string fromToConnectionWord = "to") => From + " (" + LowerBoundaryText + ") " + fromToConnectionWord + ' ' + To + " (" + UpperBoundaryText + ")";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetBoundaryText(bool isInclusive) => isInclusive ? "inclusive" : "exclusive";
        /// <inheritdoc/>
        public bool Equals(Range<T> other)
        {
            if (IsFromInclusive != other.IsFromInclusive || IsToInclusive != other.IsToInclusive)
            {
                return false;
            }

            var comparer = EqualityComparer<T>.Default;
            return comparer.Equals(From, other.From) && comparer.Equals(To, other.To);
        }

        /// <inheritdoc/>
        public override bool Equals(object? other)
        {
            if (other is null)
            {
                return false;
            }

            return other is Range<T> range && Equals(range);
        }

        /// <inheritdoc/>
        public override int GetHashCode() => MultiplyAddHash.CreateHashCode(From, To, IsFromInclusive, IsToInclusive);
        /// <summary>
        /// Checks if two ranges are equal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Range<T> first, Range<T> second) => first.Equals(second);
        /// <summary>
        /// Checks if two ranges are not equal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Range<T> first, Range<T> second) => first.Equals(second) == false;
    }

    /// <summary>
    /// Specifies the culture, case , and sort rules when comparing strings.
    /// </summary>
    /// <remarks>
    /// This enum is en extension of <see cref = "System.StringComparison"/>, adding
    /// capabilities to ignore white space when making string equality comparisons.
    /// See the <see cref = "Check.Equals(string, string, StringComparisonType)"/> when
    /// you want to compare in such a way.
    /// </remarks>
    public enum StringComparisonType
    {
        /// <summary>
        /// Compare strings using culture-sensitive sort rules and the current culture.
        /// </summary>
        CurrentCulture = 0,
        /// <summary>
        /// Compare strings using culture-sensitive sort rules, the current culture, and
        /// ignoring the case of the strings being compared.
        /// </summary>
        CurrentCultureIgnoreCase = 1,
        /// <summary>
        /// Compare strings using culture-sensitive sort rules and the invariant culture.
        /// </summary>
        InvariantCulture = 2,
        /// <summary>
        /// Compare strings using culture-sensitive sort rules, the invariant culture, and
        /// ignoring the case of the strings being compared.
        /// </summary>
        InvariantCultureIgnoreCase = 3,
        /// <summary>
        /// Compare strings using ordinal sort rules.
        /// </summary>
        Ordinal = 4,
        /// <summary>
        /// Compare strings using ordinal sort rules and ignoring the case of the strings
        /// being compared.
        /// </summary>
        OrdinalIgnoreCase = 5,
        /// <summary>
        /// Compare strings using ordinal sort rules and ignoring the white space characters
        /// of the strings being compared.
        /// </summary>
        OrdinalIgnoreWhiteSpace = 6,
        /// <summary>
        /// Compare strings using ordinal sort rules, ignoring the case and ignoring the
        /// white space characters of the strings being compared.
        /// </summary>
        OrdinalIgnoreCaseIgnoreWhiteSpace = 7,
    }

    /// <summary>
    /// This class caches <see cref = "Type"/> instances to avoid use of the typeof operator.
    /// </summary>
    public abstract class Types
    {
        /// <summary>
        /// Gets the <see cref = "FlagsAttribute"/> type.
        /// </summary>
        public static readonly Type FlagsAttributeType = typeof(FlagsAttribute);
    }
}

namespace BrilliantMessaging.GuardClauses.Exceptions
{
    /// <summary>
    /// This exception indicates that a value of a value type is the default value.
    /// </summary>
    [Serializable]
    public class ArgumentDefaultException : ArgumentException
    {
        /// <summary>
        /// Creates a new instance of <see cref = "ArgumentDefaultException"/>.
        /// </summary>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message of the exception (optional).</param>
        public ArgumentDefaultException(string? parameterName = null, string? message = null) : base(message, parameterName)
        {
        }

        /// <inheritdoc/>
        protected ArgumentDefaultException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception indicates that the state of a collection is invalid.
    /// </summary>
    [Serializable]
    public class CollectionException : ArgumentException
    {
        /// <summary>
        /// Creates a new instance of <see cref = "CollectionException"/>.
        /// </summary>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message of the exception (optional).</param>
        public CollectionException(string? parameterName = null, string? message = null) : base(message, parameterName)
        {
        }

        /// <inheritdoc/>
        protected CollectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception indicates that a collection has no items.
    /// </summary>
    [Serializable]
    public class EmptyCollectionException : InvalidCollectionCountException
    {
        /// <summary>
        /// Creates a new instance of <see cref = "EmptyCollectionException"/>.
        /// </summary>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message of the exception (optional).</param>
        public EmptyCollectionException(string? parameterName = null, string? message = null) : base(parameterName, message)
        {
        }

        /// <inheritdoc/>
        protected EmptyCollectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception indicates that that a GUID is empty.
    /// </summary>
    [Serializable]
    public class EmptyGuidException : ArgumentException
    {
        /// <summary>
        /// Creates a new instance of <see cref = "EmptyGuidException"/>.
        /// </summary>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message of the exception (optional).</param>
        public EmptyGuidException(string? parameterName = null, string? message = null) : base(message, parameterName)
        {
        }

        /// <inheritdoc/>
        protected EmptyGuidException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception indicates that a string is empty.
    /// </summary>
    [Serializable]
    public class EmptyStringException : StringException
    {
        /// <summary>
        /// Creates a new instance of <see cref = "EmptyStringException"/>.
        /// </summary>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message of the exception (optional).</param>
        public EmptyStringException(string? parameterName = null, string? message = null) : base(parameterName, message)
        {
        }

        /// <inheritdoc/>
        protected EmptyStringException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception indicates that a value is not defined in the corresponding enum type.
    /// </summary>
    [Serializable]
    public class EnumValueNotDefinedException : ArgumentException
    {
        /// <summary>
        /// Creates a new instance of <see cref = "EnumValueNotDefinedException"/>.
        /// </summary>
        /// <param name = "parameterName">The name of the parameter.</param>
        /// <param name = "message">The message of the exception.</param>
        public EnumValueNotDefinedException(string? parameterName = null, string? message = null) : base(message, parameterName)
        {
        }

        /// <inheritdoc/>
        protected EnumValueNotDefinedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception indicates that a collection contains an item that must not be part of it.
    /// </summary>
    [Serializable]
    public class ExistingItemException : CollectionException
    {
        /// <summary>
        /// Creates a new instance of <see cref = "ExistingItemException"/>.
        /// </summary>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message of the exception (optional).</param>
        public ExistingItemException(string? parameterName = null, string? message = null) : base(parameterName, message)
        {
        }

        /// <inheritdoc/>
        protected ExistingItemException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception indicates that a collection has an invalid number of items.
    /// </summary>
    [Serializable]
    public class InvalidCollectionCountException : CollectionException
    {
        /// <summary>
        /// Creates a new instance of <see cref = "InvalidCollectionCountException"/>.
        /// </summary>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message of the exception (optional).</param>
        public InvalidCollectionCountException(string? parameterName = null, string? message = null) : base(parameterName, message)
        {
        }

        /// <inheritdoc/>
        protected InvalidCollectionCountException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception indicates that a string is not a valid URI.
    /// </summary>
    [Serializable]
    public class InvalidUriException : UriException
    {
        /// <summary>
        /// Creates a new instance of <see cref = "InvalidUriException"/>.
        /// </summary>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message of the exception (optional).</param>
        public InvalidUriException(string? parameterName = null, string? message = null) : base(parameterName, message)
        {
        }

        /// <inheritdoc/>
        protected InvalidUriException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception indicates that a string is in an invalid state.
    /// </summary>
    [Serializable]
    public class StringException : ArgumentException
    {
        /// <summary>
        /// Creates a new instance of <see cref = "StringException"/>.
        /// </summary>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message of the exception (optional).</param>
        public StringException(string? parameterName = null, string? message = null) : base(message, parameterName)
        {
        }

        /// <inheritdoc/>
        protected StringException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception indicates that a string is in an invalid state.
    /// </summary>
    [Serializable]
    public class SubstringException : StringException
    {
        /// <summary>
        /// Creates a new instance of <see cref = "SubstringException"/>.
        /// </summary>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message of the exception (optional).</param>
        public SubstringException(string? parameterName = null, string? message = null) : base(parameterName, message)
        {
        }

        /// <inheritdoc/>
        protected SubstringException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception indicates that a value cannot be cast to another type.
    /// </summary>
    [Serializable]
    public class TypeCastException : ArgumentException
    {
        /// <summary>
        /// Creates a new instance of <see cref = "TypeCastException"/>.
        /// </summary>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message of the exception (optional).</param>
        public TypeCastException(string? parameterName = null, string? message = null) : base(message, parameterName)
        {
        }

        /// <inheritdoc/>
        protected TypeCastException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception indicates that an URI is invalid.
    /// </summary>
    [Serializable]
    public class UriException : ArgumentException
    {
        /// <summary>
        /// Creates a new instance of <see cref = "UriException"/>.
        /// </summary>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message of the exception (optional).</param>
        public UriException(string? parameterName = null, string? message = null) : base(message, parameterName)
        {
        }

        /// <inheritdoc/>
        protected UriException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception indicates that two values are equal.
    /// </summary>
    [Serializable]
    public class ValuesEqualException : ArgumentException
    {
        /// <summary>
        /// Creates a new instance of <see cref = "ValuesEqualException"/>.
        /// </summary>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message of the exception (optional).</param>
        public ValuesEqualException(string? parameterName = null, string? message = null) : base(message, parameterName)
        {
        }

        /// <inheritdoc/>
        protected ValuesEqualException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception indicates that a string contains only white space.
    /// </summary>
    [Serializable]
    public class WhiteSpaceStringException : StringException
    {
        /// <summary>
        /// Creates a new instance of <see cref = "WhiteSpaceStringException"/>.
        /// </summary>
        /// <param name = "parameterName">The name of the parameter (optional).</param>
        /// <param name = "message">The message of the exception (optional).</param>
        public WhiteSpaceStringException(string? parameterName = null, string? message = null) : base(parameterName, message)
        {
        }

        /// <inheritdoc/>
        protected WhiteSpaceStringException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

namespace BrilliantMessaging.GuardClauses.ExceptionFactory
{
    /// <summary>
    /// Provides static factory methods that throw default exceptions.
    /// </summary>
    // ReSharper disable once RedundantTypeDeclarationBody - requried for the Source Code Transformation
    public static class Throw
    {
        /// <summary>
        /// Throws an <see cref = "ArgumentException"/> using the optional parameter name and message.
        /// </summary>
        [DoesNotReturn]
        public static void Argument(string? parameterName = null, string? message = null) => throw new ArgumentException(message ?? $"{parameterName ?? "The value"} is invalid.", parameterName);
        /// <summary>
        /// Throws the default <see cref = "ArgumentDefaultException"/> indicating that a value is the default value of its
        /// type, using the optional parameter name and message.
        /// </summary>
        [DoesNotReturn]
        public static void ArgumentDefault(string? parameterName = null, string? message = null) => throw new ArgumentDefaultException(parameterName, message ?? $"{parameterName ?? "The value"} must not be the default value.");
        /// <summary>
        /// Throws the default <see cref = "ArgumentNullException"/>, using the optional parameter name and message.
        /// </summary>
        [DoesNotReturn]
        public static void ArgumentNull(string? parameterName = null, string? message = null) => throw new ArgumentNullException(parameterName, message ?? $"{parameterName ?? "The value"} must not be null.");
        /// <summary>
        /// Throws the exception that is returned by <paramref name = "exceptionFactory"/>.
        /// </summary>
        [DoesNotReturn]
        public static void CustomException(Func<Exception> exceptionFactory) => throw exceptionFactory.MustNotBeNull(nameof(exceptionFactory))();
        /// <summary>
        /// Throws the exception that is returned by <paramref name = "exceptionFactory"/>. <paramref name = "parameter"/> is
        /// passed to <paramref name = "exceptionFactory"/>.
        /// </summary>
        [DoesNotReturn]
        public static void CustomException<T>(Func<T, Exception> exceptionFactory, T parameter) => throw exceptionFactory.MustNotBeNull(nameof(exceptionFactory))(parameter);
        /// <summary>
        /// Throws the exception that is returned by <paramref name = "exceptionFactory"/>. <paramref name = "first"/> and
        /// <paramref name = "second"/> are passed to <paramref name = "exceptionFactory"/>.
        /// </summary>
        [DoesNotReturn]
        public static void CustomException<T1, T2>(Func<T1, T2, Exception> exceptionFactory, T1 first, T2 second) => throw exceptionFactory.MustNotBeNull(nameof(exceptionFactory))(first, second);
        /// <summary>
        /// Throws the exception that is returned by <paramref name = "exceptionFactory"/>. <paramref name = "span"/> is
        /// passed to <paramref name = "exceptionFactory"/>.
        /// </summary>
        [DoesNotReturn]
        public static void CustomSpanException<TItem>(ReadOnlySpanExceptionFactory<TItem> exceptionFactory, ReadOnlySpan<TItem> span) => throw exceptionFactory.MustNotBeNull(nameof(exceptionFactory))(span);
        /// <summary>
        /// Throws the default <see cref = "EmptyCollectionException"/> indicating that a collection has no items, using the
        /// optional parameter name and message.
        /// </summary>
        [DoesNotReturn]
        public static void EmptyCollection(string? parameterName = null, string? message = null) => throw new EmptyCollectionException(parameterName, message ?? $"{parameterName ?? "The collection"} must not be an empty collection, but it actually is.");
        /// <summary>
        /// Throws the default <see cref = "EmptyGuidException"/> indicating that a GUID is empty, using the optional
        /// parameter name and message.
        /// </summary>
        [DoesNotReturn]
        public static void EmptyGuid(string? parameterName = null, string? message = null) => throw new EmptyGuidException(parameterName, message ?? $"{parameterName ?? "The value"} must be a valid GUID, but it actually is an empty one.");
        /// <summary>
        /// Throws the default <see cref = "EmptyStringException"/> indicating that a string is empty, using the optional
        /// parameter name and message.
        /// </summary>
        [DoesNotReturn]
        public static void EmptyString(string? parameterName = null, string? message = null) => throw new EmptyStringException(parameterName, message ?? $"{parameterName ?? "The string"} must not be an empty string, but it actually is.");
        /// <summary>
        /// Throws the default <see cref = "EnumValueNotDefinedException"/> indicating that a value is not one of the
        /// constants defined in an enum, using the optional parameter name and message.
        /// </summary>
        [DoesNotReturn]
        public static void EnumValueNotDefined<T>(T parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
            where T : Enum => throw new EnumValueNotDefinedException(parameterName, message ?? $"{parameterName ?? "The value"} \"{parameter}\" must be one of the defined constants of enum \"{parameter.GetType()}\", but it actually is not.");
        /// <summary>
        /// Throws an <see cref = "InvalidOperationException"/> using the optional message.
        /// </summary>
        [DoesNotReturn]
        public static void InvalidOperation(string? message = null) => throw new InvalidOperationException(message);
        /// <summary>
        /// Throws the default <see cref = "TypeCastException"/> indicating that a reference cannot be downcast, using the
        /// optional parameter name and message.
        /// </summary>
        [DoesNotReturn]
        public static void InvalidTypeCast(object? parameter, Type targetType, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null) => throw new TypeCastException(parameterName, message ?? $"{parameterName ?? "The value"} {parameter.ToStringOrNull()} cannot be cast to \"{targetType}\".");
        /// <summary>
        /// Throws the default <see cref = "ArgumentException"/> indicating that values of the candidate type cannot be
        /// assigned to variables of the required type, using the optional parameter name and message.
        /// </summary>
        [DoesNotReturn]
        public static void MustBeAssignableTo(Type parameter, Type requiredType, string? parameterName = null, string? message = null) => throw new ArgumentException(message ?? $"Values of type \"{parameter}\" must be assignable to variables of type \"{requiredType}\", but they are not.", parameterName);
        /// <summary>
        /// Throws the default <see cref = "ArgumentException"/> indicating that the specified type is not a concrete class,
        /// using the optional parameter name and message.
        /// </summary>
        [DoesNotReturn]
        public static void MustBeConcreteClass(Type parameter, string? parameterName = null, string? message = null) => throw new ArgumentException(message ?? $"Type \"{parameter}\" must be a non-abstract class, but it is not.", parameterName);
        /// <summary>
        /// Throws the default <see cref = "ArgumentOutOfRangeException"/> indicating that a comparable value must be greater
        /// than or equal to the given boundary value, using the optional parameter name and message.
        /// </summary>
        [DoesNotReturn]
        public static void MustBeGreaterThanOrEqualTo<T>(T parameter, T boundary, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
            where T : IComparable<T> => throw new ArgumentOutOfRangeException(parameterName, message ?? $"{parameterName ?? "The value"} must be greater than or equal to {boundary}, but it actually is {parameter}.");
        /// <summary>
        /// Throws the default <see cref = "ArgumentOutOfRangeException"/> indicating that a numeric value must be positive,
        /// using the optional parameter name and message.
        /// </summary>
        [DoesNotReturn]
        public static void MustBePositive<T>(T parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null) => throw new ArgumentOutOfRangeException(parameterName, message ?? $"{parameterName ?? "The value"} must be positive, but it actually is {parameter}.");
        /// <summary>
        /// Throws the default <see cref = "ArgumentOutOfRangeException"/> indicating that a <see cref = "TimeSpan"/> must
        /// be positive or equal to <see cref = "System.Threading.Timeout.InfiniteTimeSpan"/>, using the optional parameter
        /// name and message.
        /// </summary>
        [DoesNotReturn]
        public static void MustBePositiveOrInfinite(TimeSpan parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null) => throw new ArgumentOutOfRangeException(parameterName, message ?? $"{parameterName ?? "The value"} must be positive or equal to Timeout.InfiniteTimeSpan, but it actually is {parameter}.");
        /// <summary>
        /// Throws the default <see cref = "ArgumentOutOfRangeException"/> indicating that a comparable value must not be
        /// less than the given boundary value, using the optional parameter name and message.
        /// </summary>
        [DoesNotReturn]
        public static void MustNotBeLessThan<T>(T parameter, T boundary, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
            where T : IComparable<T> => throw new ArgumentOutOfRangeException(parameterName, message ?? $"{parameterName ?? "The value"} must not be less than {boundary}, but it actually is {parameter}.");
        /// <summary>
        /// Throws the default <see cref = "ArgumentOutOfRangeException"/> indicating that a numeric value must not be
        /// negative, using the optional parameter name and message.
        /// </summary>
        [DoesNotReturn]
        public static void MustNotBeNegative<T>(T parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null) => throw new ArgumentOutOfRangeException(parameterName, message ?? $"{parameterName ?? "The value"} must not be negative, but it actually is {parameter}.");
        /// <summary>
        /// Throws the default <see cref = "ExistingItemException"/> indicating that a collection contains a null item.
        /// </summary>
        [DoesNotReturn]
        public static void NullItem(int position, string? parameterName = null, string? message = null) => throw new ExistingItemException(parameterName, message ?? $"{parameterName ?? "The collection"} must not contain null items, but a null item was found at position {position}.");
        /// <summary>
        /// Throws an <see cref = "ObjectDisposedException"/> using the optional object name and message.
        /// </summary>
        [DoesNotReturn]
        public static void ObjectDisposed(string? objectName = null, string? message = null) => throw new ObjectDisposedException(objectName, message);
        /// <summary>
        /// Throws the default <see cref = "ArgumentOutOfRangeException"/> indicating that a value is not within a specified
        /// range, using the optional parameter name and message.
        /// </summary>
        [DoesNotReturn]
        public static void MustBeInRange<T>(T parameter, Range<T> range, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null)
            where T : IComparable<T> => throw new ArgumentOutOfRangeException(parameterName, message ?? $"{parameterName ?? "The value"} must be between {range.CreateRangeDescriptionText("and")}, but it actually is {parameter}.");
        /// <summary>
        /// Throws the default <see cref = "SubstringException"/> indicating that a string does start with another one, using
        /// the optional parameter name and message.
        /// </summary>
        [DoesNotReturn]
        public static void StringStartsWith(string parameter, string other, StringComparison comparisonType, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null) => throw new SubstringException(parameterName, message ?? $"{parameterName ?? "The string"} must not start with \"{other}\" ({comparisonType}), but it actually is {parameter.ToStringOrNull()}.");
        /// <summary>
        /// Throws the default <see cref = "SubstringException"/> indicating that a string does start with another one, using
        /// the optional parameter name and message.
        /// </summary>
        [DoesNotReturn]
        public static void StringStartsWith(ReadOnlySpan<char> parameter, ReadOnlySpan<char> other, StringComparison comparisonType, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null) => throw new SubstringException(parameterName, message ?? $"{parameterName ?? "The string"} must not start with \"{other.ToString()}\" ({comparisonType}), but it actually is {parameter.ToString()}.");
        /// <summary>
        /// Throws the default <see cref = "InvalidUriException"/> indicating that a string is not a valid URI of the
        /// supplied kind, using the optional parameter name and message.
        /// </summary>
        [DoesNotReturn]
        public static void MustBeUri(string parameter, UriKind uriKind, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null) => throw new InvalidUriException(parameterName, message ?? $"{parameterName ?? "The string"} must be a valid URI ({uriKind}), but it actually is \"{parameter}\".");
        /// <summary>
        /// Throws the default <see cref = "ValuesEqualException"/> indicating that two values are equal, using the optional
        /// parameter name and message.
        /// </summary>
        [DoesNotReturn]
        public static void ValuesEqual<T>(T parameter, T other, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null) => throw new ValuesEqualException(parameterName, message ?? $"{parameterName ?? "The value"} must not be equal to {other.ToStringOrNull()}, but it actually is {parameter.ToStringOrNull()}.");
        /// <summary>
        /// Throws the default <see cref = "WhiteSpaceStringException"/> indicating that a string contains only white space,
        /// using the optional parameter name and message.
        /// </summary>
        [DoesNotReturn]
        public static void WhiteSpaceString(string parameter, [CallerArgumentExpression("parameter")] string? parameterName = null, string? message = null) => throw new WhiteSpaceStringException(parameterName, message ?? $"{parameterName ?? "The string"} must not contain only white space, but it actually is \"{parameter}\".");
    }

    /// <summary>
    /// Represents a delegate that receives a read-only span and produces an exception.
    /// </summary>
    public delegate Exception ReadOnlySpanExceptionFactory<TItem>(ReadOnlySpan<TItem> span);
}

namespace BrilliantMessaging.GuardClauses.FrameworkExtensions
{
    /// <summary>
    /// Provides extension methods for the <see cref = "IEnumerable{T}"/> interface.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Tries to cast the specified enumerable to an <see cref = "IList{T}"/>, or
        /// creates a new <see cref = "List{T}"/> containing the enumerable items.
        /// </summary>
        /// <typeparam name = "T">The item type of the enumerable.</typeparam>
        /// <param name = "source">The enumerable to be transformed.</param>
        /// <returns>The list containing the items of the enumerable.</returns>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "source"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once RedundantNullableFlowAttribute -- NotNull has an effect, see Issue72NotNullAttributeTests
        public static IList<T> AsList<T>([NotNull] this IEnumerable<T> source) => source as IList<T> ?? source.ToList();
        /// <summary>
        /// Tries to cast the specified enumerable to an <see cref = "IList{T}"/>, or
        /// creates a new collection containing the enumerable items by calling the specified delegate.
        /// </summary>
        /// <typeparam name = "T">The item type of the collection.</typeparam>
        /// <param name = "source">The enumerable that will be converted to <see cref = "IList{T}"/>.</param>
        /// <param name = "createCollection">The delegate that creates the collection containing the specified items.</param>
        /// <returns>The cast enumerable, or a new collection containing the enumerable items.</returns>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "source"/> or <paramref name = "createCollection"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<T> AsList<T>(// ReSharper disable once RedundantNullableFlowAttribute -- NotNull has an effect, see Issue72NotNullAttributeTests
        [NotNull] this IEnumerable<T> source, Func<IEnumerable<T>, IList<T>> createCollection) => source as IList<T> ?? createCollection.MustNotBeNull(nameof(createCollection))(source.MustNotBeNull(nameof(source)));
        /// <summary>
        /// Tries to downcast the specified enumerable to an array, or creates a new array with the specified items.
        /// </summary>
        /// <typeparam name = "T">The item type of the collection.</typeparam>
        /// <param name = "source">The enumerable that will be converted to an array.</param>
        /// <returns>The cast array, or a new array containing the enumerable items.</returns>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "source"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once RedundantNullableFlowAttribute -- NotNull has an effect, see Issue72NotNullAttributeTests
        public static T[] AsArray<T>([NotNull] this IEnumerable<T> source) => source as T[] ?? source.ToArray();
        /// <summary>
        /// Performs the action on each item of the specified enumerable. If the enumerable contains items that are null, this
        /// method can either throw an exception or ignore the value (your delegate will not be called in this case).
        /// </summary>
        /// <typeparam name = "T">The item type of the enumerable.</typeparam>
        /// <param name = "enumerable">The collection containing the items that will be passed to the action.</param>
        /// <param name = "action">The action that executes for each item of the collection.</param>
        /// <param name = "throwWhenItemIsNull">The value indicating whether this method should throw a <see cref = "CollectionException"/> when any of the items is null (optional). Defaults to true.</param>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "enumerable"/> or <paramref name = "action"/> is null.</exception>
        /// <exception cref = "CollectionException">Thrown when <paramref name = "enumerable"/> contains a value that is null and <paramref name = "throwWhenItemIsNull"/> is set to true.</exception>
        public static IEnumerable<T> ForEach<T>(// ReSharper disable once RedundantNullableFlowAttribute -- NotNull has an effect, see Issue72NotNullAttributeTests
        [NotNull] this IEnumerable<T> enumerable, Action<T> action, bool throwWhenItemIsNull = true)
        {
            // ReSharper disable PossibleMultipleEnumeration
            action.MustNotBeNull(nameof(action));
            var i = 0;
            if (enumerable is IList<T> list)
            {
                for (; i < list.Count; i++)
                {
                    var item = list[i];
                    if (item is null)
                    {
                        if (throwWhenItemIsNull)
                        {
                            throw new CollectionException(nameof(enumerable), $"The collection contains null at index {i}.");
                        }

                        continue;
                    }

                    action(item);
                }
            }
            else
            {
                foreach (var item in enumerable.MustNotBeNull(nameof(enumerable)))
                {
                    if (item is null)
                    {
                        if (throwWhenItemIsNull)
                        {
                            throw new CollectionException(nameof(enumerable), $"The collection contains null at index {i}.");
                        }

                        ++i;
                        continue;
                    }

                    action(item);
                    ++i;
                }
            }

            return enumerable;
        // ReSharper restore PossibleMultipleEnumeration
        }

        /// <summary>
        /// Tries to cast the specified enumerable as an <see cref = "IReadOnlyList{T}"/>, or
        /// creates a new <see cref = "List{T}"/> containing the enumerable items.
        /// </summary>
        /// <typeparam name = "T">The item type of the enumerable.</typeparam>
        /// <param name = "source">The enumerable to be transformed.</param>
        /// <returns>The list containing the items of the enumerable.</returns>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "source"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once RedundantNullableFlowAttribute -- NotNull has an effect, see Issue72NotNullAttributeTests
        public static IReadOnlyList<T> AsReadOnlyList<T>([NotNull] this IEnumerable<T> source) => source as IReadOnlyList<T> ?? source.ToList();
        /// <summary>
        /// Tries to cast the specified enumerable as an <see cref = "IReadOnlyList{T}"/>, or
        /// creates a new collection containing the enumerable items by calling the specified delegate.
        /// </summary>
        /// <typeparam name = "T">The item type of the collection.</typeparam>
        /// <param name = "source">The enumerable that will be converted to <see cref = "IReadOnlyList{T}"/>.</param>
        /// <param name = "createCollection">The delegate that creates the collection containing the specified items.</param>
        /// <returns>The cast enumerable, or a new collection containing the enumerable items.</returns>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "source"/> or <paramref name = "createCollection"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable RedundantNullableFlowAttribute -- NotNull has an effect, see Issue72NotNullAttributeTests
        public static IReadOnlyList<T> AsReadOnlyList<T>([NotNull] this IEnumerable<T> source, [NotNull] Func<IEnumerable<T>, IReadOnlyList<T>> createCollection) => source as IReadOnlyList<T> ?? createCollection.MustNotBeNull(nameof(createCollection))(source.MustNotBeNull(nameof(source)));
        // ReSharper restore RedundantNullableFlowAttribute
        /// <summary>
        /// Gets the count of the specified enumerable.
        /// </summary>
        /// <param name = "enumerable">The enumerable whose count should be determined.</param>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "enumerable"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once RedundantNullableFlowAttribute -- NotNull has an effect, see Issue72NotNullAttributeTests
        public static int Count([NotNull] this IEnumerable enumerable)
        {
            if (enumerable is ICollection collection)
            {
                return collection.Count;
            }

            if (enumerable is string @string)
            {
                return @string.Length;
            }

            return DetermineCountViaEnumerating(enumerable);
        }

        /// <summary>
        /// Gets the count of the specified enumerable.
        /// </summary>
        /// <param name = "enumerable">The enumerable whose count should be determined.</param>
        /// <param name = "parameterName">The name of the parameter that is passed to the <see cref = "ArgumentNullException"/> (optional).</param>
        /// <param name = "message">The message that is passed to the <see cref = "ArgumentNullException"/> (optional).</param>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "enumerable"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count([NotNull] this IEnumerable? enumerable, string? parameterName, string? message)
        {
            if (enumerable is ICollection collection)
            {
                return collection.Count;
            }

            if (enumerable is string @string)
            {
                return @string.Length;
            }

            return DetermineCountViaEnumerating(enumerable, parameterName, message);
        }

        /// <summary>
        /// Gets the count of the specified enumerable.
        /// </summary>
        /// <param name = "enumerable">The enumerable whose count should be determined.</param>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "enumerable"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable RedundantNullableFlowAttribute -- NotNull has an effect, see Issue72NotNullAttributeTests
        public static int GetCount<T>([NotNull] this IEnumerable<T> enumerable)
        {
            if (enumerable is ICollection collection)
            {
                return collection.Count;
            }

            if (enumerable is string @string)
            {
                return @string.Length;
            }

            if (TryGetCollectionOfTCount(enumerable, out var count))
            {
                return count;
            }

            return DetermineCountViaEnumerating(enumerable);
        }

        /// <summary>
        /// Gets the count of the specified enumerable.
        /// </summary>
        /// <param name = "enumerable">The enumerable whose count should be determined.</param>
        /// <param name = "parameterName">The name of the parameter that is passed to the <see cref = "ArgumentNullException"/> (optional).</param>
        /// <param name = "message">The message that is passed to the <see cref = "ArgumentNullException"/> (optional).</param>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "enumerable"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetCount<T>([NotNull] this IEnumerable<T> enumerable, string? parameterName, string? message = null)
        {
            if (enumerable is ICollection collection)
            {
                return collection.Count;
            }

            if (enumerable is string @string)
            {
                return @string.Length;
            }

            if (TryGetCollectionOfTCount(enumerable, out var count))
            {
                return count;
            }

            return DetermineCountViaEnumerating(enumerable, parameterName, message);
        }

        private static bool TryGetCollectionOfTCount<T>([NoEnumeration] this IEnumerable<T> enumerable, out int count)
        {
            if (enumerable is ICollection<T> collectionOfT)
            {
                count = collectionOfT.Count;
                return true;
            }

            if (enumerable is IReadOnlyCollection<T> readOnlyCollection)
            {
                count = readOnlyCollection.Count;
                return true;
            }

            count = 0;
            return false;
        }

        private static int DetermineCountViaEnumerating(IEnumerable? enumerable)
        {
            var count = 0;
            var enumerator = enumerable.MustNotBeNull(nameof(enumerable)).GetEnumerator();
            while (enumerator.MoveNext())
            {
                count++;
            }

            if (enumerator is IDisposable disposable)
            {
                disposable.Dispose();
            }

            return count;
        }

        private static int DetermineCountViaEnumerating([NotNull] IEnumerable? enumerable, string? parameterName, string? message)
        {
            var count = 0;
            var enumerator = enumerable.MustNotBeNull(parameterName, message).GetEnumerator();
            while (enumerator.MoveNext())
            {
                count++;
            }

            if (enumerator is IDisposable disposable)
            {
                disposable.Dispose();
            }

            return count;
        }

        internal static bool ContainsViaForeach<TItem>(this IEnumerable<TItem> items, TItem item)
        {
            var equalityComparer = EqualityComparer<TItem>.Default;
            foreach (var i in items)
            {
                if (equalityComparer.Equals(i, item))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// The <see cref = "MultiplyAddHash"/> class represents a simple non-cryptographic hash function that uses a prime number
    /// as seed and then manipulates this value by constantly performing <c>hash = unchecked(hash * SecondPrime + value?.GetHashCode() ?? 0);</c>
    /// for each given value. It is implemented according to the guidelines of Jon Skeet as stated in this Stack Overflow
    /// answer: http://stackoverflow.com/a/263416/1560623. IMPORTANT: do not persist any hash codes and rely on them
    /// to stay the same. Hash codes should only be used in memory within a single process session, usually for the use
    /// in dictionaries (hash tables) and sets. This algorithm, especially the prime numbers can change even during minor
    /// releases of Light.GuardClauses.
    /// </summary>
    public static class MultiplyAddHash
    {
        /// <summary>
        /// This prime number is used as an initial (seed) value when calculating hash codes. Its value is 1322837333.
        /// </summary>
        public const int FirstPrime = 1322837333;
        /// <summary>
        /// The second prime number (397) used for hash code generation. It is applied using the following statement:
        /// <c>hash = unchecked(hash * SecondPrime + value?.GetHashCode() ?? 0);</c>.
        /// It is the same value that ReSharper (2018.1) uses for hash code generation.
        /// </summary>
        public const int SecondPrime = 397;
        /// <summary>
        /// Creates a hash code from the two specified values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CreateHashCode<T1, T2>(T1 value1, T2 value2)
        {
            var hash = FirstPrime;
            CombineIntoHash(ref hash, value1);
            CombineIntoHash(ref hash, value2);
            return hash;
        }

        /// <summary>
        /// Creates a hash code from the three specified values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CreateHashCode<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
        {
            var hash = FirstPrime;
            CombineIntoHash(ref hash, value1);
            CombineIntoHash(ref hash, value2);
            CombineIntoHash(ref hash, value3);
            return hash;
        }

        /// <summary>
        /// Creates a hash code from the four specified values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CreateHashCode<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
        {
            var hash = FirstPrime;
            CombineIntoHash(ref hash, value1);
            CombineIntoHash(ref hash, value2);
            CombineIntoHash(ref hash, value3);
            CombineIntoHash(ref hash, value4);
            return hash;
        }

        /// <summary>
        /// Creates a hash code from the five specified values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CreateHashCode<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
        {
            var hash = FirstPrime;
            CombineIntoHash(ref hash, value1);
            CombineIntoHash(ref hash, value2);
            CombineIntoHash(ref hash, value3);
            CombineIntoHash(ref hash, value4);
            CombineIntoHash(ref hash, value5);
            return hash;
        }

        /// <summary>
        /// Creates a hash code from the six specified values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CreateHashCode<T1, T2, T3, T4, T5, T6>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
        {
            var hash = FirstPrime;
            CombineIntoHash(ref hash, value1);
            CombineIntoHash(ref hash, value2);
            CombineIntoHash(ref hash, value3);
            CombineIntoHash(ref hash, value4);
            CombineIntoHash(ref hash, value5);
            CombineIntoHash(ref hash, value6);
            return hash;
        }

        /// <summary>
        /// Creates a hash code from the seven specified values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CreateHashCode<T1, T2, T3, T4, T5, T6, T7>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
        {
            var hash = FirstPrime;
            CombineIntoHash(ref hash, value1);
            CombineIntoHash(ref hash, value2);
            CombineIntoHash(ref hash, value3);
            CombineIntoHash(ref hash, value4);
            CombineIntoHash(ref hash, value5);
            CombineIntoHash(ref hash, value6);
            CombineIntoHash(ref hash, value7);
            return hash;
        }

        /// <summary>
        /// Creates a hash code from the eight specified values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CreateHashCode<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8)
        {
            var hash = FirstPrime;
            CombineIntoHash(ref hash, value1);
            CombineIntoHash(ref hash, value2);
            CombineIntoHash(ref hash, value3);
            CombineIntoHash(ref hash, value4);
            CombineIntoHash(ref hash, value5);
            CombineIntoHash(ref hash, value6);
            CombineIntoHash(ref hash, value7);
            CombineIntoHash(ref hash, value8);
            return hash;
        }

        /// <summary>
        /// Creates a hash code from the nine specified values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CreateHashCode<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9)
        {
            var hash = FirstPrime;
            CombineIntoHash(ref hash, value1);
            CombineIntoHash(ref hash, value2);
            CombineIntoHash(ref hash, value3);
            CombineIntoHash(ref hash, value4);
            CombineIntoHash(ref hash, value5);
            CombineIntoHash(ref hash, value6);
            CombineIntoHash(ref hash, value7);
            CombineIntoHash(ref hash, value8);
            CombineIntoHash(ref hash, value9);
            return hash;
        }

        /// <summary>
        /// Creates a hash code from the ten specified values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CreateHashCode<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10)
        {
            var hash = FirstPrime;
            CombineIntoHash(ref hash, value1);
            CombineIntoHash(ref hash, value2);
            CombineIntoHash(ref hash, value3);
            CombineIntoHash(ref hash, value4);
            CombineIntoHash(ref hash, value5);
            CombineIntoHash(ref hash, value6);
            CombineIntoHash(ref hash, value7);
            CombineIntoHash(ref hash, value8);
            CombineIntoHash(ref hash, value9);
            CombineIntoHash(ref hash, value10);
            return hash;
        }

        /// <summary>
        /// Creates a hash code from the eleven specified values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CreateHashCode<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11)
        {
            var hash = FirstPrime;
            CombineIntoHash(ref hash, value1);
            CombineIntoHash(ref hash, value2);
            CombineIntoHash(ref hash, value3);
            CombineIntoHash(ref hash, value4);
            CombineIntoHash(ref hash, value5);
            CombineIntoHash(ref hash, value6);
            CombineIntoHash(ref hash, value7);
            CombineIntoHash(ref hash, value8);
            CombineIntoHash(ref hash, value9);
            CombineIntoHash(ref hash, value10);
            CombineIntoHash(ref hash, value11);
            return hash;
        }

        /// <summary>
        /// Creates a hash code from the eleven specified values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CreateHashCode<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12)
        {
            var hash = FirstPrime;
            CombineIntoHash(ref hash, value1);
            CombineIntoHash(ref hash, value2);
            CombineIntoHash(ref hash, value3);
            CombineIntoHash(ref hash, value4);
            CombineIntoHash(ref hash, value5);
            CombineIntoHash(ref hash, value6);
            CombineIntoHash(ref hash, value7);
            CombineIntoHash(ref hash, value8);
            CombineIntoHash(ref hash, value9);
            CombineIntoHash(ref hash, value10);
            CombineIntoHash(ref hash, value11);
            CombineIntoHash(ref hash, value12);
            return hash;
        }

        /// <summary>
        /// Creates a hash code from the thirteen specified values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CreateHashCode<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12, T13 value13)
        {
            var hash = FirstPrime;
            CombineIntoHash(ref hash, value1);
            CombineIntoHash(ref hash, value2);
            CombineIntoHash(ref hash, value3);
            CombineIntoHash(ref hash, value4);
            CombineIntoHash(ref hash, value5);
            CombineIntoHash(ref hash, value6);
            CombineIntoHash(ref hash, value7);
            CombineIntoHash(ref hash, value8);
            CombineIntoHash(ref hash, value9);
            CombineIntoHash(ref hash, value10);
            CombineIntoHash(ref hash, value11);
            CombineIntoHash(ref hash, value12);
            CombineIntoHash(ref hash, value13);
            return hash;
        }

        /// <summary>
        /// Creates a hash code from the fourteen specified values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CreateHashCode<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12, T13 value13, T14 value14)
        {
            var hash = FirstPrime;
            CombineIntoHash(ref hash, value1);
            CombineIntoHash(ref hash, value2);
            CombineIntoHash(ref hash, value3);
            CombineIntoHash(ref hash, value4);
            CombineIntoHash(ref hash, value5);
            CombineIntoHash(ref hash, value6);
            CombineIntoHash(ref hash, value7);
            CombineIntoHash(ref hash, value8);
            CombineIntoHash(ref hash, value9);
            CombineIntoHash(ref hash, value10);
            CombineIntoHash(ref hash, value11);
            CombineIntoHash(ref hash, value12);
            CombineIntoHash(ref hash, value13);
            CombineIntoHash(ref hash, value14);
            return hash;
        }

        /// <summary>
        /// Creates a hash code from the fifteen specified values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CreateHashCode<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12, T13 value13, T14 value14, T15 value15)
        {
            var hash = FirstPrime;
            CombineIntoHash(ref hash, value1);
            CombineIntoHash(ref hash, value2);
            CombineIntoHash(ref hash, value3);
            CombineIntoHash(ref hash, value4);
            CombineIntoHash(ref hash, value5);
            CombineIntoHash(ref hash, value6);
            CombineIntoHash(ref hash, value7);
            CombineIntoHash(ref hash, value8);
            CombineIntoHash(ref hash, value9);
            CombineIntoHash(ref hash, value10);
            CombineIntoHash(ref hash, value11);
            CombineIntoHash(ref hash, value12);
            CombineIntoHash(ref hash, value13);
            CombineIntoHash(ref hash, value14);
            CombineIntoHash(ref hash, value15);
            return hash;
        }

        /// <summary>
        /// Creates a hash code from the sixteen specified values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CreateHashCode<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12, T13 value13, T14 value14, T15 value15, T16 value16)
        {
            var hash = FirstPrime;
            CombineIntoHash(ref hash, value1);
            CombineIntoHash(ref hash, value2);
            CombineIntoHash(ref hash, value3);
            CombineIntoHash(ref hash, value4);
            CombineIntoHash(ref hash, value5);
            CombineIntoHash(ref hash, value6);
            CombineIntoHash(ref hash, value7);
            CombineIntoHash(ref hash, value8);
            CombineIntoHash(ref hash, value9);
            CombineIntoHash(ref hash, value10);
            CombineIntoHash(ref hash, value11);
            CombineIntoHash(ref hash, value12);
            CombineIntoHash(ref hash, value13);
            CombineIntoHash(ref hash, value14);
            CombineIntoHash(ref hash, value15);
            CombineIntoHash(ref hash, value16);
            return hash;
        }

        /// <summary>
        /// Mutates the given hash with the specified value using the following statement:
        /// <c>hash = unchecked(hash * SecondPrime + value?.GetHashCode() ?? 0);</c>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CombineIntoHash<T>(ref int hash, T value) => hash = unchecked(hash * SecondPrime + value?.GetHashCode() ?? 0);
    }

    /// <summary>
    /// Provides extension methods for <see cref = "string "/> and <see cref = "StringBuilder"/> to easily assembly error messages.
    /// </summary>
    public static class TextExtensions
    {
        /// <summary>
        /// Gets the default NewLineSeparator. This value is $",{Environment.NewLine}".
        /// </summary>
        public static readonly string DefaultNewLineSeparator = ',' + Environment.NewLine;
        /// <summary>
        /// Gets the list of types that will not be surrounded by quotation marks in error messages.
        /// </summary>
        public static readonly ReadOnlyCollection<Type> UnquotedTypes = new([typeof(int), typeof(long), typeof(short), typeof(sbyte), typeof(uint), typeof(ulong), typeof(ushort), typeof(byte), typeof(bool), typeof(double), typeof(decimal), typeof(float), ]);
        private static bool IsUnquotedType<T>()
        {
            if (typeof(T) == typeof(int))
            {
                return true;
            }

            if (typeof(T) == typeof(long))
            {
                return true;
            }

            if (typeof(T) == typeof(short))
            {
                return true;
            }

            if (typeof(T) == typeof(sbyte))
            {
                return true;
            }

            if (typeof(T) == typeof(uint))
            {
                return true;
            }

            if (typeof(T) == typeof(ulong))
            {
                return true;
            }

            if (typeof(T) == typeof(ushort))
            {
                return true;
            }

            if (typeof(T) == typeof(byte))
            {
                return true;
            }

            if (typeof(T) == typeof(bool))
            {
                return true;
            }

            if (typeof(T) == typeof(double))
            {
                return true;
            }

            if (typeof(T) == typeof(decimal))
            {
                return true;
            }

            if (typeof(T) == typeof(float))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the string representation of <paramref name = "value"/>, or <paramref name = "nullText"/> if <paramref name = "value"/> is null.
        /// If the type of <paramref name = "value"/> is not one of <see cref = "UnquotedTypes"/>, then quotation marks will be put around the string representation.
        /// </summary>
        /// <param name = "value">The item whose string representation should be returned.</param>
        /// <param name = "nullText">The text that is returned when <paramref name = "value"/> is null (defaults to "null").</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToStringOrNull<T>(this T value, string nullText = "null") => value?.ToStringRepresentation() ?? nullText;
        /// <summary>
        /// Returns the string representation of <paramref name = "value"/>. This is done by calling <see cref = "object.ToString"/>. If the type of
        /// <paramref name = "value"/> is not one of <see cref = "UnquotedTypes"/>, then the resulting string will be wrapped in quotation marks.
        /// </summary>
        /// <param name = "value">The value whose string representation is requested.</param>
        public static string? ToStringRepresentation<T>([NotNull] this T value)
        {
            value.MustNotBeNullReference(nameof(value));
            var content = value.ToString();
            if (IsUnquotedType<T>() || content.IsNullOrEmpty())
            {
                return content;
            }

            // ReSharper disable UseIndexFromEndExpression -- not possible in netstandard2.0
            if (content.Length <= 126)
            {
                Span<char> span = stackalloc char[content.Length + 2];
                span[0] = span[span.Length - 1] = '"';
                content.AsSpan().CopyTo(span.Slice(1, content.Length));
                return span.ToString();
            }

            var contentWithQuotationMarks = new char[content.Length + 2];
            contentWithQuotationMarks[0] = contentWithQuotationMarks[contentWithQuotationMarks.Length - 1] = '"';
            // ReSharper restore UseIndexFromEndExpression
            content.CopyTo(0, contentWithQuotationMarks, 1, content.Length);
            return new string (contentWithQuotationMarks);
        }

        /// <summary>
        /// Appends the content of the collection with the specified header line to the string builder.
        /// Each item is on a new line.
        /// </summary>
        /// <typeparam name = "T">The item type of the collection.</typeparam>
        /// <param name = "stringBuilder">The string builder that the content is appended to.</param>
        /// <param name = "items">The collection whose items will be appended to the string builder.</param>
        /// <param name = "headerLine">The string that will be placed before the actual items as a header.</param>
        /// <param name = "finishWithNewLine">The value indicating if a new line is added after the last item. This value defaults to true.</param>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "stringBuilder"/> or <paramref name = "items"/>is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable RedundantNullableFlowAttribute
        public static StringBuilder AppendCollectionContent<T>([NotNull] this StringBuilder stringBuilder, [NotNull] IEnumerable<T> items, string headerLine = "Content of the collection:", bool finishWithNewLine = true) => stringBuilder.MustNotBeNull(nameof(stringBuilder)).AppendLine(headerLine).AppendItemsWithNewLine(items, finishWithNewLine: finishWithNewLine);
        // ReSharper restore RedundantNullableFlowAttribute
        /// <summary>
        /// Appends the string representations of the specified items to the string builder.
        /// </summary>
        /// <param name = "stringBuilder">The string builder where the items will be appended to.</param>
        /// <param name = "items">The items to be appended.</param>
        /// <param name = "itemSeparator">The characters used to separate the items. Defaults to ", " and is not appended after the last item.</param>
        /// <param name = "emptyCollectionText">The text that is appended to the string builder when <paramref name = "items"/> is empty. Defaults to "empty collection".</param>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "stringBuilder"/> or <paramref name = "items"/> is null.</exception>
        // ReSharper disable RedundantNullableFlowAttribute
        public static StringBuilder AppendItems<T>([NotNull] this StringBuilder stringBuilder, [NotNull] IEnumerable<T> items, string itemSeparator = ", ", string emptyCollectionText = "empty collection")
        // ReSharper restore RedundantNullableFlowAttribute
        {
            stringBuilder.MustNotBeNull(nameof(stringBuilder));
            var list = items.MustNotBeNull(nameof(items)).AsList();
            var currentIndex = 0;
            var itemsCount = list.Count;
            if (itemsCount == 0)
            {
                return stringBuilder.Append(emptyCollectionText);
            }

            while (true)
            {
                stringBuilder.Append(list[currentIndex].ToStringOrNull());
                if (currentIndex < itemsCount - 1)
                {
                    stringBuilder.Append(itemSeparator);
                }
                else
                {
                    return stringBuilder;
                }

                ++currentIndex;
            }
        }

        /// <summary>
        /// Appends the string representations of the specified items to the string builder. Each item is on its own line.
        /// </summary>
        /// <param name = "stringBuilder">The string builder where the items will be appended to.</param>
        /// <param name = "items">The items to be appended.</param>
        /// <param name = "emptyCollectionText">The text that is appended to the string builder when <paramref name = "items"/> is empty. Defaults to "empty collection".</param>
        /// <param name = "finishWithNewLine">The value indicating if a new line is added after the last item. This value defaults to true.</param>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "stringBuilder"/> or <paramref name = "items"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable RedundantNullableFlowAttribute
        public static StringBuilder AppendItemsWithNewLine<T>([NotNull] this StringBuilder stringBuilder, [NotNull] IEnumerable<T> items, string emptyCollectionText = "empty collection", bool finishWithNewLine = true) => stringBuilder.AppendItems(items, DefaultNewLineSeparator, emptyCollectionText).AppendLineIf(finishWithNewLine);
        // ReSharper restore RedundantNullableFlowAttribute
        /// <summary>
        /// Appends the value to the specified string builder if the condition is true.
        /// </summary>
        /// <param name = "stringBuilder">The string builder where <paramref name = "value"/> will be appended to.</param>
        /// <param name = "condition">The boolean value indicating whether the append operation will be performed or not.</param>
        /// <param name = "value">The value to be appended to the string builder.</param>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "stringBuilder"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendIf(// ReSharper disable once RedundantNullableFlowAttribute
        [NotNull] this StringBuilder stringBuilder, bool condition, string value)
        {
            if (condition)
            {
                stringBuilder.MustNotBeNull(nameof(stringBuilder)).Append(value);
            }

            return stringBuilder;
        }

        /// <summary>
        /// Appends the value followed by a new line separator to the specified string builder if the condition is true.
        /// </summary>
        /// <param name = "stringBuilder">The string builder where <paramref name = "value"/> will be appended to.</param>
        /// <param name = "condition">The boolean value indicating whether the append operation will be performed or not.</param>
        /// <param name = "value">The value to be appended to the string builder (optional). This value defaults to an empty string.</param>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "stringBuilder"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendLineIf(// ReSharper disable once RedundantNullableFlowAttribute
        [NotNull] this StringBuilder stringBuilder, bool condition, string value = "")
        {
            if (condition)
            {
                stringBuilder.MustNotBeNull(nameof(stringBuilder)).AppendLine(value);
            }

            return stringBuilder;
        }

        /// <summary>
        /// Appends the messages of the <paramref name = "exception"/> and its nested exceptions to the
        /// specified <paramref name = "stringBuilder"/>.
        /// </summary>
        /// <exception cref = "ArgumentNullException">Thrown when any parameter is null.</exception>
        // ReSharper disable RedundantNullableFlowAttribute
        public static StringBuilder AppendExceptionMessages([NotNull] this StringBuilder stringBuilder, [NotNull] Exception exception)
        // ReSharper restore RedundantNullableFlowAttribute
        {
            stringBuilder.MustNotBeNull(nameof(stringBuilder));
            exception.MustNotBeNull(nameof(exception));
            while (true)
            {
                // ReSharper disable once PossibleNullReferenceException
                stringBuilder.AppendLine(exception.Message);
                if (exception.InnerException is null)
                {
                    return stringBuilder;
                }

                stringBuilder.AppendLine();
                exception = exception.InnerException;
            }
        }

        /// <summary>
        /// Formats all messages of the <paramref name = "exception"/> and its nested exceptions into
        /// a single string.
        /// </summary>
        /// <exception cref = "ArgumentNullException">Thrown when <paramref name = "exception"/> is null.</exception>
        // ReSharper disable once RedundantNullableFlowAttribute
        public static string GetAllExceptionMessages([NotNull] this Exception exception) => new StringBuilder().AppendExceptionMessages(exception).ToString();
        /// <summary>
        /// Checks if the two strings are equal using ordinal sorting rules as well as ignoring the white space
        /// of the provided strings.
        /// </summary>
        public static bool EqualsOrdinalIgnoreWhiteSpace(this string? x, string? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            if (x.Length == 0)
            {
                return y.Length == 0;
            }

            var indexX = 0;
            var indexY = 0;
            bool wasXSuccessful;
            bool wasYSuccessful;
            // This condition of the while loop actually has to use the single '&' operator because
            // y.TryAdvanceToNextNonWhiteSpaceCharacter must be called even though it already returned
            // false on x. Otherwise, the 'wasXSuccessful == wasYSuccessful' comparison would not return
            // the desired result.
            while ((wasXSuccessful = x.TryAdvanceToNextNonWhiteSpaceCharacter(ref indexX)) & (wasYSuccessful = y.TryAdvanceToNextNonWhiteSpaceCharacter(ref indexY)))
            {
                if (x[indexX++] != y[indexY++])
                {
                    return false;
                }
            }

            return wasXSuccessful == wasYSuccessful;
        }

        /// <summary>
        /// Checks if the two strings are equal using ordinal sorting rules, ignoring the case of the letters
        /// as well as ignoring the white space of the provided strings.
        /// </summary>
        public static bool EqualsOrdinalIgnoreCaseIgnoreWhiteSpace(this string? x, string? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            if (x.Length == 0)
            {
                return y.Length == 0;
            }

            var indexX = 0;
            var indexY = 0;
            bool wasXSuccessful;
            bool wasYSuccessful;
            // This condition of the while loop actually has to use the single '&' operator because
            // y.TryAdvanceToNextNonWhiteSpaceCharacter must be called even though it already returned
            // false on x. Otherwise, the 'wasXSuccessful == wasYSuccessful' comparison would not return
            // the desired result.
            while ((wasXSuccessful = x.TryAdvanceToNextNonWhiteSpaceCharacter(ref indexX)) & (wasYSuccessful = y.TryAdvanceToNextNonWhiteSpaceCharacter(ref indexY)))
            {
                if (char.ToLowerInvariant(x[indexX++]) != char.ToLowerInvariant(y[indexY++]))
                {
                    return false;
                }
            }

            return wasXSuccessful == wasYSuccessful;
        }

        private static bool TryAdvanceToNextNonWhiteSpaceCharacter(this string @string, ref int currentIndex)
        {
            while (currentIndex < @string.Length)
            {
                if (!char.IsWhiteSpace(@string[currentIndex]))
                {
                    return true;
                }

                ++currentIndex;
            }

            return false;
        }
    }
}

/* 
License information for JetBrains.Annotations

MIT License
Copyright (c) 2016 JetBrains http://www.jetbrains.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. */
namespace JetBrains.Annotations
{
    /// <summary>
    /// Indicates that the value of the marked element can never be <c>null</c>.
    /// </summary>
    /// <example><code>
    /// [NotNull] object Foo() {
    ///   return null; // Warning: Possible 'null' assignment
    /// }
    /// </code></example>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Delegate | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.GenericParameter)]
    internal sealed class NotNullAttribute : Attribute
    {
    }

    /// <summary>
    /// Describes dependence between method input and output.
    /// </summary>
    /// <syntax>
    /// <p>Function Definition Table syntax:</p>
    /// <list>
    /// <item>FDT      ::= FDTRow [;FDTRow]*</item>
    /// <item>FDTRow   ::= Input =&gt; Output | Output &lt;= Input</item>
    /// <item>Input    ::= ParameterName: Value [, Input]*</item>
    /// <item>Output   ::= [ParameterName: Value]* {halt|stop|void|nothing|Value}</item>
    /// <item>Value    ::= true | false | null | notnull | canbenull</item>
    /// </list>
    /// If the method has a single input parameter, its name could be omitted.<br/>
    /// Using <c>halt</c> (or <c>void</c>/<c>nothing</c>, which is the same) for the method output
    /// means that the method doesn't return normally (throws or terminates the process).<br/>
    /// Value <c>canbenull</c> is only applicable for output parameters.<br/>
    /// You can use multiple <c>[ContractAnnotation]</c> for each FDT row, or use single attribute
    /// with rows separated by the semicolon. There is no notion of order rows, all rows are checked
    /// for applicability and applied per each program state tracked by the analysis engine.<br/>
    /// </syntax>
    /// <examples><list>
    /// <item><code>
    /// [ContractAnnotation("=&gt; halt")]
    /// public void TerminationMethod()
    /// </code></item>
    /// <item><code>
    /// [ContractAnnotation("null &lt;= param:null")] // reverse condition syntax
    /// public string GetName(string surname)
    /// </code></item>
    /// <item><code>
    /// [ContractAnnotation("s:null =&gt; true")]
    /// public bool IsNullOrEmpty(string s) // string.IsNullOrEmpty()
    /// </code></item>
    /// <item><code>
    /// // A method that returns null if the parameter is null,
    /// // and not null if the parameter is not null
    /// [ContractAnnotation("null =&gt; null; notnull =&gt; notnull")]
    /// public object Transform(object data)
    /// </code></item>
    /// <item><code>
    /// [ContractAnnotation("=&gt; true, result: notnull; =&gt; false, result: null")]
    /// public bool TryParse(string s, out Person result)
    /// </code></item>
    /// </list></examples>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    internal sealed class ContractAnnotationAttribute : Attribute
    {
        public ContractAnnotationAttribute([NotNull] string contract) : this(contract, false)
        {
        }

        public ContractAnnotationAttribute([NotNull] string contract, bool forceFullStates)
        {
            Contract = contract;
            ForceFullStates = forceFullStates;
        }

        [NotNull]
        public string Contract { get; }
        public bool ForceFullStates { get; }
    }

    /// <summary>
    /// Indicates that IEnumerable passed as a parameter is not enumerated.
    /// Use this annotation to suppress the 'Possible multiple enumeration of IEnumerable' inspection.
    /// </summary>
    /// <example><code>
    /// static void ThrowIfNull&lt;T&gt;([NoEnumeration] T v, string n) where T : class
    /// {
    ///   // custom check for null but no enumeration
    /// }
    ///
    /// void Foo(IEnumerable&lt;string&gt; values)
    /// {
    ///   ThrowIfNull(values, nameof(values));
    ///   var x = values.ToList(); // No warnings about multiple enumeration
    /// }
    /// </code></example>
    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class NoEnumerationAttribute : Attribute
    {
    }
}
