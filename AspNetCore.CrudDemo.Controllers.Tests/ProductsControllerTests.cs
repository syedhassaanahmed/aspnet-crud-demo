using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AspNetCore.CrudDemo.Models;
using AspNetCore.CrudDemo.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.CrudDemo.Controllers.Tests
{
    public class ProductsControllerTests
    {
        private readonly HttpClient _client;
        private readonly IRepository<Product> _mockRepository;

        public ProductsControllerTests()
        {
            _mockRepository = Substitute.For<IRepository<Product>>();

            var webHostBuilder = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    services.AddTransient(serviceProvider => _mockRepository);
                });

            var server = new TestServer(webHostBuilder);
            _client = server.CreateClient();
        }

        [Fact]
        public async Task GetAllProducts_Returns404_IfProductListIsNull()
        {
            // Arrange
            _mockRepository.GetAllAsync().Returns((List<Product>)null);

            // Act
            var response = await _client.GetAsync("/api/products");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAllProducts_Returns200_IfProductListIsEmpty()
        {
            // Arrange
            _mockRepository.GetAllAsync().Returns(new List<Product>());

            // Act
            var response = await _client.GetAsync("/api/products");
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<Product>>(responseString);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllProducts_Returns200_IfProductsExist()
        {
            // Arrange
            var products = new List<Product> {new Product {Id = "123"}};
            _mockRepository.GetAllAsync().Returns(products);

            // Act
            var response = await _client.GetAsync("/api/products");
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<Product>>(responseString);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(products.Count, result.Count);
            Assert.Equal(products.Single().Id, result.Single().Id);
        }

        [Fact]
        public async Task GetProduct_NonExistingProductId_Returns404()
        {
            // Arrange
            const string productId = "123";
            _mockRepository.GetAsync(productId).Returns((Product)null);

            // Act
            var response = await _client.GetAsync($"/api/products/{productId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetProduct_ExistingProductId_Returns200()
        {
            // Arrange
            const string productId = "123";
            _mockRepository.GetAsync(productId).Returns(new Product { Id = productId });

            // Act
            var response = await _client.GetAsync($"/api/products/{productId}");
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Product>(responseString);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(productId, result.Id);
        }

        [Fact]
        public async Task GetProductHtml_EmptyHtmlValue_Returns404()
        {
            // Arrange
            const string productId = "123";
            _mockRepository.GetAsync(productId).Returns(new Product { Id = productId });

            // Act
            var response = await _client.GetAsync($"/api/products/{productId}/html");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetProductHtml_ExistingProductId_ReturnsHtmlContentType()
        {
            // Arrange
            const string productId = "123";
            _mockRepository.GetAsync(productId).Returns(new Product
            {
                Id = productId,
                Html = "<html></html>"
            });

            // Act
            var response = await _client.GetAsync($"/api/products/{productId}/html");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task CreateProduct_InvalidHtml_Returns400()
        {
            // Arrange
            var product = new Product { Id = "123" };

            // Act
            var postContent = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/products", postContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateProduct_ValidProduct_Returns201()
        {
            // Arrange
            const string productId = "123";
            var product = new Product {Id = productId, Html = "<html></html>"};
            _mockRepository.CreateAsync(Arg.Any<Product>()).Returns(product);
            const string apiRoute = "/api/products";

            // Act
            var postContent = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(apiRoute, postContent);
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Product>(responseString);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(productId, result.Id);
            Assert.NotNull(response.Headers.Location);
            Assert.Equal(response.Headers.Location.PathAndQuery, $"{apiRoute}/{productId}");
        }

        [Fact]
        public async Task UpdateProduct_InvalidHtml_Returns400()
        {
            // Arrange
            const string productId = "123";
            var product = new Product { Id = productId };

            // Act
            var postContent = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync($"/api/products/{productId}", postContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateProduct_NonExistingProductId_Returns404()
        {
            // Arrange
            const string productId = "123";
            var product = new Product { Id = productId, Html = "<html></html>" };
            _mockRepository.UpdateAsync(productId, Arg.Any<Product>()).Returns(false);

            // Act
            var postContent = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync($"/api/products/{productId}", postContent);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateProduct_ExistingProductId_Returns204()
        {
            // Arrange
            const string productId = "123";
            var product = new Product { Id = productId, Html = "<html></html>" };
            _mockRepository.UpdateAsync(productId, Arg.Any<Product>()).Returns(true);

            // Act
            var postContent = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync($"/api/products/{productId}", postContent);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteProduct_NonExistingProductId_Returns404()
        {
            // Arrange
            const string productId = "123";
            _mockRepository.DeleteAsync(productId).Returns(false);

            // Act
            var response = await _client.DeleteAsync($"/api/products/{productId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteProduct_ExistingProductId_Returns204()
        {
            // Arrange
            const string productId = "123";
            _mockRepository.DeleteAsync(productId).Returns(true);

            // Act
            var response = await _client.DeleteAsync($"/api/products/{productId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
