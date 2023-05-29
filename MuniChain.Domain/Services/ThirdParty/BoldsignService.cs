using BoldSign.Api;
using BoldSign.Model;
using Data.DatabaseServices;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services.ThirdParty
{
    public interface IBoldsignService
    {
        Task<SenderIdentityCreated> CreateUser(string email, string userName);

        Task<SenderIdentityViewModel> SearchUser(string email);
        Task RevokeDocument(string id, string onBehalfOfEmail, string userName);
        Task<Dictionary<DocumentProperties, MemoryStream>> GetDocument(string onBehalfOfEmail, string documentId);
        Task<List<BehalfDocument>> ListDocuments(string onBehalfOfEmail, string dealId);
        Task<string> UploadFile(IBrowserFile file, SenderIdentityViewModel user, string dealId, List<DocumentSigner> recipients);
    }

    public class BoldsignService : IBoldsignService
    {
        private Configuration configuration;
        private IBoldsignTokenService _tokenService;

        public BoldsignService(IBoldsignTokenService tokenService, IDbContextFactory<SqlDbContext> factory)
        {
            configuration = new Configuration() { BasePath = "https://api.boldsign.com" };
            _factory = factory;
            _tokenService = tokenService;
        }

        private readonly IDbContextFactory<SqlDbContext> _factory;

        public async Task<Dictionary<DocumentProperties, MemoryStream>> GetDocument(string onBehalfOfEmail, string documentId)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                var accessToken = await _tokenService.GetToken();
                configuration.SetBearerToken(accessToken);
                var apiClient = new ApiClient(configuration);
                var documentClient = new DocumentClient(apiClient);
                MemoryStream memoryStream = new MemoryStream();
                var boldSignDocumentProperties = await documentClient.GetPropertiesAsync(documentId);

                if (boldSignDocumentProperties != null &&
                    (boldSignDocumentProperties.BehalfOf.EmailAddress == onBehalfOfEmail || 
                    boldSignDocumentProperties.SignerDetails.Select(x => x.SignerEmail).Contains(onBehalfOfEmail)))
                {
                    Stream boldSignDocument;
                    try
                    {
                        boldSignDocument = await documentClient.DownloadDocumentAsync(documentId, boldSignDocumentProperties.BehalfOf.EmailAddress);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return null;
                    }

                    await boldSignDocument.CopyToAsync(memoryStream);
                    return new Dictionary<DocumentProperties, MemoryStream>() { { boldSignDocumentProperties, memoryStream } };
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task<List<BehalfDocument>> ListDocuments(string onBehalfOfEmail, string dealId)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                var accessToken = await _tokenService.GetToken();
                configuration.SetBearerToken(accessToken);
                var apiClient = new ApiClient(configuration);
                var documentClient = new DocumentClient(apiClient);

                var docs = new List<BehalfDocument>();
                try
                {
                    var allBoldsignDocs = await documentClient.ListBehalfDocumentsAsync(1, searchKey: dealId);

                    foreach(var document in allBoldsignDocs.Result)
                    {
                        if (document != null && (document.BehalfOf.EmailAddress == onBehalfOfEmail || document.SignerDetails.Select(x => x.SignerEmail).Contains(onBehalfOfEmail)))
                        {
                            docs.Add(document);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return new List<BehalfDocument>();
                }

                return docs;
            }
        }

        public async Task<SenderIdentityCreated> CreateUser(string email, string userName)
        {
            var accessToken = await _tokenService.GetToken();
            configuration.SetBearerToken(accessToken);
            ApiClient apiClient = new ApiClient(configuration);
            var userClient = new SenderIdentityClient(apiClient);

            var identity = new SenderIdentityRequest(userName, email);

            return await userClient.CreateSenderIdentityAsync(identity).ConfigureAwait(false);
        }
        public async Task<SenderIdentityViewModel> SearchUser(string email)
        {
            var accessToken = await _tokenService.GetToken();
            var configuration = new Configuration();
            configuration.SetBearerToken(accessToken);
            ApiClient apiClient = new ApiClient(configuration);
            var userClient = new SenderIdentityClient(apiClient);

            var resp = await userClient.ListSenderIdentitiesAsyncWithHttpInfo(1, search: email);

            return resp.Data.Result.FirstOrDefault();
        }

        public async Task RevokeDocument(string id, string onBehalfOfEmail, string userName)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                var accessToken = await _tokenService.GetToken();
                configuration.SetBearerToken(accessToken);
                var apiClient = new ApiClient(configuration);
                var documentClient = new DocumentClient(apiClient);

                var revokeMessage = $"This document has been revoked by {userName} via MuniChain";
                await documentClient.RevokeDocumentAsyncWithHttpInfo(id, revokeMessage, onBehalfOf: onBehalfOfEmail);
            }
        }

        public async Task<string> UploadFile(IBrowserFile file, SenderIdentityViewModel user, string dealId, List<DocumentSigner> recipients)
        {
            var accessToken = await _tokenService.GetToken();
            configuration.SetBearerToken(accessToken);
            var apiClient = new ApiClient(configuration);
            var documentClient = new DocumentClient(apiClient);

            long maxFileSize = 1024 * 1024 * 25;
            var readStream = file.OpenReadStream(maxFileSize);
            var buf = new byte[readStream.Length];
            var ms = new MemoryStream(buf);
            await readStream.CopyToAsync(ms);
            var buffer = ms.ToArray();

            var embeddedDocumentRequest = new EmbeddedDocumentRequest
            {
                Labels = new List<string> { dealId },
                Title = file.Name,
                EnableSigningOrder = false,
                HideDocumentId = true,
                Signers = recipients,
                BrandId = "0f7ef9b3-e2cb-4308-b8e4-783fd6acb81f",
                Files = new List<IDocumentFile>()
                {
                    new DocumentFileBytes()
                    {
                        // Support wordonly
                        ContentType = "application/pdf",
                        FileName = file.Name,
                        FileData = buffer
                    }
                },
                //customize page options
                SendViewOption = PageViewOption.PreparePage,
                ShowToolbar = false,
                SendLinkValidTill = DateTime.UtcNow.AddDays(20),
                OnBehalfOf = user.Email
            };

            ApiResponse<EmbeddedSendCreated> url;
            try
            {
                url = documentClient.CreateEmbeddedRequestUrlWithHttpInfo(embeddedDocumentRequest);
            }
            catch (Exception)
            {
                return "https://demo.munichain.com/error";
            }

            return url?.Data?.SendUrl?.ToString();
        }
    }
}
