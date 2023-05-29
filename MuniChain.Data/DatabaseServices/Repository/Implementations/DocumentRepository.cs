using Data.DatabaseServices.Repository;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DealComponents;

namespace Data.DatabaseServices.Repository.Implementations
{
    public interface IDocumentRepository : IRepository<Document>
    {
    }
    public class DocumentRepository : Repository<Document>, IDocumentRepository
    {
        public DocumentRepository(IDbContextFactory<SqlDbContext> context) : base(context)
        {
        }
    }
}
