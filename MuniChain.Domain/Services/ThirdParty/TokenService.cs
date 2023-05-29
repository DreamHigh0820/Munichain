using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Shared.Models.Users;
using Newtonsoft.Json;
using System.Text;

namespace Domain.Services.ThirdParty
{
    public class TokenService
    {
        private HttpClient httpClient;
        public TokenService(IHttpClientFactory factory)
        {
            httpClient = factory.CreateClient();
        }

        public async Task<string> GetToken(User user)
        {
            var uid = "u" + user.Id;
            //Request
            using var request = new HttpRequestMessage(HttpMethod.Post, $"https://munichain-dev.weavy.io/api/users/{HttpUtility.UrlEncode(uid)}/tokens");

            //Headers
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Cache-Control", "no-cache");

            //Payload
            var payload = JsonConvert.SerializeObject(
                new
                {
                    email = user?.Email,
                    name = user?.DisplayName,
                    picture = user?.ProfilePicUrl
                });
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
            // no token in store or invalid token -> request a new access_token from the Weavy backend
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "wys_aQIQin4moT4Z1Z55eyhjFSlokHjvm11XXMmn");
            var response = await httpClient.SendAsync(request);

            string bearerToken;

            if (response.IsSuccessStatusCode)
            {
                // store token
                var bearerData = await response.Content.ReadAsStringAsync();
                bearerToken = JObject.Parse(bearerData)["access_token"].ToString();
                return bearerToken;
            }
            else
            {
                return null;
            }

        }
    }
}
