using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

public static class Extensions
{
    // Cache for compiled expression trees to avoid rebuilding them
    private static readonly ConcurrentDictionary<string, object> _expressionCache = new();

    // Cache for property info to avoid reflection overhead
    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> _propertyCache = new();

    // Cache for type converters
    private static readonly ConcurrentDictionary<Type, Func<ReadOnlySpan<char>, object?>> _converterCache = new();

    public static IQueryable<TSource> SortBy<TSource>(this IQueryable<TSource> source, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return source;
        }

        var (propertyName, isDescending) = ParseSortParameter(sort.AsSpan());

        // Early validation to avoid expression tree creation if property doesn't exist
        var propertyInfo = GetPropertyInfo<TSource>(propertyName);
        if (propertyInfo is null)
        {
            throw new ArgumentException($"Property '{propertyName}' not found on type {typeof(TSource).Name}", nameof(sort));
        }

        var cacheKey = $"{typeof(TSource).FullName}_{propertyName}_{isDescending}";

        if (_expressionCache.TryGetValue(cacheKey, out var cachedExpression))
        {
            var cachedLambda = (Expression<Func<TSource, object>>)cachedExpression;
            return isDescending ?
                source.OrderByDescending(cachedLambda) :
                source.OrderBy(cachedLambda);
        }

        // Create expression tree only if not cached
        var parameter = Expression.Parameter(typeof(TSource), "p");
        var property = Expression.Property(parameter, propertyInfo);
        var convert = Expression.Convert(property, typeof(object));
        var lambda = Expression.Lambda<Func<TSource, object>>(convert, parameter);

        // Cache the expression
        _expressionCache.TryAdd(cacheKey, lambda);

