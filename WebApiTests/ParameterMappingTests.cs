using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using WebApi;

namespace WebApiTests;

public class ParameterMappingTests
{
    private static IQueryCollection CreateQueryCollection(Dictionary<string, string> values)
    {
        var query = new QueryCollection(values.ToDictionary(
            kvp => kvp.Key,
            kvp => new StringValues(kvp.Value)));
        return query;
    }

    [Fact]
    public void MapParameters_WithNoParameters_ReturnsEmptyDictionary()
    {
        // Arrange
        var queryCollection = CreateQueryCollection(new Dictionary<string, string>());

        // Act
        var result = Queries.MapParameters(null, queryCollection);

        // Assert
        Assert.Null(result.Error);
        Assert.NotNull(result.Parameters);
        Assert.Empty(result.Parameters);
    }

    [Fact]
    public void MapParameters_WithRequiredIntParameter_ParsesCorrectly()
    {
        // Arrange
        var parameterDefinitions = new Dictionary<string, Queries.ParameterDefinition>
        {
            { "id", new Queries.ParameterDefinition("int", false) }
        };
        var queryCollection = CreateQueryCollection(new Dictionary<string, string>
        {
            { "id", "42" }
        });

        // Act
        var result = Queries.MapParameters(parameterDefinitions, queryCollection);

        // Assert
        Assert.Null(result.Error);
        Assert.NotNull(result.Parameters);
        Assert.Single(result.Parameters);
        Assert.Equal(42, result.Parameters["id"]);
    }

    [Fact]
    public void MapParameters_WithRequiredDecimalParameter_ParsesCorrectly()
    {
        // Arrange
        var parameterDefinitions = new Dictionary<string, Queries.ParameterDefinition>
        {
            { "minRevenue", new Queries.ParameterDefinition("decimal", false) }
        };
        var queryCollection = CreateQueryCollection(new Dictionary<string, string>
        {
            { "minRevenue", "100000.50" }
        });

        // Act
        var result = Queries.MapParameters(parameterDefinitions, queryCollection);

        // Assert
        Assert.Null(result.Error);
        Assert.NotNull(result.Parameters);
        Assert.Single(result.Parameters);
        Assert.Equal(100000.50m, result.Parameters["minRevenue"]);
    }

    [Fact]
    public void MapParameters_WithRequiredStringParameter_ParsesCorrectly()
    {
        // Arrange
        var parameterDefinitions = new Dictionary<string, Queries.ParameterDefinition>
        {
            { "country", new Queries.ParameterDefinition("string", false) }
        };
        var queryCollection = CreateQueryCollection(new Dictionary<string, string>
        {
            { "country", "Germany" }
        });

        // Act
        var result = Queries.MapParameters(parameterDefinitions, queryCollection);

        // Assert
        Assert.Null(result.Error);
        Assert.NotNull(result.Parameters);
        Assert.Single(result.Parameters);
        Assert.Equal("Germany", result.Parameters["country"]);
    }

    [Fact]
    public void MapParameters_WithMissingRequiredParameter_ReturnsError()
    {
        // Arrange
        var parameterDefinitions = new Dictionary<string, Queries.ParameterDefinition>
        {
            { "id", new Queries.ParameterDefinition("int", false) }
        };
        var queryCollection = CreateQueryCollection([]);

        // Act
        var result = Queries.MapParameters(parameterDefinitions, queryCollection);

        // Assert
        Assert.NotNull(result.Error);
        Assert.Contains("Required parameter 'id' is missing", result.Error);
        Assert.Null(result.Parameters);
    }

    [Fact]
    public void MapParameters_WithMissingOptionalParameter_ReturnsNullValue()
    {
        // Arrange
        var parameterDefinitions = new Dictionary<string, Queries.ParameterDefinition>
        {
            { "country", new Queries.ParameterDefinition("string", true) }
        };
        var queryCollection = CreateQueryCollection(new Dictionary<string, string>());

        // Act
        var result = Queries.MapParameters(parameterDefinitions, queryCollection);

        // Assert
        Assert.Null(result.Error);
        Assert.NotNull(result.Parameters);
        Assert.Single(result.Parameters);
        Assert.True(result.Parameters.ContainsKey("country"));
        Assert.Null(result.Parameters["country"]);
    }

    [Fact]
    public void MapParameters_WithMultipleMissingOptionalParameters_ReturnsAllNullValues()
    {
        // Arrange
        var parameterDefinitions = new Dictionary<string, Queries.ParameterDefinition>
        {
            { "country", new Queries.ParameterDefinition("string", true) },
            { "emailPattern", new Queries.ParameterDefinition("string", true) },
            { "minRevenue", new Queries.ParameterDefinition("decimal", true) }
        };
        var queryCollection = CreateQueryCollection(new Dictionary<string, string>());

        // Act
        var result = Queries.MapParameters(parameterDefinitions, queryCollection);

        // Assert
        Assert.Null(result.Error);
        Assert.NotNull(result.Parameters);
        Assert.Equal(3, result.Parameters.Count);
        Assert.True(result.Parameters.ContainsKey("country"));
        Assert.Null(result.Parameters["country"]);
        Assert.True(result.Parameters.ContainsKey("emailPattern"));
        Assert.Null(result.Parameters["emailPattern"]);
        Assert.True(result.Parameters.ContainsKey("minRevenue"));
        Assert.Null(result.Parameters["minRevenue"]);
    }

