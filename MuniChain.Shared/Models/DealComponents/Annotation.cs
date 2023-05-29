using Microsoft.JSInterop;
using Syncfusion.Blazor.PdfViewer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Models.DealComponents
{
    public class DocumentAnnotation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public int? PageNumber { get; set; }
        public string? Author { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public string? CreatedById { get; set; }
        public string? TextMarkupContent { get; set; }
        public bool IsPublished { get; set; } = false;
        public string? DocumentID { get; set; }
        public List<AnnotationComment>? Comments { get; set; } = new();
        [NotMapped]
        public bool IsEdit { get; set; } = false;

        public object Clone()
        {
            return (DocumentAnnotation)MemberwiseClone();
        }
    }

    public class AnnotationComment
    {
        public string Id { get; set; }
        public string DocumentAnnotationId { get; set; }
        public string CreatedById { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public string Message { get; set; }
        public bool? IsDeleted { get; set; }
        [NotMapped]
        public bool IsEdit { get; set; } = false;
    }
}
