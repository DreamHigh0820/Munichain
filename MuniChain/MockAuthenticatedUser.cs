using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace UI
{
    public class MockAuthenticatedUser : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        const string userId = "581f0c4d-69fa-4789-af6e-cf3a1563f01d";
        const string userName = "Garrett London";
        const string userCity = "Mount Kisco";
        const string userRole = "CPA";

        public MockAuthenticatedUser(
          IOptionsMonitor<AuthenticationSchemeOptions> options,
          ILoggerFactory logger,
          UrlEncoder encoder,
          ISystemClock clock)
          : base(options, logger, encoder, clock) { }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                  new Claim("emails", "glondon@munichain.com"),
                  new Claim(ClaimTypes.NameIdentifier, userId),
                  new Claim("name", userName),
                  new Claim("jobTitle", userRole),
                  new Claim("city", userCity),
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return await Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}