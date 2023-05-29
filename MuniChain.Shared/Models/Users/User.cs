using Shared.Helpers;
using Shared.Models.AppComponents;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.Users
{
    public class User
    {
        public object Clone()
        {
            return MemberwiseClone();
        }

        [NotMapped]
        public string BlobUrl { get; set; }
        public User()
        {
            BlobUrl = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production"
                ? "https://stmunichaindev.blob.core.windows.net/profile-pictures" : "https://stmunichainprod.blob.core.windows.net/profile-pictures";
        }

        public string? Id { get; set; }

        // In order to prevent loading profile picture from cache, add random string to blob url.
        [NotMapped]
        public string? ProfilePicUrl => IsLinkedin == true ? $"http://api.linkedin.com/v1/people/{Id}/picture-url" : $"{BlobUrl}/{Id}?{UserExtensions.RandomString(3)}=";
        [NotMapped]
        public Firm? AssociatedFirm { get; set; } = new();
        public string? DisplayName { get; set; }
        public string? City { get; set; }
        public string? JobTitle { get; set; }
        public string? PostalCode { get; set; }
        public string? StateValue { get; set; }
        public string? Email { get; set; }
        [NotMapped]
        public bool? IsLinkedin { get; set; }
        public string? PhoneNumber { get; set; }
        public List<string>? AreasOfExpertise { get; set; }
        public string? Bio { get; set; }
        public bool? Confirmed { get; set; }
        public string? TimeZone { get; set; }
    }
}
