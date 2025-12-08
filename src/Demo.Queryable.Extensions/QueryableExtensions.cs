using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Demo.Queryable.Extensions;

public static class QueryableExtensions
{
    // Cache for compiled expression trees to avoid rebuilding them
    private static readonly ConcurrentDictionary<(Type, string), object> _expressionCache = [];

    // Cache for type converters
    private static readonly ConcurrentDictionary<Type, Func<ReadOnlySpan<char>, object?>> _converterCache = [];


    private static readonly ConstantExpression _zeroConstantExpression = Expression.Constant(0);
    private static readonly ConstantExpression _ordinalIgnoreCaseConstantExpression = Expression.Constant(StringComparison.OrdinalIgnoreCase);

    private static readonly MethodInfo _boolCompareToMethod = typeof(bool).GetMethod(nameof(bool.CompareTo), [typeof(bool)])!;
    private static readonly MethodInfo _stringCompareMethod = typeof(string).GetMethod(nameof(string.Compare), [typeof(string), typeof(string), typeof(StringComparison)])!;


    public static IOrderedQueryable<TSource> SortBy<TSource>(this IQueryable<TSource> source, Sort<TSource> sort)
        where TSource : class
    {
        var cacheKey = (typeof(TSource), sort.PropertyInfo.Name);

        Expression<Func<TSource, object>> lambda;

        // Use the cache to get an expression if it was already created
        if (_expressionCache.TryGetValue(cacheKey, out var cachedExpression))
        {
            lambda = (Expression<Func<TSource, object>>)cachedExpression;
        }
        else
        {
            // Create expression tree only if not cached
            var parameter = Expression.Parameter(typeof(TSource), "p");
            var property = Expression.Property(parameter, sort.PropertyInfo);
            var convert = Expression.Convert(property, typeof(object));
            lambda = Expression.Lambda<Func<TSource, object>>(convert, parameter);

            // Cache the expression
            _expressionCache.TryAdd(cacheKey, lambda);
        }

        // Apply ordering.
        //    Currently the most time-consuming operation and memory allocation is here
        return sort.IsAscending ?
            source.OrderBy(lambda) :
            source.OrderByDescending(lambda);
    }


