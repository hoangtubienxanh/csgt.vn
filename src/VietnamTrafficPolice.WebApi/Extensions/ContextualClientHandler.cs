using Microsoft.Extensions.Options;

using VietnamTrafficPolice.Configurations;
using VietnamTrafficPolice.WebApi.Services;

namespace VietnamTrafficPolice.WebApi.Extensions;

internal static class ContextualClientHandler
{
    internal static HttpMessageHandler? ProxifiedHttpClient(
        IServiceProvider sp)
    {
        var proxyProvider = sp.GetRequiredService<IProxyProvider>();
        if (proxyProvider.TryGet(out var webProxy) is false)
        {
            return default;
        }

        var handler = new SocketsHttpHandler { UseProxy = true, Proxy = webProxy };
        return handler;
    }

    internal static SocketsHttpHandler? ProxifiedVietnamTrafficPoliceClient(
        IServiceProvider sp)
    {
        var options = sp.GetRequiredService<IOptionsMonitor<VietnamTrafficPoliceCitizenPortalOptions>>()
            .CurrentValue;

        var proxyProvider = sp.GetService<IProxyProvider>();
        if (proxyProvider is null || proxyProvider.TryGet(out var proxy) is false)
        {
            return default;
        }

        var handler = new SocketsHttpHandler();
        handler.UseProxy = true;
        handler.Proxy = proxy;

        return handler;
    }
}