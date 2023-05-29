using Azure.Identity;
using Data.DatabaseServices;
using Domain.Services;
using Domain.Services.Database;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Shared.Models.Config;
using Syncfusion.Blazor;
using System.ComponentModel;
using UI;
using UI.Startup;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
builder.Logging.AddJsonConsole();
var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

// If not offline, we want to use B2C login flow.
if (env != "Offline")
{
    services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddInMemoryTokenCaches();
}

services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});


// If in PROD we want to add key vault, etc.
if (env != "Development" && env != "Offline")
{
    
    var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri"));
    var creds = new DefaultAzureCredential();
    builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, creds);
}
if (env == "Offline")
{
    services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, MockAuthenticatedUser>("OpenIdConnect", null);
}


services.AddControllersWithViews().AddMicrosoftIdentityUI();
services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
    options.AddPolicy("IsAdmin", policy => policy.RequireClaim("emails", new string[] { "glondon@munichain.com", "mgagliano@munichain.com", "mgerstenfeld@munichain.com", "mlieberman@munichain.com", "alingo@munichain.com", "nk2k3406@gmail.com" }));
});
services.AddRazorPages(options => options.RootDirectory = "/Pages");
services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = false;
    options.DisconnectedCircuitMaxRetained = 100;
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
    options.MaxBufferedUnacknowledgedRenderBatches = 10;
}).AddHubOptions(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.EnableDetailedErrors = false;
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.MaximumParallelInvocationsPerClient = 1;
    options.StreamBufferCapacity = 10;

}).AddMicrosoftIdentityConsentHandler();
services.AddSyncfusionBlazor();
services.AddHttpContextAccessor();
services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings().UseMemoryStorage());

services.AddHttpClient();
services.AddHangfireServer();

services.AddSingleton<IUserIdProvider, EmailBasedUserIdProvider>();
if (env != "Offline")
{
    services.AddSignalR(e =>
    {
        e.MaximumReceiveMessageSize = 102400000;
    }).AddAzureSignalR(options =>
    {
        options.ServerStickyMode =
            Microsoft.Azure.SignalR.ServerStickyMode.Required;
    });
}
builder.Services.Configure<OpenAiAuth>(builder.Configuration.GetSection("OpenAi"));
services.AddDbContextFactory<SqlDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        options => options.EnableRetryOnFailure());
}, lifetime: ServiceLifetime.Scoped);

services.AddServices(builder.Configuration, env);

var app = builder.Build();
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(builder.Configuration.GetValue<string>("SyncfusionLicense"));

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard();
app.UseEndpoints(
    endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapBlazorHub(options =>
        {
            options.WebSockets.CloseTimeout = new TimeSpan(1, 1, 1);
            options.LongPolling.PollTimeout = new TimeSpan(1, 0, 0);
        });
        endpoints.MapFallbackToPage("/_Host");
        //endpoints.MapHub<ChatHub>(ChatHub.HubUrl);
        endpoints.MapHub<SignalRHub>(SignalRHub.HubUrl);
    });

app.Run();