# aspnet-core-crud-demo

ASP.NET Core 1.1 App demonstrating simple CRUD operations using [DocumentDB Emulator](https://docs.microsoft.com/en-us/azure/documentdb/documentdb-nosql-local-emulator).

## Continuous integration

| Build server                | Platform     | Build status                                                                                                                                                    |
|-----------------------------|--------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| AppVeyor                    | Windows      | [![Build status](https://ci.appveyor.com/api/projects/status/a6oe47uup57u4x49?svg=true)](https://ci.appveyor.com/project/syedhassaanahmed/aspnet-core-crud-demo)|
| Travis CI                   | Linux / OS X | [![Build Status](https://travis-ci.org/syedhassaanahmed/aspnet-core-crud-demo.svg?branch=master)](https://travis-ci.org/syedhassaanahmed/aspnet-core-crud-demo) |

## Code Coverage

[![Coverage Status](https://coveralls.io/repos/github/syedhassaanahmed/aspnet-core-crud-demo/badge.svg?branch=master)](https://coveralls.io/github/syedhassaanahmed/aspnet-core-crud-demo?branch=master)

## Deployment

[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)

## API Routes

- GET `api/products` - returns all products
- GET `api/products/{productId}` - returns product for a specific `productId`
- GET `api/products/{productId}/html` - **renders** product `Html` for a specific `productId`
- POST `api/products` - creates a new product, automatically generates an `Id`, sets `Created` and validates `Html`
- PUT `api/products/{productId}` - updates an existing product, sets `Updated`, validates `Html`, assumes that the whole object is being updated and does not allow changing restricted fields (`Id`, `Created`)
- DELETE `api/products/{productId}` - deletes a product

## Integration Tests

- `ProductsController` is tested using `Microsoft.AspNetCore.TestHost` as [described here](https://docs.microsoft.com/en-us/aspnet/core/testing/integration-testing)
- `ProductRepository` is tested by creating an `xUnit Fixture` [DocumentDbFixture](https://github.com/syedhassaanahmed/aspnet-core-crud-demo/blob/master/AspNetCore.CrudDemo.Services.Tests/DocumentDbFixture.cs) which assumes that DocumentDB Emulator is installed and running.  