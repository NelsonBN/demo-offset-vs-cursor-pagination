using System.Data;
using Dapper;

namespace Demo;

public static class DapperEndpoints
{
    public static IEndpointRouteBuilder MapDapperEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("dp");
        group.MapOffsetEndpoints();
        group.MapCursorEndpoints();
        return endpoints;
    }

    private static IEndpointRouteBuilder MapOffsetEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("by-offset");

        group.MapGet("", async (
            HttpContext context,
            IDbConnection connection,
            int page = 1,
            int pageSize = 20) =>
        {
            var offset = (page - 1) * pageSize;
            var limit = pageSize;

            var query = await connection
                .QueryAsync<Product>(
                    """
                    SELECT Id, Name, Quantity, Code
                    FROM Product
                    ORDER BY Id
                    LIMIT @pageSize OFFSET @offset
                    """,
                    new { pageSize, offset });

            var products = query.ToList();

            context.Response.Headers.Append(nameof(page), page.ToString());
            context.Response.Headers.Append(nameof(pageSize), pageSize.ToString());

            return products;
        });

        group.MapGet("sorted", async (
            HttpContext context,
            IDbConnection connection,
            int page = 1,
            int pageSize = 20,
            string sort = nameof(Product.Id)) =>
        {
            var offset = (page - 1) * pageSize;
            var limit = pageSize;

            (var propertyName, var isDescending) = sort.AsSpan().ParseSortParameter();

            var query = await connection
                .QueryAsync<Product>(
                    $"""
                    SELECT Id, Name, Quantity, Code
                    FROM Product
                    ORDER BY {propertyName} {(isDescending ? "DESC" : "ASC")}
                    LIMIT @pageSize OFFSET @offset
                    """,
                    new
                    {
                        pageSize,
                        offset
                    });

            var products = query.ToList();

            context.Response.Headers.Append(nameof(page), page.ToString());
            context.Response.Headers.Append(nameof(pageSize), pageSize.ToString());

            return products;
        });

        return group;
    }

    private static IEndpointRouteBuilder MapCursorEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("by-cursor");

        group.MapGet("", async (
            HttpContext context,
            IDbConnection connection,
            int? cursor = null,
            int pageSize = 20) =>
        {
            var query = await connection
                .QueryAsync<Product>(
                    """
                    SELECT Id, Name, Quantity, Code
                    FROM Product
                    WHERE (@cursor IS NULL OR Id > @cursor)
                    ORDER BY Id
                    LIMIT @pageSize
                    """,
                    new { pageSize, cursor });

            var products = query.ToList();

            if (products.Count > 0)
            {
                context.Response.Headers.Append(nameof(cursor), products[^1].Id.ToString());
            }
            context.Response.Headers.Append(nameof(pageSize), pageSize.ToString());

            return products;
        });

        group.MapGet("sorted", async (
            HttpContext context,
            IDbConnection connection,
            string? cursor = null,
            int pageSize = 20,
            string sort = nameof(Product.Id)) =>
        {
            (var propertyName, var isDescending) = sort.AsSpan().ParseSortParameter();

            string whereCursor;
            object? fieldValue = null;
            int? keyValue = null;

            if (string.IsNullOrWhiteSpace(cursor))
            {
                whereCursor = "TRUE";
            }
            else
            {
                var parts = cursor.Split('|');
                var fieldValueStr = parts[0];
                var keyValueStr = parts[1];

                // Convert field value to appropriate type based on property name
                fieldValue = propertyName.ToLowerInvariant() switch
                {
                    "id" => int.Parse(fieldValueStr),
                    "quantity" => int.Parse(fieldValueStr),
                    "code" => Guid.Parse(fieldValueStr),
                    "name" => fieldValueStr,
                    _ => fieldValueStr
                };

                // Key value (Id) is always an integer
                keyValue = int.Parse(keyValueStr);

                if (nameof(Product.Id).Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    whereCursor = $"({propertyName} {(isDescending ? "<" : ">")} @fieldValue)";
                }
                else
                {
                    whereCursor = $"({propertyName} {(isDescending ? "<" : ">")} @fieldValue) OR ({propertyName} = @fieldValue AND Id > @keyValue)";
                }
            }

            var sql = $"""
                SELECT Id, Name, Quantity, Code
                FROM Product
                WHERE {whereCursor}
                ORDER BY {propertyName} {(isDescending ? "DESC" : "ASC")}, Id ASC
                LIMIT @pageSize
                """;

            var query = await connection
                .QueryAsync<Product>(
                    sql,
                    new
                    {
                        pageSize,
                        fieldValue,
                        keyValue
                    });

            var products = query.ToList();

            if (products.Count > 0 && !string.IsNullOrWhiteSpace(cursor))
            {
                context.Response.Headers.Append(nameof(cursor), cursor);
            }
            context.Response.Headers.Append(nameof(pageSize), pageSize.ToString());

            return products;
        });

        return group;
    }
}
