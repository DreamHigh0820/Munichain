using BoldSign.Model.Webhook;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.Models.AppComponents;
using Shared.Models.Enums;
using Data.DatabaseServices.Repository.Implementations;
using Data.DatabaseServices;

namespace Data.Controllers
{
    [Route("webhook")]
    [AllowAnonymous]
    [ApiController]
    public class WebhookController : Controller
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IDealRepository _dealService;

        public WebhookController(ILogger<WebhookController> logger, IDbContextFactory<SqlDbContext> factory, IDealRepository dealService)
        {
            _logger = logger;
            _dealService = dealService;
            _factory = factory;
        }

        private readonly IDbContextFactory<SqlDbContext> _factory;

        [HttpPost("receive")]
        [AllowAnonymous]
        public async Task<IActionResult> Receive(object message)
        {
            WebhookEvent webhookEvent = new();
            try
            {
                webhookEvent = JsonConvert.DeserializeObject<WebhookEvent>(message?.ToString());
            }
            catch (Exception ex)
            {
                // log why unable to deserialize message
                _logger.LogError("Error reading webhook message: " + ex.Message);
                return Ok();
            }

            var DealId = webhookEvent?.Document?.Labels?.FirstOrDefault();
            if (DealId == null)
            {
                return Ok();
            }
            var deal = await _dealService.GetById(DealId);

            if (deal == null)
            {
                return Ok();
            }

            try
            {
                using (var _dbContext = _factory.CreateDbContext())
                {
                    var notifications = new List<Notification>();
                    var action = Enum.TryParse<NotificationAction>(webhookEvent?.Event?.EventType.ToString(), out var eTemp);
                    if (eTemp == null)
                    {
                        // Unsupported notification for Boldsign Action
                        return Ok();
                    }

                    foreach (var recipient in webhookEvent?.Document?.SignerDetails)
                    {
                        notifications.Add(new Notification(null, null, deal, recipient.SignerEmail)
                        {
                            Action = eTemp,
                            DocumentName = webhookEvent?.Document?.MessageTitle,
                            DocumentId = webhookEvent?.Document?.DocumentId,
                            ActionBy = webhookEvent?.Document?.SenderDetail.EmailAddress,
                        });
                    }

                    _dbContext.Notifications.AddRange(notifications);
                    await _dbContext.SaveChangesAsync();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error reading webhook message: " + ex.Message);
                throw new Exception("Error reading webhook message: " + ex.Message + " : event: " + webhookEvent);
            }

        }
    }
}
