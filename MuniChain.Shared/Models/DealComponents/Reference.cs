using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DealComponents
{
    public class Reference
    {
        public string? Id { get; set; }
        public string? DealId { get; set; }
        public string? FirmId { get; set; }
        public string? GivenBy { get; set; }
        public DateTime? DateGivenUTC { get; set; }
        [NotMapped]
        public string? UserRole { get; set; }
        [NotMapped]
        public string? DealName { get; set; }
        public string? Message { get; set; }
    }
}
