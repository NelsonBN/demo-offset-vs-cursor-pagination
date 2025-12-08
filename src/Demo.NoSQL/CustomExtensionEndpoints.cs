using Demo.Queryable.Extensions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Demo.NoSQL;

internal static class CustomExtensionEndpoints
{
    public static IEndpointRouteBuilder MapCustomExtensionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("ex");
        group.MapOffsetEndpoints();
        group.MapCursorEndpoints();
        return endpoints;
    }

    private static IEndpointRouteBuilder MapOffsetEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("by-offset");

        group.MapGet("", async (
            HttpContext context,
            IMongoCollection<Product> collection,
            CancellationToken cancellationToken,
            int page = 1,
            int pageSize = 20) =>
        {
            var products = await collection.AsQueryable()
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            context.Response.Headers.Append(nameof(page), page.ToString());
            context.Response.Headers.Append(nameof(pageSize), pageSize.ToString());

            return products;
        });

        group.MapGet("sorted", async (
            HttpContext context,
            IMongoCollection<Product> collection,
            CancellationToken cancellationToken,
            int page = 1,
            int pageSize = 20,
            string sort = nameof(Product.Id)) =>
        {
            Sort<Product> sortQuery = sort;


            var products = await collection.AsQueryable()
                .SortBy(sortQuery)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            context.Response.Headers.Append(nameof(page), page.ToString());
            context.Response.Headers.Append(nameof(pageSize), pageSize.ToString());
            context.Response.Headers.Append(nameof(sort), sort);

            return products;
        });

        return group;
    }

    private static IEndpointRouteBuilder MapCursorEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("by-cursor");

        group.MapGet("", async (
            HttpContext context,
            IMongoCollection<Product> collection,
            CancellationToken cancellationToken,
            Guid? cursor = null,
            int pageSize = 20) =>
        {
            IQueryable<Product> query = collection.AsQueryable()
                .OrderBy(p => p.Id);

            if (cursor is not null)
            {
                query = query.Where(p => p.Id > cursor.Value);
            }

            var products = await query
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            if (products.Count > 0)
            {
                context.Response.Headers.Append(nameof(cursor), products[^1].Id.ToString());
            }

            context.Response.Headers.Append(nameof(pageSize), pageSize.ToString());

            return products;
        });


        group.MapGet("sorted", async (
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

        return group;
    }
}
