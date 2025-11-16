using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Demo.SQL;

public static class ExtraEndpoints
{
    public static IEndpointRouteBuilder MapExtraEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("extra");

        group.MapGet("last", (IDbConnection connection)
            => connection.QuerySingleOrDefaultAsync<Product>(
                """
                SELECT Id, Name, Quantity, Code
                FROM Product
                ORDER BY Id DESC
                LIMIT 1
                """));

        group.MapDelete("products/{id:int}", async (int id, IDbConnection connection) =>
        {
            var affectedRows = await connection.ExecuteAsync(
                """
                DELETE FROM Product
                WHERE Id = @Id
                """,
                new { Id = id });

            if (affectedRows == 0)
            {
                return Results.NotFound();
            }

            return Results.NoContent();
        });

        return endpoints;
    }
}
