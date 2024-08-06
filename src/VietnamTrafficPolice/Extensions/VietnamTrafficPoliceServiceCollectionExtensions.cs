using AngleSharp.Html.Parser;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using VietnamTrafficPolice.Configurations;

namespace VietnamTrafficPolice.Extensions;

public static class VietnamTrafficPoliceServiceCollectionExtensions
{
    /// <summary>
    ///     Adds and configures VietnamTrafficPolice-related services.
    /// </summary>
    public static IServiceCollection AddVietnamTrafficPoliceClient(this IServiceCollection services,
        Action<VietnamTrafficPoliceCitizenPortalOptions> configureOptions)
    {
        services.AddOptions<VietnamTrafficPoliceCitizenPortalOptions>()
            .Configure(configureOptions)
            .Validate(options => !string.IsNullOrEmpty(options.ApiUrl),
                "VietnamTrafficPoliceCitizenPortal: Missing ApiUrl");

        services.RegisterDependencies();
        return services;
    }

    private static void RegisterDependencies(this IServiceCollection services)
    {
        services.TryAddSingleton<IHtmlParser, HtmlParser>();

        services.AddTransient<VietnamTrafficPoliceSessionHandler>();

        services.AddHttpClient<IVietnamTrafficPoliceClient, VietnamTrafficPoliceClient>((http, sp) =>
            {
                var options = sp.GetRequiredService<IOptionsMonitor<VietnamTrafficPoliceCitizenPortalOptions>>();
                // .CurrentValue;
                return new VietnamTrafficPoliceClient(http, options, sp.GetRequiredService<IHtmlParser>());
            })
            .AddHttpMessageHandler<VietnamTrafficPoliceSessionHandler>()
            .UseContextualCookies();
    }
}