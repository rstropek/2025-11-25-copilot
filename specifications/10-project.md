Create a .NET solution with the following structure:

## Project Structure

* Aspire App Host (`AppHost`)
  * Must reference the ASP.NET Core Minimal API project
* Aspire Service Defaults (`ServiceDefaults`)
  * AppHost MUST NOT reference ServiceDefaults
* ASP.NET Core Minimal API project (`WebApi`)
  * Must reference service defaults
* Put everything under a common solution file (`DynamicDbApi.sln`)

## Configure Aspire

* Add the WebApi to the AppHost (in Program.cs)
* Add the service defaults to the WebApi using DI (in Program.cs)
* Add a SQLite database to the AppHost and make WebApi refer to it.
  * Use the community-maintained NuGet package for SQLite support in Aspire. It is documented in Microsoft's docs. You can also find information on the website of Aspire (aspire.dev).
* Add a configuration section `Database` to the AppHost's `appsettings.json` that holds:
  * `path`: The folder where the SQLite database file is stored (use the full path to the project root)
  * `file`: The name of the SQLite database file (use `dynamicdbapi.db`)
* Pass the configuration values to `AddSqlite` when adding the database to the AppHost
* Add the SQLite client to the WebApi using the corresponding Apire NuGet package (SQLite **without** Entity Framework Core)

## Migrations

* Add a folder (`Migrations`) to the WebApi project to hold database migration files
  * Add a sample migration creating a Dummy table with two columns (e.g., Id and Name)
  * Note that all migration files must be idempotent
  * Add a setting to appsettings.json that configures the path to the migrations folder
* Create a file `Migrations.cs` in the WebApi project
  * Contains extension methods for endpoint builder (research C# 14 docs for new extension block feature)
  * Add an endpoint `POST /migrate` that applies all pending migrations to the database
  * Add a POST request to the `/migrate` endpoint for testing purposes
* Create a REST Client `WebApi.http` file in the root folder of the project
  * Make the host name configurable via a variable

## Testing

* Once generated everything, compile the solution with `dotnet build` to ensure there are no errors
* Run the web API project to ensure it starts without errors
* Ensure that the `/migrate` endpoint works as expected
