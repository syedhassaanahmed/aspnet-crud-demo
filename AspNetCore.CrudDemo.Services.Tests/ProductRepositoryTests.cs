using System;
using System.Threading.Tasks;
using AspNetCore.CrudDemo.Models;
using Xunit;

namespace AspNetCore.CrudDemo.Services.Tests
{
    public class ProductRepositoryTests : IClassFixture<DocumentDbFixture>, IDisposable
    {
        private readonly DocumentDbFixture _fixture;

        public ProductRepositoryTests(DocumentDbFixture fixture)
        {
            _fixture = fixture;
            _fixture.CreateCollectionAsync("Products").Wait();
        }

        [Fact]
        public async Task GetAllAsync_NoProducts_ReturnsEmptyCollection()
        {
            // Arrange
            var productRepository = new ProductRepository(_fixture.Options);

            // Act
            var result = await productRepository.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_SomeProducts_ReturnsNonEmptyCollection()
        {
            // Arrange
            const string productId = "123";
            await _fixture.Client.CreateDocumentAsync(_fixture.CollectionUri, new Product { Id = productId });
            var productRepository = new ProductRepository(_fixture.Options);

            // Act
            var result = await productRepository.GetAllAsync();

            // Assert
            Assert.Single(result, x => x.Id == productId);
        }

        [Fact]
        public async Task GetAsync_NonExistingProductId_ReturnsNull()
        {
            // Arrange
            var productRepository = new ProductRepository(_fixture.Options);

            // Act
            var result = await productRepository.GetAsync("123");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_ExistingProductId_ReturnsProduct()
        {
            // Arrange
            const string productId = "123";
            await _fixture.Client.CreateDocumentAsync(_fixture.CollectionUri, new Product { Id = productId });
            var productRepository = new ProductRepository(_fixture.Options);

            // Act
            var result = await productRepository.GetAsync(productId);

            // Assert
            Assert.Equal(productId, result.Id);
        }

        [Fact]
        public async Task CreateAsync_ProductWithNoData_GeneratesNewIdForProduct()
        {
            // Arrange
            var productRepository = new ProductRepository(_fixture.Options);

            // Act
            var result = await productRepository.CreateAsync(new Product());

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Id);
        }

        [Fact]
        public async Task CreateAsync_Always_GeneratesNewIdForProduct()
        {
            // Arrange
            const string productId = "123";
            var productRepository = new ProductRepository(_fixture.Options);

            // Act
            var result = await productRepository.CreateAsync(new Product { Id = productId });

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(productId, result.Id);
        }

        [Fact]
        public async Task CreateAsync_Always_SetsCreated()
        {
            // Arrange
            var product = new Product();
            var expectedCreated = product.Created;
            var productRepository = new ProductRepository(_fixture.Options);

            // Act
            var result = await productRepository.CreateAsync(product);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(expectedCreated, result.Created);
        }

        [Fact]
        public async Task UpdateAsync_NonExistingProduct_ReturnsFalse()
        {
            // Arrange
            const string productId = "123";
            var productRepository = new ProductRepository(_fixture.Options);

            // Act
            var result = await productRepository.UpdateAsync(productId,
                new Product { Id = productId });

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAsync_ExistingProduct_ReturnsTrue()
        {
            // Arrange
            const string productId = "123";
            var product = new Product {Id = productId};
            await _fixture.Client.CreateDocumentAsync(_fixture.CollectionUri, product);

            const string expectedHtml = "<html></html>";
            product.Html = expectedHtml;

            var productRepository = new ProductRepository(_fixture.Options);

            // Act
            var updated = await productRepository.UpdateAsync(productId, product);
            var result = _fixture.FetchEntity<Product>(productId);

            // Assert
            Assert.True(updated);
            Assert.NotNull(result);
            Assert.Equal(expectedHtml, result.Html);
        }

        [Fact]
        public async Task UpdateAsync_Always_SetsModified()
        {
            // Arrange
            const string productId = "123";
            var product = new Product { Id = productId };
            await _fixture.Client.CreateDocumentAsync(_fixture.CollectionUri, product);
            
            var productRepository = new ProductRepository(_fixture.Options);

            // Act
            var updated = await productRepository.UpdateAsync(productId, product);
            var result = _fixture.FetchEntity<Product>(productId);

            // Assert
            Assert.True(updated);
            Assert.NotNull(result);
            Assert.NotNull(result.Modified);
        }

        [Fact]
        public async Task UpdateAsync_Never_UpdatesCreated()
        {
            // Arrange
            const string productId = "123";
            var product = new Product { Id = productId };

            await _fixture.Client.CreateDocumentAsync(_fixture.CollectionUri, product);
            product = _fixture.FetchEntity<Product>(productId);

            var expectedCreated = product.Created;
            product.Created = DateTimeOffset.Now;

            var productRepository = new ProductRepository(_fixture.Options);

            // Act
            var updated = await productRepository.UpdateAsync(productId, product);
            var result = _fixture.FetchEntity<Product>(productId);

            // Assert
            Assert.True(updated);
            Assert.NotNull(result);
            Assert.Equal(expectedCreated, result.Created);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingProduct_ReturnsFalse()
        {
            // Arrange
            const string productId = "123";
            var productRepository = new ProductRepository(_fixture.Options);

            // Act
            var deleted = await productRepository.DeleteAsync(productId);
            var result = _fixture.FetchEntity<Product>(productId);

            // Assert
            Assert.False(deleted);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_ExistingProduct_ReturnsTrue()
        {
            // Arrange
            const string productId = "123";
            await _fixture.Client.CreateDocumentAsync(_fixture.CollectionUri,
                new Product { Id = productId });
            var productRepository = new ProductRepository(_fixture.Options);

            // Act
            var deleted = await productRepository.DeleteAsync(productId);
            var result = _fixture.FetchEntity<Product>(productId);

            // Assert
            Assert.True(deleted);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_AlreadyDeletedProduct_ReturnsFalse()
        {
            // Arrange
            const string productId = "123";
            await _fixture.Client.CreateDocumentAsync(_fixture.CollectionUri,
                new Product { Id = productId });
            var productRepository = new ProductRepository(_fixture.Options);

            // Act
            var firstDelete = await productRepository.DeleteAsync(productId);
            var secondDelete = await productRepository.DeleteAsync(productId);

            // Assert
            Assert.True(firstDelete);
            Assert.False(secondDelete);
        }

        public void Dispose()
        {
            _fixture.DeleteCollectionAsync().Wait();
        }
    }
}
