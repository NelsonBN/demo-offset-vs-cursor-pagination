using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Demo.Queryable.Extensions;

public ref struct Sort<TData>
    where TData : class
{
    public readonly ReadOnlySpan<char> PropertyName;
    public readonly bool IsAscending;
    public readonly PropertyInfo PropertyInfo;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Sort(ReadOnlySpan<char> propertyName, bool isAscending, PropertyInfo propertyInfo)
    {
        PropertyName = propertyName;
        IsAscending = isAscending;
        PropertyInfo = propertyInfo;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out ReadOnlySpan<char> propertyName, out bool isAscending, out PropertyInfo propertyInfo)
    {
        propertyName = PropertyName;
        isAscending = IsAscending;
        propertyInfo = PropertyInfo;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Sort<TData>(string sort)
        => sort.ParseSort<TData>();
}

public ref struct Sort
{
    public readonly ReadOnlySpan<char> PropertyName;
    public readonly bool IsAscending;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Sort(ReadOnlySpan<char> propertyName, bool isAscending)
    {
        PropertyName = propertyName;
        IsAscending = isAscending;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out ReadOnlySpan<char> propertyName, out bool isAscending)
    {
        propertyName = PropertyName;
        isAscending = IsAscending;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Sort(string sort)
        => sort.ParseSort();
}

public static class SortUtils
{
    // Cache for property info to avoid reflection overhead
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = [];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseSort<TData>(this ReadOnlySpan<char> query, out Sort<TData> sort)
        where TData : class
    {
        if (query.TryParseSort(out var output))
        {
            // Use cache to avoid reflection overhead and reduce allocations
            var properties = _propertyCache.GetOrAdd(
                typeof(TData),
                static t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

            foreach (var property in properties)
            {
                if (output.PropertyName.Equals(property.Name.AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    sort = new(
                        output.PropertyName,
                        output.IsAscending,
                        property);
                    return true;
                }
            }
        }

        sort = new(
            ReadOnlySpan<char>.Empty,
            true,
            default!);
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Sort<TData> ParseSort<TData>(this ReadOnlySpan<char> sort)
        where TData : class
    {
        if (sort.TryParseSort<TData>(out var output))
        {
            return output;
        }

        throw new ArgumentException($"Property '{sort}' not found on type '{typeof(TData).Name}'", nameof(sort));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseSort(this ReadOnlySpan<char> query, out Sort sort)
    {
        if (query.IsEmpty)
        {
            sort = new Sort(ReadOnlySpan<char>.Empty, true);
            return false;
        }

        var firstChar = query[0];

        // Check if it's a sign character
        if (firstChar is '+' or '-')
        {
            sort = new Sort(query[1..], firstChar == '+');
            return true;
        }

        // No sign, ascending by default
        sort = new Sort(query, true);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Sort ParseSort(this ReadOnlySpan<char> sort)
    {
        if (sort.TryParseSort(out var output))
        {
            return output;
        }

        throw new ArgumentException("Sort needs to be defined", nameof(sort));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Sort ParseSort(this ReadOnlySpan<char> sort, ReadOnlySpan<char> defaultPropertyName)
    {
        if (sort.TryParseSort(out var output))
        {
            return output;
        }

        return new Sort(defaultPropertyName, true);
    }
}
