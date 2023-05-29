using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;

namespace Domain.Services.ThirdParty
{
    public interface IBoldsignTokenService
    {
        Task<string> GetToken();
    }

    public class BoldsignTokenService : IBoldsignTokenService
    {
        private string? _token;

        public async Task<string> GetToken()
        {
            if (_isEmptyOrInvalid(_token))
            {
                using var http = new HttpClient();
                // need to add required scopes.
                var parameters = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("scope", "BoldSign.Documents.All BoldSign.Templates.All BoldSign.Users.All BoldSign.SenderIdentity.All")
                };

                using var encodedContent = new FormUrlEncodedContent(parameters);

                using var request = new HttpRequestMessage()
                {
                    Content = encodedContent,
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://account.boldsign.com/connect/token"),
                };

                //clientid for get access token
                const string clientId = "9dc83e01-9218-4d56-9b8c-8fc30ee1c8b6";

                //clientsecret for get access token
                const string clientSecret = "f181fb97-0ec0-404c-826d-4f9973e4310b";

                var encodedAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedAuth);

                //send request for fectch access token
                using var response = await http.SendAsync(request).ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var tokenResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
                tokenResponse?.TryGetValue("access_token", out _token);

                return _token;
            }

            return _token;
        }

        private bool _isEmptyOrInvalid(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return true;
            }

            var jwtToken = new JwtSecurityToken(token);
            return jwtToken == null || jwtToken.ValidFrom > DateTime.UtcNow || jwtToken.ValidTo < DateTime.UtcNow;
        }
    }
}