        return isDescending ?
            source.OrderByDescending(lambda) :
            source.OrderBy(lambda);
    }

    public static IQueryable<TSource> SortBy<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, string sort, string? cursor)
    {
        if (string.IsNullOrEmpty(sort))
        {
            return source;
        }

        var (propertyName, isDescending) = ParseSortParameter(sort.AsSpan());

        // Early validation
        var propertyInfo = GetPropertyInfo<TSource>(propertyName);
        if (propertyInfo is null)
        {
            throw new ArgumentException($"Property '{propertyName}' not found on type {typeof(TSource).Name}", nameof(sort));
        }

        // Get key property info
        var keyMember = keySelector.Body as MemberExpression
            ?? throw new ArgumentException("Key selector must be a member expression", nameof(keySelector));

        // Process cursor if provided
        if (!string.IsNullOrWhiteSpace(cursor))
        {
            var cursorSpan = cursor.AsSpan();
            var separatorIndex = cursorSpan.IndexOf('|');

            if (separatorIndex == -1 || separatorIndex == 0 || separatorIndex == cursorSpan.Length - 1)
            {
                throw new ArgumentException("Invalid cursor format", nameof(cursor));
            }

            var fieldValueSpan = cursorSpan[..separatorIndex];
            var keyValueSpan = cursorSpan[(separatorIndex + 1)..];

            var parameter = Expression.Parameter(typeof(TSource), "p");
            var fieldProperty = Expression.Property(parameter, propertyInfo);

            // Convert field value
            var fieldValue = ConvertFieldValue(fieldValueSpan, propertyInfo.PropertyType);
            if (fieldValue is null)
            {
                throw new ArgumentException($"Invalid field value for property '{propertyName}'", nameof(cursor));
            }

            var fieldValueConstant = Expression.Constant(fieldValue, propertyInfo.PropertyType);

            Expression filterExpression;

            if (string.Equals(propertyInfo.Name, keyMember.Member.Name, StringComparison.Ordinal))
            {
                // Same property as key, simple comparison
                filterExpression = isDescending
                    ? Expression.LessThan(fieldProperty, fieldValueConstant)
                    : Expression.GreaterThan(fieldProperty, fieldValueConstant);
            }
            else
            {
                // Different properties, need compound comparison
                var fieldComparison = isDescending
                    ? Expression.LessThan(fieldProperty, fieldValueConstant)
                    : Expression.GreaterThan(fieldProperty, fieldValueConstant);

                var fieldEquality = Expression.Equal(fieldProperty, fieldValueConstant);
                var keyProperty = Expression.Property(parameter, keyMember.Member.Name);
                var keyPropertyInfo = (PropertyInfo)keyMember.Member;

                var keyValue = ConvertFieldValue(keyValueSpan, keyPropertyInfo.PropertyType);
                if (keyValue is null)
                {
                    throw new ArgumentException($"Invalid key value for property '{keyMember.Member.Name}'", nameof(cursor));
                }

                var keyValueConstant = Expression.Constant(keyValue, keyPropertyInfo.PropertyType);
                var keyComparison = Expression.GreaterThan(keyProperty, keyValueConstant);

                var equalityAndKeyComparison = Expression.AndAlso(fieldEquality, keyComparison);
                filterExpression = Expression.OrElse(fieldComparison, equalityAndKeyComparison);
            }

            var filterLambda = Expression.Lambda<Func<TSource, bool>>(filterExpression, parameter);
            source = source.Where(filterLambda);
        }

        // Apply ordering using cached expression
        var orderCacheKey = $"{typeof(TSource).FullName}_{propertyName}_order";

        if (_expressionCache.TryGetValue(orderCacheKey, out var cachedOrderExpression))
        {
            var cachedOrderLambda = (Expression<Func<TSource, object>>)cachedOrderExpression;
            return isDescending ? source.OrderByDescending(cachedOrderLambda) : source.OrderBy(cachedOrderLambda);
        }

        var orderParameter = Expression.Parameter(typeof(TSource), "p");
        var orderProperty = Expression.Property(orderParameter, propertyInfo);
        var orderConvert = Expression.Convert(orderProperty, typeof(object));
        var orderLambda = Expression.Lambda<Func<TSource, object>>(orderConvert, orderParameter);

        return  isDescending ?
            source.OrderByDescending(orderLambda).ThenBy(keySelector) :
            source.OrderBy(orderLambda).ThenBy(keySelector);
    }

    private static PropertyInfo? GetPropertyInfo<T>(string propertyName)
    {
        var cacheKey = (typeof(T), propertyName);

        return _propertyCache.GetOrAdd(cacheKey, static key =>
            key.Item1.GetProperty(key.Item2, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase));
    }

    private static object? ConvertFieldValue(ReadOnlySpan<char> fieldValue, Type targetType)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Get or create converter for this type
        var converter = _converterCache.GetOrAdd(underlyingType, static type => CreateConverter(type));

        return converter(fieldValue);
    }

    private static Func<ReadOnlySpan<char>, object?> CreateConverter(Type type)
    {
        return type.Name switch
        {
            nameof(Int32) => span => int.TryParse(span, out var result) ? result : null,
            nameof(Int64) => span => long.TryParse(span, out var result) ? result : null,
            nameof(Double) => span => double.TryParse(span, out var result) ? result : null,
            nameof(Decimal) => span => decimal.TryParse(span, out var result) ? result : null,
            nameof(Boolean) => span => bool.TryParse(span, out var result) ? result : null,
            nameof(DateTime) => span => DateTime.TryParse(span, out var result) ? result : null,
            nameof(Guid) => span => Guid.TryParse(span, out var result) ? result : null,
            nameof(String) => span => span.ToString(),
            _ => _ => throw new ArgumentException($"Unsupported field type for cursor: {type.Name}")
        };
    }

    public static (string propertyName, bool isDescending) ParseSortParameter(this ReadOnlySpan<char> sort)
    {
        // Trim whitespace without allocation
        sort = sort.Trim();

        if (sort.IsEmpty)
        {
            return (string.Empty, false);
        }

        var firstChar = sort[0];
        if (firstChar == '+' || firstChar == '-')
        {
            return (sort[1..].Trim().ToString(), firstChar == '-');
        }

        return (sort.ToString(), false);
    }
}
