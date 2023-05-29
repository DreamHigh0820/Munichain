using Shared.Models.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.AppComponents
{
    public class Firm
    {
        public object Clone()
        {
            return MemberwiseClone();
        }

        public string? Id { get; set; }
        public string? Name { get; set; }
        [DisplayName("Firm Type")]
        public FirmType? FirmType { get; set; }
        [DisplayName("Phone Number")]
        public string? PhoneNumber { get; set; }
        public string? City { get; set; }
        public States? State { get; set; }
        public bool? Confirmed { get; set; }
        [NotMapped]
        public List<string?> FirmAdminEmails
        {
            get
            {
                return Members?.Where(x => x.IsAdmin).Select(x => x.EmailAddress).ToList();
            }
        }
        public string? Bio { get; set; }
        public List<FirmMember>? Members { get; set; }
    }

    public class FirmMember
    {
        public string? Id { get; set; }
        [DisplayName("Firm ID")]
        public string? FirmId { get; set; }
        public string? UserId { get; set; }
        [DisplayName("Email Address")]
        public string? EmailAddress { get; set; }
        public bool IsAdmin { get; set; }
    }

    public enum FirmType
    {
        [Description("Municipal Advisor")]
        Advisor,
        Issuer,
        [Description("Bond Counsel")]
        BondCounsel
    }
}
