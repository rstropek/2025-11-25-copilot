using System.Net;
using System.Text.Json;

namespace WebApiTests;

public class QueriesTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    [Fact]
    public async Task GetAllCustomers_ReturnsArray()
    {
        // Arrange
        await fixture.HttpClient.PostAsync("/migrate", null);

        // Act
        var response = await fixture.HttpClient.GetAsync("/customers/all");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var customers = JsonSerializer.Deserialize<JsonElement[]>(content);
        
        Assert.NotNull(customers);
        Assert.Equal(5, customers.Length);
    }

    [Fact]
    public async Task GetCustomerById_WithValidId_ReturnsSingleCustomer()
    {
        // Arrange
        await fixture.HttpClient.PostAsync("/migrate", null);

        // Act
        var response = await fixture.HttpClient.GetAsync("/customers/by-id?id=1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var customer = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.Equal(1, customer.GetProperty("Id").GetInt32());
        Assert.Equal("Acme Corporation", customer.GetProperty("Name").GetString());
    }

    [Fact]
    public async Task GetCustomerById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        await fixture.HttpClient.PostAsync("/migrate", null);

        // Act
        var response = await fixture.HttpClient.GetAsync("/customers/by-id?id=999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomerById_WithMissingRequiredParameter_ReturnsBadRequest()
    {
        // Arrange
        await fixture.HttpClient.PostAsync("/migrate", null);

        // Act
        var response = await fixture.HttpClient.GetAsync("/customers/by-id");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Required parameter 'id' is missing", content);
    }

    [Fact]
    public async Task FilterCustomers_WithAllParameters_ReturnsFilteredResults()
    {
        // Arrange
        await fixture.HttpClient.PostAsync("/migrate", null);

        // Act
        var response = await fixture.HttpClient.GetAsync("/customers/filtered?country=Germany&minRevenue=200000");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var customers = JsonSerializer.Deserialize<JsonElement[]>(content);
        
        Assert.NotNull(customers);
        Assert.Single(customers);
        Assert.Equal("Deutsche Industries", customers[0].GetProperty("Name").GetString());
    }

    [Fact]
    public async Task FilterCustomers_WithCountryParameter_ReturnsFilteredResults()
    {
        // Arrange
        await fixture.HttpClient.PostAsync("/migrate", null);

        // Act
        var response = await fixture.HttpClient.GetAsync("/customers/filtered?country=USA");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var customers = JsonSerializer.Deserialize<JsonElement[]>(content);
        
        Assert.NotNull(customers);
        Assert.Single(customers);
        Assert.Equal("USA", customers[0].GetProperty("Country").GetString());
    }

    [Fact]
    public async Task FilterCustomers_WithEmailPattern_ReturnsFilteredResults()
    {
        // Arrange
        await fixture.HttpClient.PostAsync("/migrate", null);

        // Act
        var response = await fixture.HttpClient.GetAsync("/customers/filtered?emailPattern=%tokyo%");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var customers = JsonSerializer.Deserialize<JsonElement[]>(content);
        
        Assert.NotNull(customers);
        Assert.Single(customers);
        Assert.Contains("tokyo", customers[0].GetProperty("Email").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FilterCustomers_WithMinRevenue_ReturnsFilteredResults()
    {
        // Arrange
        await fixture.HttpClient.PostAsync("/migrate", null);

        // Act
        var response = await fixture.HttpClient.GetAsync("/customers/filtered?minRevenue=100000");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var customers = JsonSerializer.Deserialize<JsonElement[]>(content);
        
        Assert.NotNull(customers);
        Assert.Equal(3, customers.Length);
    }

    [Fact]
    public async Task FilterCustomers_WithNoOptionalParameters_ReturnsAllCustomers()
    {
        // Arrange
        await fixture.HttpClient.PostAsync("/migrate", null);

        // Act
        var response = await fixture.HttpClient.GetAsync("/customers/filtered");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var customers = JsonSerializer.Deserialize<JsonElement[]>(content);
        
        Assert.NotNull(customers);
        Assert.Equal(5, customers.Length);
    }

    [Fact]
    public async Task FilterCustomers_WithNoResults_ReturnsEmptyArray()
    {
        // Arrange
        await fixture.HttpClient.PostAsync("/migrate", null);

        // Act
        var response = await fixture.HttpClient.GetAsync("/customers/filtered?country=NonExistentCountry");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var customers = JsonSerializer.Deserialize<JsonElement[]>(content);
        
        Assert.NotNull(customers);
        Assert.Empty(customers);
    }
}
