# Configurable queries

We want to add configurable queries to our **WebApi** project.

## Migration

For demo purposes, add a migration for a new table `Customers` with the following columns:

- Id (int, primary key, auto-increment)
- Name (string)
- Email (string)
- Country (string)
- Revenue (decimal)

The migration must also contain seed data for 5 customers.

## Configuration

- Add a configuration file `database-endpoints.json` to the WebApi project.
  - No need to copy it to the output directory.
  - Add a configuration setting in `appsettings.json` to point to this file

## Query Definition

`database-endpoints.json` must contain the following query definitions:

```json
{
  "Endpoints": [
    {
      "Route": "/customers/filtered",
      "Method": "GET",
      "Query": "SELECT * FROM Customers WHERE (Country = $country OR $country IS NULL) AND (Email LIKE $emailPattern OR $emailPattern IS NULL) AND (Revenue >= $minRevenue OR $minRevenue IS NULL)",
      "Parameters": {
        "country": { "type": "string", "optional": true },
        "emailPattern": { "type": "string", "optional": true },
        "minRevenue": { "type": "decimal", "optional": true }
      },
      "ReturnType": "Array"
    },
    {
      "Route": "/customers/all",
      "Method": "GET",
      "Query": "SELECT * FROM Customers",
      "ReturnType": "Array"
    },
    {
      "Route": "/customers/by-id",
      "Method": "GET",
      "Query": "SELECT * FROM Customers WHERE Id = $id",
      "Parameters": {
        "id": { "type": "int", "optional": false }
      },
      "ReturnType": "Single"
    }
  ]
}
```

## Implementation

Implement an endpoint using C# 14 extension blocks (similar to `./WebApi/Migrations.cs`) with the following behavior:

- Reads the configuration file `database-endpoints.json`
  - No caching for now
- Handle requests according to the defined routes
- Accept query parameters as defined in the configuration
  - In case of missing required parameters, return bad request with error description
  - Optional parameters might be missing. That should not lead to an error.
- Execute the defined SQL query against the SQLite database
  - Pass the parameters to the query
  - In case of an exception during query execution, return internal server error **without** exposing technical details
- Return the results as JSON
  - If `ReturnType` is `Array`, return an array of results
  - If `ReturnType` is `Single`, return a single result or not found if no result exists

## Testing

- Add sample requests to `WebApi.http` to test the new endpoints
- Add integration tests to `WebApiTests` for all endpoints (in `QueriesTests.cs`):
  - Test with all parameters provided
  - Test with some optional parameters missing
  - Test with required parameters missing (expect bad request)
  - Test with no results (expect empty array or not found)
  - Test with valid results (expect correct JSON response)
- Build the project with `dotnet build`
- Run the integration tests with `dotnet test` to ensure all tests pass

