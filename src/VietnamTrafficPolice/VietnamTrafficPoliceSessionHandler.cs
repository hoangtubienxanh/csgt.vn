namespace VietnamTrafficPolice;

internal class VietnamTrafficPoliceSessionHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.SetCookieContext("primary");
        return await base.SendAsync(request, cancellationToken);
    }
}