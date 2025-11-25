# Configurable inserts

We want to add configurable insert operations to our **WebApi** project.

## Configuration

- Extend the configuration file `database-endpoints.json` to support insert operations
  - No schema changes needed for the configuration file location

## Insert Definition

`database-endpoints.json` must be extended to support the following insert definitions:

```json
{
  "Endpoints": [
    {
      "Route": "/customers",
      "Method": "POST",
      "Query": "INSERT INTO Customers (Name, Email, Country, Revenue) VALUES ($name, $email, $country, $revenue); SELECT last_insert_rowid() AS Id",
      "Parameters": {
        "name": { "type": "string", "optional": false },
        "email": { "type": "string", "optional": false },
        "country": { "type": "string", "optional": true },
        "revenue": { "type": "decimal", "optional": true }
      }
    },
    {
      "Route": "/customers/bulk",
      "Method": "POST",
      "Query": "INSERT INTO Customers (Name, Email, Country, Revenue) VALUES ($name, $email, $country, $revenue)",
      "Parameters": {
        "name": { "type": "string", "optional": false },
        "email": { "type": "string", "optional": false },
        "country": { "type": "string", "optional": true },
        "revenue": { "type": "decimal", "optional": true }
      },
      "AcceptArray": true
    }
  ]
}
```

## Implementation

Extend the endpoint implementation in `./WebApi/QueryEndpoints.cs` with the following behavior:

- Support `POST` method in addition to `GET`
- Make sure to structure the code so that GET and POST handling are cleanly separated
  - However, common logic must be reused (DRY principle)
- For POST requests:
  - Read request body as JSON
  - If `AcceptArray` is `true`, accept an array of objects in the request body and execute the query for each object
  - If `AcceptArray` is not set or `false`, accept a single object in the request body
  - Map JSON properties to query parameters (case-insensitive matching)
  - Validate that all required parameters are provided
  - In case of missing required parameters, return bad request with error description
  - Optional parameters might be missing from the JSON body. That should not lead to an error. Pass `NULL` to the query for missing optional parameters
- Execute the defined SQL query against the SQLite database
  - Pass the parameters from the JSON body to the query
  - For array inserts, execute all queries in a transaction
  - In case of an exception during query execution, return internal server error **without** exposing technical details
- Return http status created for successful inserts
  - For single inserts, return the newly created record's ID in the response body as JSON: `{ "id": <newly_created_id> }`
  - For bulk inserts response body is empty 

## Testing

- Add sample requests to `WebApi.http` to test the new insert endpoints
- Add integration tests to `WebApiTests` for all insert endpoints (in `InsertsTests.cs`):
  - Test single insert with all parameters provided
  - Test single insert with optional parameters missing
  - Test single insert with required parameters missing (expect bad request)
  - Test bulk insert with multiple valid records
  - Test bulk insert with some records missing required parameters (expect bad request)
  - Test that single insert returns the correct ID
  - Test that bulk insert completes successfully with 204 status
- Build the project with `dotnet build`
- Run the integration tests with `dotnet test` to ensure all tests pass
