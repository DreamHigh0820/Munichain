using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DealComponents
{
    public class DealParticipant
    {

        [NotMapped]
        public string BlobUrl { get; set; }
        public DealParticipant()
        {
            BlobUrl = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production"
                ? "https://stmunichaindev.blob.core.windows.net/profile-pictures" : "https://stmunichainprod.blob.core.windows.net/profile-pictures";
        }

        public object Clone()
        {
            return (DealParticipant)MemberwiseClone();
        }

        public string? Id { get; set; }
        public string? DealId { get; set; }
        public string? UserId { get; set; }
        public string? EmailAddress { get; set; }
        public string? DisplayName { get; set; }
        [NotMapped]
        public string? ProfilePicUrl => $"{BlobUrl}/{UserId}";
        public string? Role { get; set; }
        public string? CreatedBy { get; set; }
        public bool? IsPublic { get; set; }
        public DateTime? DateAddedUTC { get; set; }
        public List<string> DealPermissions { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
