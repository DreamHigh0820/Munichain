using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Data.DatabaseServices;
using Domain.Models;
using Domain.Models.DealComponents;
using Shared.Models.Users;

namespace Domain.Services.Database
{
    public interface IAnnotationService
    {
        Task<List<DocumentAnnotation>> GetAllAnnotations(string documentID, string userID);
        Task<bool> CreateAnnotation(DocumentAnnotation annotation);
        Task UpdateAnnotation(DocumentAnnotation annotation);
        Task DeleteAnnotation(DocumentAnnotation annotation);
        Task<List<AnnotationComment>> GetAllAnnotationsComments(string annotationID);
        Task<bool> CreateComment(AnnotationComment annotationComment, DocumentAnnotation annotation, User user);
        Task<bool> DeleteComment(string commentID);
    }

    public class AnnotationService : IAnnotationService
    {
        private readonly IDbContextFactory<SqlDbContext> _factory;

        public AnnotationService(IDbContextFactory<SqlDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<DocumentAnnotation>> GetAllAnnotations(string documentID, string userID)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.Annotations
                    .Include(x => x.Comments)
                    .Where(x => x.DocumentID == documentID)
                    .Where(x => x.IsPublished == true || x.CreatedById == userID)
                    .ToListAsync();
            }
        }

        public async Task<bool> CreateAnnotation(DocumentAnnotation annotation)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                _dbContext.Add(annotation);
                try
                {
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                catch (Exception)
                {
                    Console.WriteLine("ERROR");
                    return false;
                }
            }
        }

        public async Task UpdateAnnotation(DocumentAnnotation annotation)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                _dbContext.Entry(annotation).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteAnnotation(DocumentAnnotation annotation)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                _dbContext.Remove(annotation);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<AnnotationComment>> GetAllAnnotationsComments(string annotationID)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.Annotations
                    .Where(x => x.Id == annotationID)
                    .SelectMany(x => x.Comments)
                    .ToListAsync();
            }
        }

        public async Task<bool> CreateComment(AnnotationComment comment, DocumentAnnotation annotation, User user)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                var exists = _dbContext.AnnotationComment.Any(x => x.Id == comment.Id);
                if (exists)
                {
                    _dbContext.Entry(comment).State = EntityState.Modified;
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                else
                {
                    comment = new AnnotationComment()
                    {
                        Id = Guid.NewGuid().ToString(),
                        DocumentAnnotationId = annotation.Id,
                        CreatedById = user.Id,
                        CreatedByUserName = user.DisplayName,
                        Message = comment.Message,
                        CreatedTime = DateTimeOffset.UtcNow,
                        IsDeleted = false
                    };
                    _dbContext.AnnotationComment.Add(comment);
                    try
                    {
                        await _dbContext.SaveChangesAsync();
                        return true;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("ERROR");
                        return false;
                    }
                }

            }
        }

        public async Task<bool> DeleteComment(string commentId)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                var commentToDelete = await _dbContext.AnnotationComment.FirstOrDefaultAsync(comment => comment.Id == commentId);
                try
                {
                    if (commentToDelete != null)
                    {
                        _dbContext.Remove(commentToDelete);
                        await _dbContext.SaveChangesAsync();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("ERROR");
                    return false;
                }
            }
        }
    }
}
