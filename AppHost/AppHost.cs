var builder = DistributedApplication.CreateBuilder(args);

var dbPath = builder.Configuration["Database:path"] ?? throw new InvalidOperationException("Database path not configured");
var dbFile = builder.Configuration["Database:file"] ?? throw new InvalidOperationException("Database file not configured");

var sqlite = builder.AddSqlite("my-database", dbPath, dbFile);

var webApi = builder.AddProject<Projects.WebApi>("webapi")
    .WithReference(sqlite);

builder.Build().Run();
