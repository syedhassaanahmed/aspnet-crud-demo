namespace AspNetCore.CrudDemo.Options
{
    public class DocumentDbOptions
    {
        public string Collection { get; set; }
        public string Database { get; set; }
        public string EndPoint { get; set; }
        public string PrimaryKey { get; set; }
    }
}
