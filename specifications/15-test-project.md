# Test Project

## Project Configuration

* Add an xUnit test project `WebApiTests` to the solution
  * Reference the `WebApi` project
* Reference the `Aspire.Hosting.Testing` NuGet package in the test project
* Reference the AppHost project from WebApiTests

## Test Fixture

From another project, I have the following test fixture. Apply it similarly to this project:

```cs
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace WebApiTests;

public class WebApiTestFixture : IAsyncLifetime
{
    public DistributedApplication App { get; private set; } = null!;
    public HttpClient HttpClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        App = await builder.BuildAsync();
        await App.StartAsync();

        HttpClient = App.CreateHttpClient("webapi");

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await App.ResourceNotifications.WaitForResourceHealthyAsync(
            "webapi",
            cts.Token);
    }

    public async Task DisposeAsync()
    {
        HttpClient?.Dispose();
        if (App != null)
        {
            await App.DisposeAsync();
        }
    }
}
```

## Tests

* Create a test that executes the `migrate` endpoint (see `/WebApi/Migrations.cs`)

## Testing

* Build the solution with `dotnet build` to ensure there are no errors
* Do not start the application, run run the tests with `dotnet test` instead to ensure they pass
