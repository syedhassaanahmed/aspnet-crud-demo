namespace AspNet.Crud.Demo.Models
{
    public class NewProduct
    {
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
    }

    public class UpdatedProduct : NewProduct
    {
        public string ETag { get; set; } = default!;
    }

    public class Product : UpdatedProduct
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
    }
}
