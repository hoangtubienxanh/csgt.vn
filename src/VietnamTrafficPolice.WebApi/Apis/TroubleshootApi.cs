using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using VietnamTrafficPolice.WebApi.Storage.Ef;

namespace VietnamTrafficPolice.WebApi.Apis;

public static class TroubleshootApi
{
    public static void MapTroubleshootApi(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/troubleshoot");

        group.MapGet("/configuration-trace", (HttpContext httpContext,
            IContextualHttpClientFactory contextualHttpClientFactory, IConfiguration configuration) =>
        {
            var config = (configuration as IConfigurationRoot)!.GetDebugView();
            return config;
        });

        group.MapGet("/proxy-trace", async (
            [FromQuery] string? address,
            IContextualHttpClientFactory contextualHttpClientFactory) =>
        {
            address ??= "https://cloudflare.com/cdn-cgi/trace";

            var http = contextualHttpClientFactory.CreateClient("proxified-client", "default");

            var payload = new HttpRequestMessage(HttpMethod.Get, address);
            var responseMessage = await http.SendAsync(payload);

            responseMessage.EnsureSuccessStatusCode();
            return await responseMessage.Content.ReadAsStringAsync();
        });

        group.MapGet("/migrate", async (TrafficIncidentDbContext dbContext) =>
        {
            await dbContext.Database.MigrateAsync();
        });
    }
}