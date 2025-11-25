# Unit Testing

- Restructure `/WebApi/Queries.cs` so that parameter mapping can be unit tested in isolation
- Implement unit tests in WebApiTests for parameter mapping using `/WebApi/database-endpoints.json` as a source
  - The tests must ensure that missing optional parameters are still passed with value `null`
- Compile the project and ensure all tests pass
