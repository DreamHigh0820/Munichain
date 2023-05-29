using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models.DealComponents
{
    public class AppointmentData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string? Subject { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAllDay { get; set; }
        public string? CategoryColor { get; set; }
        public bool IsReadonly { get; set; }
        public string? RecurrenceRule { get; set; }
        public int? RecurrenceID { get; set; }
        public string? RecurrenceException { get; set; }
        public string? StartTimezone { get; set; }
        public string? EndTimezone { get; set; }
        public string? DealModelId { get; set; }
    }
}
