# aspnet-crud-demo
![.NET CI](https://github.com/syedhassaanahmed/aspnet-crud-demo/actions/workflows/dotnet-ci.yml/badge.svg)

This ASP.NET 6.0 App demonstrates simple CRUD operations on [Cosmos DB Core API](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/query/getting-started) using [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/providers/cosmos/?tabs=dotnet-core-cli).

## API Routes
The app documents API routes through [OpenAPI specification](https://learn.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger?view=aspnetcore-7.0).

## Optimistic Concurrency Control
Optimistic concurrency control has been implemented using ETag and HTTP headers, similar to the [Cosmos DB approach](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/database-transactions-optimistic-concurrency#implementing-optimistic-concurrency-control-using-etag-and-http-headers).

## Integration Tests
Integration tests use the in-memory Test Server as [described here](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0). As a prerequisite, [Cosmos DB Emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/local-emulator?tabs=ssl-netstd21) must be installed and running.
