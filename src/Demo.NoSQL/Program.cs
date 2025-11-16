using Demo.NoSQL;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Linq.Expressions;

BsonClassMap.RegisterClassMap<Product>(cm =>
{
    cm.AutoMap();
    cm.MapIdProperty(p => p.Id)
        .SetSerializer(new GuidSerializer(BsonType.String));
});


var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .AddSingleton(sp =>
        new MongoUrl(sp.GetRequiredService<IConfiguration>().GetConnectionString("Default")))
    .AddSingleton<IMongoClient>(sp =>
        new MongoClient(sp.GetRequiredService<MongoUrl>()))
    .AddScoped(sp =>
    {
        var mongoUrl = sp.GetRequiredService<MongoUrl>();
        var mongoClient = sp.GetRequiredService<IMongoClient>();

        return mongoClient.GetDatabase(mongoUrl.DatabaseName);
    })
    .AddScoped(sp =>
        sp.GetRequiredService<IMongoDatabase>().GetCollection<Product>(nameof(Product)));

var app = builder.Build();

app.MapMongoEndpoints();

await app.RunAsync();


internal static class Endpoints
{
    public static IEndpointRouteBuilder MapMongoEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapOffsetEndpoints();
        endpoints.MapCursorEndpoints();
        endpoints.MapExtraEndpoints();
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
            var products = await collection
                .Find(Builders<Product>.Filter.Empty)
                .Sort(Builders<Product>.Sort.Ascending(p => p.Id))
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
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
            var (propertyName, isDescending) = sort.ParseSortParameter();
            var sortDefinition = isDescending
                ? Builders<Product>.Sort.Descending(propertyName)
                : Builders<Product>.Sort.Ascending(propertyName);

            var products = await collection
                .Find(Builders<Product>.Filter.Empty)
                .Sort(sortDefinition)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
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
            var filter = cursor.HasValue
                ? Builders<Product>.Filter.Gt(p => p.Id, cursor.Value)
                : Builders<Product>.Filter.Empty;

            var products = await collection
                .Find(filter)
                .Sort(Builders<Product>.Sort.Ascending(p => p.Id))
                .Limit(pageSize)
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
            var (propertyName, isDescending) = sort.ParseSortParameter();

            FilterDefinition<Product> filter;
            SortDefinition<Product> sortDefinition;

            if (string.IsNullOrWhiteSpace(cursor))
            {
                filter = Builders<Product>.Filter.Empty;
            }
            else
            {
                var cursorSpan = cursor.AsSpan();
                var separatorIndex = cursorSpan.IndexOf('|');

                if (separatorIndex == -1)
                {
                    throw new ArgumentException("Invalid cursor format", nameof(cursor));
                }

                var fieldValueSpan = cursorSpan[..separatorIndex];
                var keyValueSpan = cursorSpan[(separatorIndex + 1)..];

                filter = propertyName.ToLowerInvariant() switch
                {
                    "id" => isDescending
                        ? Builders<Product>.Filter.Lt(p => p.Id, Guid.Parse(keyValueSpan))
                        : Builders<Product>.Filter.Gt(p => p.Id, Guid.Parse(keyValueSpan)),
                    "name" => CreateFilter(
                        (Product p) => p.Name, fieldValueSpan.ToString(),
                        p => p.Id, Guid.Parse(keyValueSpan),
                        isDescending),
                    "quantity" => CreateFilter(
                        (Product p) => p.Quantity, int.Parse(fieldValueSpan),
                        p => p.Id, Guid.Parse(keyValueSpan),
                        isDescending),
                    _ => throw new ArgumentException($"Unsupported sort property: {propertyName}")
                };
            }

            sortDefinition = isDescending
                ? Builders<Product>.Sort.Descending(propertyName).Ascending(p => p.Id)
                : Builders<Product>.Sort.Ascending(propertyName).Ascending(p => p.Id);

            var products = await collection
                .Find(filter)
                .Sort(sortDefinition)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            if (products.Count > 0)
            {
                var lastProduct = products[^1];
                var fieldValue = propertyName.ToLowerInvariant() switch
                {
                    "id" => lastProduct.Id.ToString(),
                    "name" => lastProduct.Name,
                    "quantity" => lastProduct.Quantity.ToString(),
                    _ => string.Empty
                };
                var nextCursor = $"{fieldValue}|{lastProduct.Id}";
                context.Response.Headers.Append(nameof(cursor), nextCursor);
            }

            context.Response.Headers.Append(nameof(pageSize), pageSize.ToString());
            context.Response.Headers.Append(nameof(sort), sort);

            return products;
        });

        return group;
    }


    private static IEndpointRouteBuilder MapExtraEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("extra");

        group.MapGet("last", async (IMongoCollection<Product> collection, CancellationToken cancellationToken)
            => await collection
                .Find(Builders<Product>.Filter.Empty)
                .Sort(Builders<Product>.Sort.Descending(p => p.Id))
                .Limit(1)
                .FirstOrDefaultAsync(cancellationToken));

        group.MapDelete("products/{id:guid}", async (Guid id, IMongoCollection<Product> collection, CancellationToken cancellationToken) =>
        {
            var result = await collection.DeleteOneAsync(
                Builders<Product>.Filter.Eq(p => p.Id, id),
                cancellationToken);

            if (result.DeletedCount == 0)
            {
                return Results.NotFound();
            }

            return Results.NoContent();
        });

        return group;
    }

    private static FilterDefinition<TEntity> CreateFilter<TEntity, TField, TKey>(
        Expression<Func<TEntity, TField>> field,
        TField fieldValue,
        Expression<Func<TEntity, TKey>> keyField,
        TKey keyValue,
        bool isDescending)
        where TField : IComparable<TField>
        where TKey : IComparable<TKey>
    {
        var fieldComparison = isDescending
            ? Builders<TEntity>.Filter.Lt(field, fieldValue)
            : Builders<TEntity>.Filter.Gt(field, fieldValue);

        var equalityAndKeyComparison = Builders<TEntity>.Filter.And(
            Builders<TEntity>.Filter.Eq(field, fieldValue),
            Builders<TEntity>.Filter.Gt(keyField, keyValue));

        return Builders<TEntity>.Filter.Or(fieldComparison, equalityAndKeyComparison);
    }
}
