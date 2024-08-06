using System.Net;

namespace VietnamTrafficPolice.WebApi.Services;

public interface IProxyProvider
{
    bool TryGet(out IWebProxy? proxy);
}