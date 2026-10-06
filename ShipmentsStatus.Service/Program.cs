using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using ShipmentsStatus.Contracts.Repositories;
using ShipmentsStatus.Contracts.Settings;
using ShipmentsStatus.Infrastructure.Data;
using ShipmentsStatus.Infrastructure.Repositories;
using ShipmentsStatus.Infrastructure.Services;
using ShipmentsStatus.Infrastructure.Services.Strategies;
using ShipmentsStatus.Service;
using ShipmentsStatus.Service.Constants;
using ShipmentsStatus.Service.Logging;
using ShipmentsStatus.Service.Services;
using System.Net.Http.Headers;
using System.Text;

var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = ServiceConstants.ServiceName;
    })
    .UseSerilog((hostContext, _, loggerConfiguration) =>
    {
        loggerConfiguration.ConfigureServiceLogging(hostContext.Configuration);
    })
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

        // Configuration
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.Configure<CourierSettings>(configuration.GetSection("CourierApis"));
        services.Configure<DpdSettings>(configuration.GetSection("CourierApis:DPD"));
        services.Configure<DpdRomaniaSettings>(configuration.GetSection("CourierApis:DPDRomania"));
        services.Configure<GlsSettings>(configuration.GetSection("CourierApis:GLS"));

        // Database context
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddSingleton<IDbExecutor>(sp => new DapperDbExecutor(connectionString));

        // HttpClients
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError() // Handles 5xx and 408
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));


        // GLS REST client
        services.AddHttpClient<GlsService>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<CourierSettings>>().Value.GLS;
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(
                        Encoding.UTF8.GetBytes($"{settings.Username}:{settings.Password}")
                    ));
            client.BaseAddress = new Uri(settings.BaseUrl);
        });

        // DPD-Romania REST client
        services.AddHttpClient<DpdRomaniaService>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<CourierSettings>>().Value.DPDRomania;
            client.BaseAddress = new Uri(settings.BaseUrl);
        });

        // FedEx REST client (OAuth token handled in FedexRestStrategy)
        services.AddHttpClient<FedexRestStrategy>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<CourierSettings>>().Value.Fedex.Rest;
            client.BaseAddress = new Uri(settings.BaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        // Repositories
        services.AddScoped<IShipmentRepository, ShipmentRepository>();

        // Services
        services.AddScoped<CourierFactory>();
        services.AddScoped<DpdService>();
        services.AddScoped<FedexService>();
        services.AddScoped<FedexSoapStrategy>();
        services.AddScoped<IShipmentStatusSyncService, ShipmentStatusSyncService>();

        // Background worker
        services.AddHostedService<Worker>();

        // Host options
        services.Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromSeconds(15));
    })
    .Build();

host.Run();