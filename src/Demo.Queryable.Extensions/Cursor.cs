using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Demo.Queryable.Extensions;

public readonly struct Cursor
{
    public readonly string KeyValue;
    public readonly string? TargetValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Cursor(string keyValue)
    {
        KeyValue = keyValue;
        TargetValue = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Cursor(string keyValue, string? targetValue)
    {
        KeyValue = keyValue;
        TargetValue = targetValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out string keyValue, out string? targetValue)
    {
        keyValue = KeyValue;
        targetValue = TargetValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Cursor?(string? sort)
    {
        return sort.Decode();
    }
}


public static class CursorUtils
{
    private const char Delimiter = '\u0000';


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? EncodeCursor<TSource>(this IEnumerable<TSource>? data)
    {
        if (data is null)
        {
            return null;
        }

        if (!data.Any())
        {
            return null;
        }

        var lastValue = data.LastOrDefault();
        if (lastValue is null)
        {
            return null;
        }

        return Encode(lastValue.ToString());
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? EncodeCursor<TSource, TKey>(this IEnumerable<TSource>? data, Expression<Func<TSource, TKey>> keySelector, string? targetSelector = null)
        where TSource : class
    {
        if (data is null)
        {
            return null;
        }

        var lastRecord = data.LastOrDefault();
        if (lastRecord is null)
        {
            return null;
        }

        var keyMember = (MemberExpression)keySelector.Body;
        var keyProperty = lastRecord.GetType().GetProperty(keyMember.Member.Name)!;


        var type = lastRecord.GetType();
        var keyValue = type?.GetProperty(
            keyProperty.Name,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?
            .GetValue(lastRecord, null)?
            .ToString() ??
            throw new InvalidOperationException("Key value cannot be null");


        var targetValue = targetSelector is null ?
            null :
            type?.GetProperty(
                targetSelector,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?
                .GetValue(lastRecord, null)?
                .ToString();


        if (targetValue is null)
        {
            return CursorUtils.Encode(keyValue);
        }

        return CursorUtils.Encode(keyValue, targetValue);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Encode(string keyValue)
    {
        // Calculate exact byte count needed (not max, but actual)
        var byteCount = Encoding.UTF8.GetByteCount(keyValue);
        Span<byte> bytes = stackalloc byte[byteCount];
        Encoding.UTF8.GetBytes(keyValue, bytes);
        return Base64Url.EncodeToString(bytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Encode(string keyValue, string? targetValue)
    {
        if (targetValue is null)
        {
            return Encode(keyValue);
        }

        // Calculate exact byte count for both parts
        var keyByteCount = Encoding.UTF8.GetByteCount(keyValue);
        var targetByteCount = Encoding.UTF8.GetByteCount(targetValue);
        var totalByteCount = keyByteCount + 1 + targetByteCount; // +1 for delimiter

        Span<byte> bytes = stackalloc byte[totalByteCount];

        // Encode KeyValue
        Encoding.UTF8.GetBytes(keyValue, bytes);

        // Add delimiter
        bytes[keyByteCount] = (byte)Delimiter;

        // Encode TargetValue
        Encoding.UTF8.GetBytes(targetValue, bytes[(keyByteCount + 1)..]);

        return Base64Url.EncodeToString(bytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryDecode(this string? query, out Cursor? cursor)
    {
        if (query is null)
        {
            cursor = null;
            return true;
        }

        try
        {
            var bytes = Base64Url.DecodeFromChars(query);
            var combined = Encoding.UTF8.GetString(bytes);
            var span = combined.AsSpan();

            var delimiterIndex = span.IndexOf(Delimiter);
            if (delimiterIndex == -1)
            {
                cursor = new Cursor(span.ToString(), null);
                return true;
            }

            cursor = new Cursor(
                span[..delimiterIndex].ToString(),
                span[(delimiterIndex + 1)..].ToString());
            return true;
        }
        catch (FormatException)
        {
            cursor = null;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Cursor? Decode(this string? query)
    {
        if (TryDecode(query, out var cursor))
        {
            return cursor;
        }

        throw new FormatException("Invalid cursor format");
    }
}
