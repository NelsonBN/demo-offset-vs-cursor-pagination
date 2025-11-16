using System.Data;
using Demo.SQL;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .AddScoped<IDbConnection>(sp =>
        new NpgsqlConnection(sp.GetRequiredService<IConfiguration>().GetConnectionString("Default")))
    .AddDbContext<DataContext>();

var app = builder.Build();

app.MapEntityFrameworkEndpoints();
app.MapDapperEndpoints();
app.MapExtraEndpoints();

await app.RunAsync();
