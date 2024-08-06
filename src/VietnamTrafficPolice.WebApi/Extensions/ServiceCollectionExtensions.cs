using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Polly;
using Polly.Retry;
using Polly.Timeout;

using TesseractCSharp;

using VietnamTrafficPolice.Extensions;
using VietnamTrafficPolice.WebApi.Configurations;
using VietnamTrafficPolice.WebApi.Services;
using VietnamTrafficPolice.WebApi.Services.VietnamTrafficPolice;
using VietnamTrafficPolice.WebApi.Storage.Ef;

namespace VietnamTrafficPolice.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        services.AddOpenApi();
        services.AddProblemDetails();

        builder.AddMySqlDbContext<TrafficIncidentDbContext>("traffic-incident-context");
        services.AddDistributedMySqlCache(options =>
        {
            options.ConnectionString = configuration.GetConnectionString("traffic-incident-context");
            options.TableName = "DistributedCache";
        });
        services.AddHybridCache();

        services.AddCors(options
            => options.AddPolicy("default", cors =>
                cors.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()));

        services.TryAddSingleton(TimeProvider.System);

        services.AddVietnamTrafficIntegration(configuration);

        // Override behavior for handler generation when criteria are met
        services.AddContextualHttpClientFactory((client, _, sp) =>
        {
            return client switch
            {
                "proxified-client" => ContextualClientHandler.ProxifiedHttpClient(sp),
                nameof(VietnamTrafficPoliceClient) => ContextualClientHandler.ProxifiedVietnamTrafficPoliceClient(sp),
                _ => default
            };
        });

        return builder;
    }

    private static void AddVietnamTrafficIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        // Enable forward proxy support for Vietnam's traffic police client
        services.AddSingleton<IProxyProvider, DefaultProxyProviderService>();

        services.AddVietnamTrafficPoliceClient(configuration
            .GetRequiredSection("TrafficPoliceOptions").Bind);

        // Add Per-request proxy client
        services.AddHttpClient<IProxifiedVietnamTrafficPoliceClient, VietnamTrafficPoliceClient>((http, sp) =>
        {
            var contextClientFactory = sp.GetRequiredService<IContextualHttpClientFactory>();
            return contextClientFactory.CreateClient<VietnamTrafficPoliceClient>("default");
        });

        services.AddResiliencePipeline("vietnam-traffic-police-captcha-retry", static builder =>
        {
            // See: https://www.pollydocs.org/strategies/retry.html
            builder.AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<TimeoutRejectedException>()
                    .Handle<ApiException>(),
                MaxRetryAttempts = 5
            });

            // See: https://www.pollydocs.org/strategies/timeout.html
            builder.AddTimeout(TimeSpan.FromSeconds(5));
        });

        services.Configure<TesseractEngineOptions>(configuration
            .GetRequiredSection(TesseractEngineOptions.Section).Bind);

        services.AddTransient<TesseractEngine>(sp =>
        {
            var (directoryPath, language) =
                sp.GetRequiredService<IOptionsMonitor<TesseractEngineOptions>>().CurrentValue;
            var engine = new TesseractEngine(directoryPath, language, EngineMode.Default);
            return engine;
        });

        services.AddOpenTelemetry().WithTracing(tracing =>
            tracing.AddSource(TrafficIncidentService.ActivitySourceName));

        services.AddMemoryCache();
    }
}