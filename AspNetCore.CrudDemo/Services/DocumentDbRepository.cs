using System;
using System.Collections.Generic;
using AspNetCore.CrudDemo.Options;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AspNetCore.CrudDemo.Services
{
    public class DocumentDbRepository<T> : IRepository<T> where T : class
    {
        private readonly IOptions<DocumentDbOptions> _options;
        private IDocumentClient _client;

        private string DatabaseId => _options.Value.Database;
        private string CollectionId => _options.Value.Collection;

        private Uri DatabaseUri => UriFactory.CreateDatabaseUri(DatabaseId);
        private Uri CollectionUri => UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId);

        protected DocumentDbRepository(IOptions<DocumentDbOptions> options)
        {
            _options = options;
            InitializeAsync().Wait();
        }

        private async Task InitializeAsync()
        {
            _client = new DocumentClient(new Uri(_options.Value.EndPoint), _options.Value.PrimaryKey);
            await CreateDatabaseIfNotExistsAsync();
            await CreateCollectionIfNotExistsAsync();
        }

        private async Task CreateDatabaseIfNotExistsAsync()
        {
            await CreateResourceIfNotExistsAsync
            (
                () => _client.ReadDatabaseAsync(DatabaseUri),
                () => _client.CreateDatabaseAsync(new Database { Id = DatabaseId })
            );
        }

        private async Task CreateCollectionIfNotExistsAsync()
        {
            await CreateResourceIfNotExistsAsync
            (
                () => _client.ReadDocumentCollectionAsync(CollectionUri),
                () => _client.CreateDocumentCollectionAsync(DatabaseUri,
                    new DocumentCollection { Id = CollectionId })
            );
        }

        private static async Task CreateResourceIfNotExistsAsync(Func<Task> readFunc, Func<Task> createFunc)
        {
            try
            {
                await readFunc();
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode != HttpStatusCode.NotFound)
                    throw;

                await createFunc();
            }
        }

        public virtual Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult(_client.CreateDocumentQuery<T>(CollectionUri).AsEnumerable());
        }

        private Document GetDocument(string id)
        {
            return _client.CreateDocumentQuery(CollectionUri)
                .Where(x => x.Id == id)
                .AsEnumerable()
                .FirstOrDefault();
        }

        public virtual Task<T> GetAsync(string id)
        {
            var document = GetDocument(id);
            return Task.FromResult((T) (dynamic) document);
        }

        public virtual async Task<T> CreateAsync(T item)
        {
            var response = await _client.CreateDocumentAsync(CollectionUri, item);
            return (T)(dynamic)response.Resource;
        }

        public virtual async Task<bool> UpdateAsync(string id, T item)
        {
            return await UpdateOrDeleteAsync(id, document => 
                _client.ReplaceDocumentAsync(document.SelfLink, item));
        }

        public virtual async Task<bool> DeleteAsync(string id)
        {
            return await UpdateOrDeleteAsync(id, document =>
                _client.DeleteDocumentAsync(document.SelfLink));
        }

        private async Task<bool> UpdateOrDeleteAsync(string id, Func<Document, Task> updateOrDeleteFunc)
        {
            var document = GetDocument(id);
            if (document == null)
                return false;

            try
            {
                await updateOrDeleteFunc(document);
                return true;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode != HttpStatusCode.NotFound)
                    throw;

                return false;
            }
        }
    }
}
