using Demo.Queryable.Extensions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Demo.NoSQL;

internal static class CustomExtensionEndpoints
{
    public static IEndpointRouteBuilder MapCustomExtensionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("ex/by-cursor/sorted", async (
            HttpContext context,
            IMongoCollection<Product> collection,
            CancellationToken cancellationToken,
            string? cursor = null,
            int pageSize = 20,
            string sort = nameof(Product.Id)) =>
        {
            Sort<Product> sortQuery = sort;
            var sortPropertyName = sortQuery.PropertyName.ToString();
            var cursorQuery = CursorUtils.Decode(cursor);

            var products = await collection.AsQueryable()
                .CursorPage(s => s.Id, sort, cursor)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var resultCursor = products.EncodeCursor(s => s.Id, sortPropertyName);
            if (resultCursor is not null)
            {
                context.Response.Headers.Append(nameof(cursor), resultCursor);
            }

            context.Response.Headers.Append(nameof(pageSize), pageSize.ToString());
            context.Response.Headers.Append(nameof(sort), sort);

            return products;
        });

        return endpoints;
    }
}
