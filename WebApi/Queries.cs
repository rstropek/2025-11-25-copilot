using Microsoft.Data.Sqlite;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

[assembly: InternalsVisibleTo("WebApiTests")]

namespace WebApi;

public static class Queries
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public void MapDynamicQueryEndpoints()
        {
            // Load configuration
            var serviceProvider = endpoints.ServiceProvider;
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();
            
            var configPath = configuration["DatabaseEndpointsConfigPath"]
                ?? throw new InvalidOperationException("DatabaseEndpointsConfigPath not configured");

            var fullPath = Path.Combine(env.ContentRootPath, configPath);
            if (!File.Exists(fullPath))
            {
                throw new InvalidOperationException($"Configuration file not found: {fullPath}");
            }

            var json = File.ReadAllText(fullPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var config = JsonSerializer.Deserialize<DatabaseEndpointsConfig>(json, options);
            if (config?.Endpoints == null)
            {
                throw new InvalidOperationException("Invalid configuration format");
            }

            // Dynamically map all endpoints from configuration
            foreach (var endpointDef in config.Endpoints)
            {
                var route = endpointDef.Route;
                var method = endpointDef.Method.ToUpperInvariant();

                if (method == "GET")
                {
                    endpoints.MapGet(route, async (HttpContext context, SqliteConnection connection) =>
                    {
                        // Validate and collect parameters
                        var result = MapParameters(endpointDef.Parameters, context.Request.Query);
                        if (result.Error != null)
                        {
                            return Results.BadRequest(new { error = result.Error });
                        }
                        
                        var parameters = result.Parameters!;

                        try
                        {
                            await connection.OpenAsync();
                            try
                            {
                                using var command = connection.CreateCommand();
                                command.CommandText = endpointDef.Query;

                                // Add parameters to the command
                                foreach (var param in parameters)
                                {
                                    var dbParam = command.CreateParameter();
                                    dbParam.ParameterName = $"${param.Key}";
                                    dbParam.Value = param.Value ?? DBNull.Value;
                                    command.Parameters.Add(dbParam);
                                }

                                using var reader = await command.ExecuteReaderAsync();
                                var results = new List<Dictionary<string, object?>>();

                                while (await reader.ReadAsync())
                                {
                                    var row = new Dictionary<string, object?>();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                    }
                                    results.Add(row);
                                }

                                if (endpointDef.ReturnType.Equals("Single", StringComparison.OrdinalIgnoreCase))
                                {
                                    return results.Count == 0 ? Results.NotFound() : Results.Ok(results[0]);
                                }
                                else
                                {
                                    return Results.Ok(results);
                                }
                            }
                            finally
                            {
                                await connection.CloseAsync();
                            }
                        }
                        catch (Exception)
                        {
                            return Results.Problem("An error occurred while executing the query");
                        }
                    });
                }
            }
        }
    }

    internal static ParameterMappingResult MapParameters(
        Dictionary<string, ParameterDefinition>? parameterDefinitions,
        IQueryCollection queryCollection)
    {
        var parameters = new Dictionary<string, object?>();
        
        if (parameterDefinitions != null && parameterDefinitions.Count > 0)
        {
            foreach (var param in parameterDefinitions)
            {
                var queryValue = queryCollection[param.Key].FirstOrDefault();

                if (string.IsNullOrEmpty(queryValue))
                {
                    if (!param.Value.Optional)
                    {
                        return new ParameterMappingResult(
                            null, 
                            $"Required parameter '{param.Key}' is missing");
                    }
                    parameters[param.Key] = null;
                }
                else
                {
                    try
                    {
                        parameters[param.Key] = param.Value.Type.ToLowerInvariant() switch
                        {
                            "int" => int.Parse(queryValue, CultureInfo.InvariantCulture),
                            "decimal" => decimal.Parse(queryValue, CultureInfo.InvariantCulture),
                            "string" => queryValue,
                            _ => throw new InvalidOperationException($"Unsupported parameter type: {param.Value.Type}")
                        };
                    }
                    catch (FormatException)
                    {
                        return new ParameterMappingResult(
                            null,
                            $"Parameter '{param.Key}' has invalid format for type '{param.Value.Type}'");
                    }
                }
            }
        }

        return new ParameterMappingResult(parameters, null);
    }

    internal record ParameterMappingResult(
        Dictionary<string, object?>? Parameters,
        string? Error);

    internal record DatabaseEndpointsConfig(List<EndpointDefinition> Endpoints);

    internal record EndpointDefinition(
        string Route,
        string Method,
        string Query,
        Dictionary<string, ParameterDefinition>? Parameters,
        string ReturnType);

    internal record ParameterDefinition(string Type, bool Optional);
}
