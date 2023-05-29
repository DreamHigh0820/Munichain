using Domain.Models.DealComponents;
using Shared.Models.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DealComponents
{
    public class Document
    {
        public object Clone()
        {
            return MemberwiseClone();
        }

        public string? Id { get; set; }
        public string? DealId { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedByFullName { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        [NotMapped]
        public string? Status { get; set; }
        [NotMapped]
        public bool? IsSignature { get; set; }
        public PublicDocumentViewSettings? PublicDocumentViewSettings { get; set; }
        [DisplayName("User Permissions")]
        public List<string>? UserPermissions { get; set; }
        [DisplayName("Created Date")]
        public DateTime? CreatedDateTimeUTC { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