    public static IQueryable<TSource> CursorPage<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Sort<TSource> sort, Cursor? cursor)
        where TSource : class
    {
        // Get key property info
        var keyMember = (MemberExpression)keySelector.Body;

        // Process cursor if provided
        if (cursor is not null)
        {
            var parameter = Expression.Parameter(typeof(TSource), "p");

            var keyPropertyInfo = (PropertyInfo)keyMember.Member;
            var keyValueSpan = cursor.Value.KeyValue.AsSpan();
            var keyValue = ConvertFieldValue(keyValueSpan, keyPropertyInfo.PropertyType);
            if (keyValue is null)
            {
                throw new ArgumentException($"Invalid key value for property '{keyMember.Member.Name}'", nameof(cursor));
            }

            var keyValueConstant = Expression.Constant(keyValue, keyPropertyInfo.PropertyType);
            var keyProperty = Expression.Property(parameter, keyMember.Member.Name);

            Expression filterExpression;

            if (sort.PropertyInfo.Name == keyMember.Member.Name || cursor.Value.TargetValue is null)
            {
                // When the target property is the same as the key or no target value is provided,
                //   we can do a simple comparison
                //     e.g., Id > targetValue or Id < targetValue
                filterExpression = CreateComparisonExpression(
                    keyProperty,
                    keyValueConstant,
                    sort.IsAscending,
                    keyPropertyInfo.PropertyType);
            }
            else
            {
                // When the target property is different from the key, we need to build a compound expression
                //   e.g., (Age > targetValue) OR (Age == targetValue AND Id > keyValue)

                // Convert target value typed
                var targetValueSpan = cursor.Value.TargetValue.AsSpan();
                var targetValue = ConvertFieldValue(targetValueSpan, sort.PropertyInfo.PropertyType);
                if (targetValue is null)
                {
                    throw new ArgumentException($"Invalid field value for property '{sort.PropertyInfo.Name}'", nameof(cursor));
                }

                var targetValueConstant = Expression.Constant(targetValue, sort.PropertyInfo.PropertyType);
                var targetProperty = Expression.Property(parameter, sort.PropertyInfo);


                // 1: Create comparison for target property
                //  e.g. Age > targetValue
                var fieldComparison = CreateComparisonExpression(
                    targetProperty,
                    targetValueConstant,
                    sort.IsAscending,
                    sort.PropertyInfo.PropertyType);

                // 2: Create equality for target property
                //   e.g: Age == targetValue
                var targetEquality = Expression.Equal(targetProperty, targetValueConstant);

                // 3: Create comparison for key property
                //   e.g., Id > keyValue
                var keyComparison = Expression.GreaterThan(keyProperty, keyValueConstant);

                // 3: Combine: (Age == targetValue) AND (Id > keyValue)
                var equalityAndKeyComparison = Expression.AndAlso(targetEquality, keyComparison);

                // 4: (Age > targetValue) OR ((Age == targetValue) AND (Id > keyValue))
                filterExpression = Expression.OrElse(fieldComparison, equalityAndKeyComparison);
            }

            var filterLambda = Expression.Lambda<Func<TSource, bool>>(filterExpression, parameter);
            source = source.Where(filterLambda);
        }

        var sourceOrdered = source.SortBy(sort);
        if (sort.PropertyInfo.Name == keyMember.Member.Name)
        {
            return sourceOrdered;
        }

        return sourceOrdered.ThenBy(keySelector);
    }


    private static Expression CreateComparisonExpression(
        Expression left,
        Expression right,
        bool isAscending,
        Type propertyType)
    {
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        // Boolean doesn't support GreaterThan/LessThan operators
        // Use CompareTo instead: value.CompareTo(target) > 0
        if (underlyingType == typeof(bool))
        {
            var compareToCall = Expression.Call(left, _boolCompareToMethod, right);
            return isAscending
                ? Expression.GreaterThan(compareToCall, _zeroConstantExpression)
                : Expression.LessThan(compareToCall, _zeroConstantExpression);
        }

        else if (underlyingType == typeof(string))
        {
            var compareCall = Expression.Call(_stringCompareMethod, left, right, _ordinalIgnoreCaseConstantExpression);
            return isAscending
                ? Expression.GreaterThan(compareCall, _zeroConstantExpression)
                : Expression.LessThan(compareCall, _zeroConstantExpression);
        }

        // For other types, use standard comparison operators
        return isAscending
            ? Expression.GreaterThan(left, right)
            : Expression.LessThan(left, right);
    }


    private static object? ConvertFieldValue(ReadOnlySpan<char> fieldValue, Type targetType)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Get or create converter for this type
        var converter = _converterCache.GetOrAdd(
            underlyingType,
            static type => type.Name switch
            {
                nameof(Char) => span => char.TryParse(span.ToString(), out var result) ? result : null,

                nameof(Int16) => span => short.TryParse(span, out var result) ? result : null,
                nameof(UInt16) => span => ushort.TryParse(span, out var result) ? result : null,

                nameof(Int32) => span => int.TryParse(span, out var result) ? result : null,
                nameof(UInt32) => span => uint.TryParse(span, out var result) ? result : null,

                nameof(Int64) => span => long.TryParse(span, out var result) ? result : null,
                nameof(UInt64) => span => ulong.TryParse(span, out var result) ? result : null,

                nameof(Single) => span => float.TryParse(span, out var result) ? result : null,
                nameof(Double) => span => double.TryParse(span, out var result) ? result : null,
                nameof(Decimal) => span => decimal.TryParse(span, out var result) ? result : null,

                nameof(Boolean) => span => bool.TryParse(span, out var result) ? result : null,

                nameof(DateTime) => span => DateTime.TryParse(span, out var result) ? result : null,
                nameof(Guid) => span => Guid.TryParse(span, out var result) ? result : null,
                nameof(String) => span => span.ToString(),

                _ => _ => throw new NotSupportedException($"Unsupported field type for cursor '{type.Name}'")
            });

        return converter(fieldValue);
    }
}
