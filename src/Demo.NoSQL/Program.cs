using Demo.NoSQL;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

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
app.MapLinqEndpoints();
app.MapExtraEndpoints();

await app.RunAsync();
