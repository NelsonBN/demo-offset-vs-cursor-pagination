using MongoDB.Driver;

namespace Demo.NoSQL;

public static class ExtraEndpoints
{
    public static IEndpointRouteBuilder MapExtraEndpoints(this IEndpointRouteBuilder endpoints)
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
}
