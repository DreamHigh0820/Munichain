using Bunit;
using Bunit.TestDoubles;
using Domain.Services.Database;
using Domain.Services.ThirdParty;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Blazor;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Telerik.JustMock;

namespace Tests.TestStartup
{
    public class BaseTestStartup : TestContext
    {
        public IFileService _fileService;
        public IMessageBoardService _messageBoardService;
        public IBoldsignService _boldSignService;
        public IEmailService _emailService;
        public IUserService _userService;
        public IFirmsService _firmService;
        public INotificationService _notificationService;
        public IDocumentService _documentService;
        public IDealParticipantService _dealParticipantService;
        public IDealService _dealService;
        public TestDbContextFactory factory;

        public BaseTestStartup()
        {
            // Add authorization and user claims
            var authContext = this.AddTestAuthorization();
            authContext.SetAuthorized("TEST USER");
            authContext.SetClaims(
                new Claim("jobTitle", "Municipal Advisor"),
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", Guid.NewGuid().ToString()),
                new Claim("emails", "test@munichain.com")
            );

            // Fake Appsettings
            var inMemorySettings = new Dictionary<string, string> { };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            //Mock Services and IOptions
            factory = new TestDbContextFactory();
            _fileService = Mock.Create<IFileService>();
            _messageBoardService = Mock.Create<IMessageBoardService>();
            _boldSignService = Mock.Create<IBoldsignService>();
            _emailService = Mock.Create<IEmailService>();
            _userService = Mock.Create<IUserService>();
            _firmService = Mock.Create<IFirmsService>();
            _notificationService = Mock.Create<INotificationService>();
            _documentService = Mock.Create<IDocumentService>();
            _dealParticipantService = Mock.Create<IDealParticipantService>();
            _dealService = Mock.Create<IDealService>();


            // Inject dependencies
            Services.AddSingleton(_userService);
            Services.AddSingleton(_boldSignService);
            Services.AddSingleton(_messageBoardService);
            Services.AddSingleton(_emailService);
            Services.AddSingleton(_firmService);
            Services.AddSingleton(_fileService);
            Services.AddSingleton(_notificationService);
            Services.AddSingleton(_documentService);
            Services.AddSingleton(_dealParticipantService);
            Services.AddSingleton(_dealService);
            Services.AddSyncfusionBlazor(options =>
            {
                options.IgnoreScriptIsolation = true;
            });
        }
    }
}
