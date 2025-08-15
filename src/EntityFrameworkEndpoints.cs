using Microsoft.EntityFrameworkCore;

namespace Demo;

public static class EntityFrameworkEndpoints
{
    public static IEndpointRouteBuilder MapEntityFrameworkEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("ef");
        group.MapOffsetEndpoints();
        group.MapCursorEndpoints();
        return endpoints;
    }

    private static IEndpointRouteBuilder MapOffsetEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("by-offset");

        group.MapGet("", async (
            HttpContext context,
            DataContext db,
            CancellationToken cancellationToken,
            int page = 1,
            int pageSize = 20) =>
        {
            var products = await db
                .Products
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            context.Response.Headers.Append(nameof(page), page.ToString());
            context.Response.Headers.Append(nameof(pageSize), pageSize.ToString());

            return products;
        });

        group.MapGet("sorted", async (
            HttpContext context,
            DataContext db,
            CancellationToken cancellationToken,
            int page = 1,
            int pageSize = 20,
            string sort = nameof(Product.Id)) =>
        {
            var products = await db
                .Products
                .SortBy(sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
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
            DataContext db,
            CancellationToken cancellationToken,
            int? cursor = null,
            int pageSize = 20) =>
        {
            IQueryable<Product> query = db
                .Products
                .OrderBy(p => p.Id);

            if (cursor is not null)
            {
                query = query.Where(p => p.Id > cursor.Value);
            }

            var products = await query
                .Take(pageSize)
                .AsNoTracking()
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
            DataContext db,
            CancellationToken cancellationToken,
            string? cursor = null,
            int pageSize = 20,
            string sort = nameof(Product.Id)) =>
        {
            IQueryable<Product> query = db
                .Products
                .SortBy(s => s.Id, sort, cursor);

            var products = await query
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Generate next cursor from the last item
            if (products.Count > 0 && !string.IsNullOrWhiteSpace(cursor))
            {
                context.Response.Headers.Append(nameof(cursor), cursor);
            }
            context.Response.Headers.Append(nameof(pageSize), pageSize.ToString());
            context.Response.Headers.Append(nameof(sort), sort);

            return products;
        });

        return group;
    }
}
