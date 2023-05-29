using Data.DatabaseServices.Repository.Implementations;
using Microsoft.Extensions.Logging;
using Shared.Models.DealComponents;

namespace Domain.Services.Database
{
    public interface IDocumentService
    {
        Task<bool> Create(Document document);
        Task<bool> DeleteById(string id);
        Task<Document> GetById(string id);
        Task<bool> Update(Document document);
    }

    public class DocumentService : IDocumentService
    {
        public DocumentService(IDocumentRepository documents, ILogger<DocumentService> logger)
        {
            _documents = documents;
            _logger = logger;
        }

        private readonly IDocumentRepository _documents;
        private readonly ILogger<DocumentService> _logger;

        public async Task<Document> GetById(string id)
        {
            try
            {
                return await _documents.GetById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get document by ID", ex, id);
                return null;
            }
        }

        public async Task<bool> Create(Document document)
        {
            try
            {
                await _documents.Add(document);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create document", ex);
                return false;
            }
        }

        public async Task<bool> Update(Document document)
        {
            try
            {
                await _documents.Update(document);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to update document", ex);
                return false;
            }
        }

        public async Task<bool> DeleteById(string id)
        {
            try
            {
                await _documents.Remove(id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to delete document", ex);
                return false;
            }
        }
    }
}
