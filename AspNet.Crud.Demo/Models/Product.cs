namespace AspNet.Crud.Demo.Models
{
    public class ProductRequest
    {
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
    }

    public class Product : ProductRequest
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
        public string ETag { get; set; } = default!;
    }
}
