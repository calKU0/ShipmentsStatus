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
using ShipmentsStatus.Service.Services;
using System.Net.Http.Headers;

var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = ServiceConstants.ServiceName;
    })
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        var logsExpirationDays = Convert.ToInt32(configuration["AppSettings:LogsExpirationDays"]);
        Directory.CreateDirectory(logDirectory);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(
                path: Path.Combine(logDirectory, "log-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: logsExpirationDays,
                shared: true,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .MinimumLevel.Override("System.Net.Http.HttpClient", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .CreateLogger();

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
            client.BaseAddress = new Uri(settings.BaseUrl);
        });

        // DPD-Romania REST client
        services.AddHttpClient<DpdRomaniaService>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<CourierSettings>>().Value.DPDRomania;
            client.BaseAddress = new Uri(settings.BaseUrl);
        });

        // FedEx REST client
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
    .UseSerilog()
    .Build();

host.Run();