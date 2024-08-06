using System.Net;

using VietnamTrafficPolice.WebApi.Configurations;

namespace VietnamTrafficPolice.WebApi.Services;

public class DefaultProxyProviderService(IConfiguration configuration) : IProxyProvider
{
    public bool TryGet(out IWebProxy? webProxy)
    {
        var proxyOptions = new ProxyOptions();
        configuration.GetRequiredSection("ProxyOptions").Bind(proxyOptions);

        if (proxyOptions.Items is not { Length: >= 1 } items)
        {
            var connectionStringWarp = configuration.GetConnectionString("warp");

            if (connectionStringWarp != null)
            {
                webProxy = new WebProxy { Address = new Uri(connectionStringWarp), BypassProxyOnLocal = false };
                return true;
            }
        }

        // May someone will need an example for proxy list?
        // Certainly not me

        webProxy = default;
        return false;
    }
}