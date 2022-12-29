using AspNet.Crud.Demo.Models;
using AspNet.Crud.Demo.Services;
using AspNet.Crud.Demo.Settings;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using NUnit.Framework;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace AspNet.Crud.Demo.Integration.Tests
{
    public class ProductsControllerTests
    {
        private const string DatabaseName = "IntegrationTestsDb";

        private readonly WebApplicationFactory<Program> _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Configure Cosmos DB Emulator Credentials
                    services.Configure<CosmosDbSettings>(settings =>
                    {
                        settings.AccountEndpoint = "https://localhost:8081/";
                        settings.AccountKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
                        settings.DatabaseName = DatabaseName;
                    });
                });
            });

        private ProductDbContext _dbContext;

        private HttpClient _httpClient;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var scope = _factory.Services.GetService<IServiceScopeFactory>()!.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

            _httpClient = _factory.CreateClient();
        }

        [SetUp]
        public async Task SetUpAsync()
        {
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.EnsureCreatedAsync();
        }

        [Test]
        [NonParallelizable]
        public async Task GetProducts_NoProduct_ReturnsEmptyList()
        {
            // Act
            var response = await _httpClient.GetAsync("/Products");
            var result = await response.Content.ReadFromJsonAsync<IEnumerable<Product>>();

            // Assert
            Assert.Multiple(() =>
            {   
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(result, Is.Empty);
            });
        }

        [Test]
        [NonParallelizable]
        public async Task GetProducts_SingleProduct_ReturnsListWithSameProduct()
        {
            // Arrange
            var product = new Product { Name = nameof(GetProducts_SingleProduct_ReturnsListWithSameProduct) };
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _httpClient.GetAsync("/Products");
            var result = await response.Content.ReadFromJsonAsync<IEnumerable<Product>>();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(result!.Single().Id, Is.EqualTo(product.Id));
                Assert.That(result!.Single().Name, Is.EqualTo(product.Name));
            });
        }

        [Test]
        public async Task GetProduct_NonExistingProduct_ReturnsNotFound()
        {
            // Act
            var response = await _httpClient.GetAsync($"/Products/{Guid.NewGuid():D}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task GetProduct_SingleProduct_ReturnsSameProduct()
        {
            // Arrange
            var product = new Product { Name = nameof(GetProduct_SingleProduct_ReturnsSameProduct) };
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _httpClient.GetAsync($"/Products/{product.Id:D}");
            var result = await response.Content.ReadFromJsonAsync<Product>();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Headers.ETag!.ToString(), Is.EqualTo(product.ETag));
                Assert.That(result!.Id, Is.EqualTo(product.Id));
                Assert.That(result!.Name, Is.EqualTo(product.Name));
            });
        }

        [Test]
        public async Task GetProduct_WrongETag_ReturnsSameProduct()
        {
            // Arrange
            var product = new Product { Name = nameof(GetProduct_WrongETag_ReturnsSameProduct) };
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, $"/Products/{product.Id:D}");
            request.Headers.Add(HeaderNames.IfNoneMatch, "\"blabla\"");
            var response = await _httpClient.SendAsync(request);

            var result = await response.Content.ReadFromJsonAsync<Product>();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Headers.ETag!.ToString(), Is.EqualTo(product.ETag));
                Assert.That(result!.Id, Is.EqualTo(product.Id));
                Assert.That(result!.Name, Is.EqualTo(product.Name));
            });
        }

        [Test]
        public async Task GetProduct_IfNoneMatchHeader_ReturnsNotModified()
        {
            // Arrange
            var product = new Product { Name = nameof(GetProduct_IfNoneMatchHeader_ReturnsNotModified) };
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, $"/Products/{product.Id:D}");
            request.Headers.Add(HeaderNames.IfNoneMatch, product.ETag);
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotModified));
        }

        [Test]
        public async Task PostProduct_SingleProduct_ReturnsSameProduct()
        {
            // Arrange
            var productRequest = new ProductRequest { Name = nameof(PostProduct_SingleProduct_ReturnsSameProduct) };
            var apiRoute = "/Products";

            // Act
            var response = await _httpClient.PostAsJsonAsync(apiRoute, productRequest);
            var result = await response.Content.ReadFromJsonAsync<Product>();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
                Assert.That(response.Headers.ETag!.ToString(), Is.EqualTo(result!.ETag));
                Assert.That(result!.Name, Is.EqualTo(productRequest.Name));
                Assert.That($"{apiRoute}/{result!.Id:D}", Is.EqualTo(response.Headers.Location!.PathAndQuery));
            });
        }

        [Test]
        public async Task PutProduct_NonExistingProduct_ReturnsNotFound()
        {
            // Arrange
            var productRequest = new ProductRequest { Name = string.Empty };

            // Act
            var response = await _httpClient.PutAsJsonAsync($"/Products/{Guid.NewGuid():D}", productRequest);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task PutProduct_WrongETag_Returns412PreconditionFailed()
        {
            // Arrange
            var product = new Product { Name = nameof(PutProduct_WrongETag_Returns412PreconditionFailed) };
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            var productRequest = new ProductRequest { Name = $"{product.Name}_updated" };

            // Act
            var request = new HttpRequestMessage(HttpMethod.Put, $"/Products/{product.Id:D}");
            request.Headers.Add(HeaderNames.IfMatch, "\"blabla\"");
            request.Content = new StringContent(JsonSerializer.Serialize(productRequest), Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.PreconditionFailed));
        }

        [Test]
        public async Task PutProduct_ConcurrentUpdates_Returns412PreconditionFailed()
        {
            // Arrange
            var product = new Product { Name = nameof(PutProduct_ConcurrentUpdates_Returns412PreconditionFailed) };
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();
            var productId = product.Id.ToString("D");

            // Update product outside of the DBContext
            var cosmosClient = _dbContext.Database.GetCosmosClient();
            var container = cosmosClient.GetContainer(DatabaseName, nameof(Product));
            var partitionKey = new PartitionKey(productId);

            await container.PatchItemAsync<Product>(productId, partitionKey, 
                new[] { PatchOperation.Replace($"/{nameof(Product.Name)}", $"{product.Name}_updated_externally") });

            // Act
            var productRequest = new ProductRequest { Name = $"{product.Name}_updated_via_api" };
            var request = new HttpRequestMessage(HttpMethod.Put, $"/Products/{productId}");
            request.Headers.Add(HeaderNames.IfMatch, product.ETag);
            request.Content = new StringContent(JsonSerializer.Serialize(productRequest), Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.PreconditionFailed));
        }

        [Test]
        public async Task PutProduct_SingleProduct_ReturnsSameProduct()
        {
            // Arrange
            var product = new Product { Name = nameof(PutProduct_SingleProduct_ReturnsSameProduct) };
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            var productRequest = new ProductRequest { Name = $"{product.Name}_updated_via_api" };

            // Act
            var response = await _httpClient.PutAsJsonAsync($"/Products/{product.Id:D}", productRequest);
            var result = await response.Content.ReadFromJsonAsync<Product>();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Headers!.ToString(), Is.Not.EqualTo(product.ETag));
                Assert.That(result!.Name, Is.EqualTo(productRequest.Name));
            });
        }

        [Test]
        public async Task DeleteProduct_NonExistingProduct_ReturnsNotFound()
        {
            // Act
            var response = await _httpClient.DeleteAsync($"/Products/{Guid.NewGuid():D}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task DeleteProduct_SingleProduct_ReturnsSameProduct()
        {
            // Arrange
            var product = new Product { Name = nameof(DeleteProduct_SingleProduct_ReturnsSameProduct) };
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _httpClient.DeleteAsync($"/Products/{product.Id:D}");
            var result = await response.Content.ReadFromJsonAsync<Product>();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(result!.Id, Is.EqualTo(product.Id));
                Assert.That(result!.Name, Is.EqualTo(product.Name));
            });
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _httpClient.Dispose();
            _factory.Dispose();
        }
    }
}