    [Fact]
    public void MapParameters_WithSomeOptionalParametersProvided_MixesValuesAndNulls()
    {
        // Arrange
        var parameterDefinitions = new Dictionary<string, Queries.ParameterDefinition>
        {
            { "country", new Queries.ParameterDefinition("string", true) },
            { "emailPattern", new Queries.ParameterDefinition("string", true) },
            { "minRevenue", new Queries.ParameterDefinition("decimal", true) }
        };
        var queryCollection = CreateQueryCollection(new Dictionary<string, string>
        {
            { "country", "Germany" },
            { "minRevenue", "200000" }
        });

        // Act
        var result = Queries.MapParameters(parameterDefinitions, queryCollection);

        // Assert
        Assert.Null(result.Error);
        Assert.NotNull(result.Parameters);
        Assert.Equal(3, result.Parameters.Count);
        Assert.Equal("Germany", result.Parameters["country"]);
        Assert.Null(result.Parameters["emailPattern"]);
        Assert.Equal(200000m, result.Parameters["minRevenue"]);
    }

    [Fact]
    public void MapParameters_WithInvalidIntFormat_ReturnsError()
    {
        // Arrange
        var parameterDefinitions = new Dictionary<string, Queries.ParameterDefinition>
        {
            { "id", new Queries.ParameterDefinition("int", false) }
        };
        var queryCollection = CreateQueryCollection(new Dictionary<string, string>
        {
            { "id", "not-a-number" }
        });

        // Act
        var result = Queries.MapParameters(parameterDefinitions, queryCollection);

        // Assert
        Assert.NotNull(result.Error);
        Assert.Contains("Parameter 'id' has invalid format for type 'int'", result.Error);
        Assert.Null(result.Parameters);
    }

    [Fact]
    public void MapParameters_WithInvalidDecimalFormat_ReturnsError()
    {
        // Arrange
        var parameterDefinitions = new Dictionary<string, Queries.ParameterDefinition>
        {
            { "minRevenue", new Queries.ParameterDefinition("decimal", false) }
        };
        var queryCollection = CreateQueryCollection(new Dictionary<string, string>
        {
            { "minRevenue", "not-a-decimal" }
        });

        // Act
        var result = Queries.MapParameters(parameterDefinitions, queryCollection);

        // Assert
        Assert.NotNull(result.Error);
        Assert.Contains("Parameter 'minRevenue' has invalid format for type 'decimal'", result.Error);
        Assert.Null(result.Parameters);
    }

    [Fact]
    public void MapParameters_WithMixedRequiredAndOptionalParameters_ParsesCorrectly()
    {
        // Arrange
        var parameterDefinitions = new Dictionary<string, Queries.ParameterDefinition>
        {
            { "id", new Queries.ParameterDefinition("int", false) },
            { "country", new Queries.ParameterDefinition("string", true) }
        };
        var queryCollection = CreateQueryCollection(new Dictionary<string, string>
        {
            { "id", "1" }
        });

        // Act
        var result = Queries.MapParameters(parameterDefinitions, queryCollection);

        // Assert
        Assert.Null(result.Error);
        Assert.NotNull(result.Parameters);
        Assert.Equal(2, result.Parameters.Count);
        Assert.Equal(1, result.Parameters["id"]);
        Assert.Null(result.Parameters["country"]);
    }

    [Fact]
    public void MapParameters_WithEmptyStringValue_TreatsAsMissing()
    {
        // Arrange
        var parameterDefinitions = new Dictionary<string, Queries.ParameterDefinition>
        {
            { "country", new Queries.ParameterDefinition("string", true) }
        };
        var queryCollection = CreateQueryCollection(new Dictionary<string, string>
        {
            { "country", "" }
        });

        // Act
        var result = Queries.MapParameters(parameterDefinitions, queryCollection);

        // Assert
        Assert.Null(result.Error);
        Assert.NotNull(result.Parameters);
        Assert.Single(result.Parameters);
        Assert.Null(result.Parameters["country"]);
    }

    [Fact]
    public void MapParameters_UsingDatabaseEndpointsConfig_FilteredEndpoint()
    {
        // Arrange - Using the actual configuration from database-endpoints.json
        var parameterDefinitions = new Dictionary<string, Queries.ParameterDefinition>
        {
            { "country", new Queries.ParameterDefinition("string", true) },
            { "emailPattern", new Queries.ParameterDefinition("string", true) },
            { "minRevenue", new Queries.ParameterDefinition("decimal", true) }
        };
        var queryCollection = CreateQueryCollection(new Dictionary<string, string>
        {
            { "country", "USA" },
            { "minRevenue", "50000.75" }
        });

        // Act
        var result = Queries.MapParameters(parameterDefinitions, queryCollection);

        // Assert
        Assert.Null(result.Error);
        Assert.NotNull(result.Parameters);
        Assert.Equal(3, result.Parameters.Count);
        Assert.Equal("USA", result.Parameters["country"]);
        Assert.Null(result.Parameters["emailPattern"]);
        Assert.Equal(50000.75m, result.Parameters["minRevenue"]);
    }

    [Fact]
    public void MapParameters_UsingDatabaseEndpointsConfig_ByIdEndpoint()
    {
        // Arrange - Using the actual configuration from database-endpoints.json
        var parameterDefinitions = new Dictionary<string, Queries.ParameterDefinition>
        {
            { "id", new Queries.ParameterDefinition("int", false) }
        };
        var queryCollection = CreateQueryCollection(new Dictionary<string, string>
        {
            { "id", "123" }
        });

        // Act
        var result = Queries.MapParameters(parameterDefinitions, queryCollection);

        // Assert
        Assert.Null(result.Error);
        Assert.NotNull(result.Parameters);
        Assert.Single(result.Parameters);
        Assert.Equal(123, result.Parameters["id"]);
    }
}
