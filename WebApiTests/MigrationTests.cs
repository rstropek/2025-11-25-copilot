using System.Net;

namespace WebApiTests;

public class MigrationTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    [Fact]
    public async Task MigrateEndpoint_ReturnsSuccess()
    {
        // Act
        var response = await fixture.HttpClient.PostAsync("/migrate", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Migrations applied successfully", content);
    }
}
