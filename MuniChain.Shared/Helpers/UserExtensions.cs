using Microsoft.AspNetCore.Components.Authorization;
using Shared.Models.Users;
using System.Security.Claims;

namespace Shared.Helpers
{
    public static class UserExtensions
    {
        public static bool ContainsAny(this string haystack, params string[] needles)
        {
            foreach (string needle in needles)
            {
                if (haystack.Contains(needle))
                    return true;
            }

            return false;
        }

        public static User ToUser(this AuthenticationState state)
        {
            var user = new User()
            {
                City = state.User.Claims.GetClaim("city"),
                DisplayName = state.User.Claims.GetClaim("name"),
                Id = state.User.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value,
                JobTitle = state.User.Claims.GetClaim("jobTitle"),
                IsLinkedin = state.User.Claims.GetClaim("http://schemas.microsoft.com/identity/claims/identityprovider")?.Contains("linkedin"),
                StateValue = state.User.Claims.GetClaim("state"),
                PhoneNumber = state.User.Claims.GetClaim("extension_PhoneNumber"),
                PostalCode = state.User.Claims.GetClaim("postalCode"),
                Email = state?.User?.Claims?.FirstOrDefault(x => x.Type == "emails")?.Value
        };

            return user;
        }

        private static string? GetClaim(this IEnumerable<Claim> claims, string claimType)
        {
            return claims?.FirstOrDefault(x => x.Type == claimType)?.Value;
        }

        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string ToUserTimeZone(this DateTime? utcTime, User user)
        {
            if (utcTime.HasValue && !string.IsNullOrEmpty(user.TimeZone))
            {
                return TimeZoneInfo.ConvertTimeFromUtc(utcTime.Value, TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone)).ToString();
            }
            else if (utcTime.HasValue)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(utcTime.Value, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")).ToString();
            }
            return string.Empty;
        }
        public static string ToUserTimeZone(this DateTime utcTime, User user)
        {
            if (!string.IsNullOrEmpty(user.TimeZone))
            {
                return TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone)).ToString();
            }
            else
            {
                return TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")).ToString();
            }
        }
    }
}
