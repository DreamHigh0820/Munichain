using Blazored.Toast;
using Microsoft.Extensions.Azure;
using Data.DatabaseServices.Repository;
using Domain.Services.ThirdParty;
using Domain.UIProvider;
using Domain.Services.Database;
using Data.DatabaseServices.Repository.Implementations;
using Shared.Models.Chat;
using UI.Components.Weavy.WeavyChat;
using UI.Components.Weavy;
using Hangfire;
using Domain.Services;

namespace UI.Startup
{
    public static class ServiceInjection
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config, string env)
        {
            services.AddScoped<InitialStateProvider>();
            services.AddSingleton<TokenService>();
            services.AddScoped<WeavyJsInterop>();

            // Database Repository
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddScoped<IDealRepository, DealRepository>();
            services.AddScoped<INotificationPreferenceRepository, NotificationPreferenceRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();

            // Business Services
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IDealService, DealService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IConversationService, ConversationService>();
            services.AddScoped<IMessageBoardService, MessageBoardService>();
            services.AddScoped<IFirmsService, FirmsService>();
            services.AddScoped<IBoldsignService, BoldsignService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IAnnotationService, AnnotationService>();
            services.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IDealViewUIProvider, DealViewUIProvider>();
            services.AddSingleton<IBoldsignTokenService, BoldsignTokenService>();
            services.AddScoped<IFileService, FileService>();
            services.AddSingleton<IEmailService, EmailService>();

            services.AddSingleton<NotificationGrouper>();


            if (env == "Offline" || env == "Development")
            {
                services.AddScoped<IAIService, AIMockService>();

            }
            else
            {
                services.AddScoped<IAIService, AIService>();
            }

            services.AddTransient<IDealParticipantService, DealParticipantService>();
            services.AddBlazoredToast();
            services.AddAzureClients(clientBuilder =>
            {
                var connString = env == "Offline" ? "UseDevelopmentStorage=true" : config["AzureBlobStorage:ConnectionString:blob"];
                clientBuilder.AddBlobServiceClient(connString, preferMsi: true);
            });

            return services;
        }
    }
}
