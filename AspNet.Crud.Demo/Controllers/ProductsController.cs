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

            return Ok(product);
        }

        // POST: /products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(NewProduct newProduct)
        {
            var product = new Product
            {
                Name = newProduct.Name,
                Price = newProduct.Price,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // PUT: /products/{guid}
        [HttpPut("{id}")]
        public async Task<ActionResult<Product>> PutProduct(Guid id, UpdatedProduct updatedProduct)
        {
            var product = await _dbContext.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (product.ETag != updatedProduct.ETag)
            {
                return StatusCode(StatusCodes.Status409Conflict, product.ETag);
            }

            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;
            product.ModifiedAt = DateTimeOffset.UtcNow;

            _dbContext.Entry(product).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DBConcurrencyException)
            {
                return StatusCode(StatusCodes.Status409Conflict, product.ETag);
            }

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