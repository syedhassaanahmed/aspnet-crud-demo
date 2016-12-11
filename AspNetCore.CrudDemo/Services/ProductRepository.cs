using System;
using System.Threading.Tasks;
using AspNetCore.CrudDemo.Models;
using AspNetCore.CrudDemo.Options;
using Microsoft.Extensions.Options;

namespace AspNetCore.CrudDemo.Services
{
    public class ProductRepository : DocumentDbRepository<Product>
    {
        public ProductRepository(IOptions<DocumentDbOptions> options) : base(options)
        {
        }

        public override async Task<Product> CreateAsync(Product product)
        {
            // Empty ProductId because DocumentDB will automatically create a GUID
            product.Id = string.Empty;
            product.Created = DateTimeOffset.Now;

            return await base.CreateAsync(product);
        }

        public override async Task<bool> UpdateAsync(string productId, Product product)
        {
            // Fetch existing product so that we can restrict updating some fields (Id, Created etc)
            var existing = await GetAsync(productId);
            if (existing == null)
                return false;

            product.Id = existing.Id;
            product.Created = existing.Created;
            product.Modified = DateTimeOffset.Now;

            return await base.UpdateAsync(productId, product);
        }
    }
}
