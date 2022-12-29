using AspNet.Crud.Demo.Models;
using AspNet.Crud.Demo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AspNet.Crud.Demo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductDbContext _dbContext;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ProductDbContext dbContext, ILogger<ProductsController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // GET: /products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _dbContext.Products.ToListAsync();
        }

        // GET: /products/{guid}
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(Guid id)
        {
            var product = await _dbContext.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (Request.Headers.IfNoneMatch.FirstOrDefault() == product.ETag)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            Response.Headers.ETag = product.ETag;
            return Ok(product);
        }

        // POST: /products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(ProductRequest productRequest)
        {
            var product = new Product
            {
                Name = productRequest.Name,
                Price = productRequest.Price,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            Response.Headers.ETag = product.ETag;
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // PUT: /products/{guid}
        [HttpPut("{id}")]
        public async Task<ActionResult<Product>> PutProduct(Guid id, ProductRequest productRequest)
        {
            var existingProduct = await _dbContext.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            var ifMatchHeader = Request.Headers.IfMatch.FirstOrDefault();
            if (ifMatchHeader != null && ifMatchHeader != existingProduct.ETag)
            {
                return StatusCode(StatusCodes.Status412PreconditionFailed);
            }

            var product = new Product
            {
                Id = id,
                Name = productRequest.Name,
                Price = productRequest.Price,
                CreatedAt = existingProduct.CreatedAt,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            _dbContext.Entry(existingProduct).CurrentValues.SetValues(product);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DBConcurrencyException)
            {
                return StatusCode(StatusCodes.Status412PreconditionFailed);
            }

            Response.Headers.ETag = product.ETag;
            return Ok(product);
        }

        // DELETE: /products/{guid}
        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(Guid id)
        {
            var product = await _dbContext.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();

            return product;
        }
    }
}