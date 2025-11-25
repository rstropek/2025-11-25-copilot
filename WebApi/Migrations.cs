using Microsoft.Data.Sqlite;

namespace WebApi;

public static class Migrations
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public void MapMigrationEndpoints()
        {
            endpoints.MapPost("/migrate", async (SqliteConnection connection, IConfiguration configuration, IWebHostEnvironment env) =>
            {
                var migrationsPath = configuration["MigrationsPath"] 
                    ?? throw new InvalidOperationException("MigrationsPath not configured");
                
                var basePath = Path.Combine(env.ContentRootPath, migrationsPath);
                if (!Directory.Exists(basePath))
                {
                    return Results.Problem($"Migrations directory not found: {basePath}");
                }

                var migrationFiles = Directory.GetFiles(basePath, "*.sql")
                    .OrderBy(f => f)
                    .ToList();

                if (migrationFiles.Count == 0)
                {
                    return Results.Ok(new { message = "No migration files found" });
                }

                await connection.OpenAsync();
                try
                {
                    var appliedMigrations = new List<string>();
                    foreach (var file in migrationFiles)
                    {
                        var sql = await File.ReadAllTextAsync(file);
                        using var command = connection.CreateCommand();
                        command.CommandText = sql;
                        await command.ExecuteNonQueryAsync();
                        appliedMigrations.Add(Path.GetFileName(file));
                    }

                    return Results.Ok(new
                    {
                        message = "Migrations applied successfully",
                        appliedMigrations
                    });
                }
                finally
                {
                    await connection.CloseAsync();
                }
            })
            .WithName("ApplyMigrations");
        }
    }
}
