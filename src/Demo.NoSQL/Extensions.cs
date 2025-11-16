using System.Linq.Expressions;
using MongoDB.Driver;

namespace Demo.NoSQL;

public static class Extensions
{
    public static (string propertyName, bool isDescending) ParseSortParameter(this string sort)
    {
        var span = sort.AsSpan().Trim();

        if (span.IsEmpty)
        {
            return ("", false);
        }

        var firstChar = span[0];
        if (firstChar is '+' or '-')
        {
            return (span[1..].Trim().ToString(), firstChar == '-');
        }

        return (span.ToString(), false);
    }

}
