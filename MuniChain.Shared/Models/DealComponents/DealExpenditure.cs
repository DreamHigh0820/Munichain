using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.DealComponents
{
    public class DealExpenditure
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Required]
        public string? DealModelId { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        [DisplayName("Expenditure Name")]
        public string? DisplayName { get; set; }
        public decimal? Value { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsOther { get; set; }

        public object Clone()
        {
            return (DealExpenditure)MemberwiseClone();
        }
    }
}
