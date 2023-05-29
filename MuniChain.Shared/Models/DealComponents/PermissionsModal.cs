using System.ComponentModel;

namespace Shared.Models.DealComponents
{
    public class PermissionsModal
    {
        [DisplayName("Deal Permission")]
        public string DealPermission { get; set; }
        [DisplayName("Expenditure Permission")]
        public string ExpenditurePermission { get; set; }
        [DisplayName("Performance Permission")]
        public string PerformancePermission { get; set; }
        [DisplayName("Admin Status")]
        public bool IsAdmin { get; set; }
    }
}
