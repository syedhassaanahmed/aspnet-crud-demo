using AspNet.Crud.Demo.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNet.Crud.Demo.Services
{
    public class ProductDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var productEntity = modelBuilder.Entity<Product>();

            productEntity.HasNoDiscriminator()
                .ToContainer(nameof(Product))
                .HasPartitionKey(p => p.Id);

            productEntity.Property(p => p.ETag)
                .IsETagConcurrency();
        }
    }
}
