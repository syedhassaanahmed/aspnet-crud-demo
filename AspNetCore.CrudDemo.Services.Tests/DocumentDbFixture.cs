using System;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.CrudDemo.Options;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;

namespace AspNetCore.CrudDemo.Services.Tests
{
    public class DocumentDbFixture : IDisposable
    {
        private const string TestEndpoint = "https://localhost:8081";
        private const string TestPrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private static readonly string RandomDatabaseId = Guid.NewGuid().ToString("N");
        private static readonly Uri DatabaseUri = UriFactory.CreateDatabaseUri(RandomDatabaseId);

        public IDocumentClient Client { get; } = new DocumentClient(new Uri(TestEndpoint), TestPrimaryKey);
        public Uri CollectionUri { get; private set; }
        public IOptions<DocumentDbOptions> Options { get; } = 
            Microsoft.Extensions.Options.Options.Create(new DocumentDbOptions
        {
            Database = RandomDatabaseId,
            EndPoint = TestEndpoint,
            PrimaryKey = TestPrimaryKey
        });
        
        public DocumentDbFixture()
        {
            Client.CreateDatabaseAsync(new Database { Id = RandomDatabaseId }).Wait();
        }

        public async Task CreateCollectionAsync(string collectionId)
        {
            Options.Value.Collection = collectionId;
            CollectionUri = UriFactory.CreateDocumentCollectionUri(RandomDatabaseId, collectionId);
            await Client.CreateDocumentCollectionAsync(DatabaseUri, new DocumentCollection { Id = collectionId });
        }

        public T FetchEntity<T>(string id)
        {
            var document = Client.CreateDocumentQuery(CollectionUri)
                .Where(x => x.Id == id)
                .AsEnumerable()
                .FirstOrDefault();

            return (T) (dynamic) document;
        }

        public async Task DeleteCollectionAsync()
        {
            await Client.DeleteDocumentCollectionAsync(CollectionUri);
        }

        public void Dispose()
        {
            Client.DeleteDatabaseAsync(DatabaseUri).Wait();
        }
    }
}
