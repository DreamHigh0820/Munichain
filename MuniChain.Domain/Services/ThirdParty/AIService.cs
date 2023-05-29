using Microsoft.Extensions.Options;
using OpenAI;
using Shared.Models.Config;
using Shared.Models.DealComponents;
using Shared.Models.Users;

namespace Domain.Services.ThirdParty
{
    public interface IAIService
    {
        Task GetBio(string messageToPost, User user, Delegate handler);
        Task GetMessageBoard(DealModel deal, List<DealParticipant> participants, User user, Delegate handler);
    }

    public class AIService : IAIService
    {
        private OpenAIClient apiClient;

        public AIService(IOptions<OpenAiAuth> auth)
        {
            var apiKey = auth.Value.ApiKey;
            apiClient = new OpenAIClient(authentication: apiKey, new Engine("text-davinci-003"));
        }

        public async Task GetBio(string messageToPost, User user, Delegate handler)
        {
            var query = "Write a short paragraph for a ";
            query += string.IsNullOrEmpty(user.JobTitle) ? "business person" : user.JobTitle;
            query += " from ";
            query += user.City + " " + user.StateValue;
            query += " named " + user.DisplayName;
            query += " that is dedicated to clients, community, and family";
            query += string.IsNullOrEmpty(messageToPost) ? "" : " specializing in " + messageToPost;
            if (user?.AreasOfExpertise != null && user?.AreasOfExpertise.Any() == true)
            {
                var areasOfExpertise = string.Join(", ", user.AreasOfExpertise);
                query += string.IsNullOrEmpty(areasOfExpertise) ? "" : " and " + areasOfExpertise;
            }

            var res = "";
            await apiClient.CompletionEndpoint.StreamCompletionAsync(result =>
            {
                var first = result.Completions.FirstOrDefault();
                res += first?.Text;
                if (first?.Text != "\n")
                {
                    handler.DynamicInvoke(first?.Text);
                }
            }, query, max_tokens: 256, top_p: 1, temperature: 0.7, presencePenalty: 0, frequencyPenalty: 0);
        }

        public async Task GetMessageBoard(DealModel deal, List<DealParticipant> participants, User user, Delegate handler)
        {
            var query = $"Write paragraphs for the new launch of a {deal.OfferingType} municipal bond transaction with the name ";
            query += deal.Issuer;
            query += " in ";
            query += deal.State;
            query += " and that is launching on " + deal.SaleDateUTC + " . The keywords should be bold.";

            List<string> addlQueries = new List<string>();
            addlQueries.Add("The person who is running this transaction is very excited about how this will shape the future of financing for the community and save taxpayers money through an efficient bond issuance process.");
            addlQueries.Add("Ensuring integrity of transactions and working alongside all participants in the new issue process makes a more effective capital raising situation.");

            var res = "";
            await apiClient.CompletionEndpoint.StreamCompletionAsync(result =>
            {
                var first = result.Completions.FirstOrDefault();
                if (!string.IsNullOrEmpty(first.Text) && first.Text != "\n")
                {
                    res += first.Text;
                    handler.DynamicInvoke(res, false);
                }
            }, query, addlQueries.ToArray(), max_tokens: 256, top_p: 1, temperature: 0.7, presencePenalty: 0, frequencyPenalty: 0);

            handler.DynamicInvoke(res, true);

        }
    }

    public class AIMockService : IAIService
    {
        public AIMockService()
        {
        }

        public async Task GetBio(string messageToPost, User user, Delegate handler)
        {
            return;
        }

        public async Task GetMessageBoard(DealModel deal, List<DealParticipant> participants, User user, Delegate handler)
        {
            return;

        }
    }
}
