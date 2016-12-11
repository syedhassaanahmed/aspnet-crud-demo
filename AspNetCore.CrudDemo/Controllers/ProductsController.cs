using System.Threading.Tasks;
using AspNetCore.CrudDemo.Models;
using AspNetCore.CrudDemo.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace AspNetCore.CrudDemo.Controllers
{
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        private readonly IRepository<Product> _repository;

        public ProductsController(IRepository<Product> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _repository.GetAllAsync();

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet]
        [Route("{productId}")]
        public async Task<IActionResult> GetAsync(string productId)
        {
            var result = await _repository.GetAsync(productId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet]
        [Route("{productId}/html")]
        public async Task<IActionResult> GetHtmlAsync(string productId)
        {
            var result = await _repository.GetAsync(productId);

            if (string.IsNullOrWhiteSpace(result?.Html))
                return NotFound();

            return Content(result.Html, MediaTypeHeaderValue.Parse("text/html"));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody]Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _repository.CreateAsync(product);
            return Created($"{Request.GetDisplayUrl()}/{result.Id}", result);
        }

        [HttpPut]
        [Route("{productId}")]
        public async Task<IActionResult> UpdateAsync(string productId, [FromBody]Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _repository.UpdateAsync(productId, product);

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpDelete]
        [Route("{productId}")]
        public async Task<IActionResult> DeleteAsync(string productId)
        {
            var result = await _repository.DeleteAsync(productId);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
