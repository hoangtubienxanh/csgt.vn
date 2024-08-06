using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;

using VietnamTrafficPolice.Apis.Supplement;
using VietnamTrafficPolice.WebApi.Services.VietnamTrafficPolice;

namespace VietnamTrafficPolice.WebApi.Apis;

public static class TrafficIncidentApi
{
    public static void MapTrafficIncidentApi(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("");

        group.MapGet("/traffic-incident", GetTrafficIncidentById)
            .Produces(StatusCodes.Status500InternalServerError);
    }

    [EndpointSummary("Find traffic incidents by Id (vehicle registration plate)")]
    [EndpointDescription("Returns traffic incidents from 3rd party api")]
    private static async Task<Results<Ok<TrafficIncidentApiResponse>, NotFound>> GetTrafficIncidentById(
        [FromQuery] string plateId,
        [FromServices] HybridCache cache,
        [AsParameters] TrafficIncidentService service,
        CancellationToken cancellationToken = default)
    {
        var entryOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(60), LocalCacheExpiration = TimeSpan.FromMinutes(60)
        };

        var store = await cache.GetOrCreateAsync(
            plateId,
            async cancel =>
            {
                var data = await service.GetTrafficIncidentById(
                    new TrafficIncidentRequest(plateId, VehicleType.Car),
                    cancel);
                return data.ToTrafficIncidentApiResponse();
            },
            entryOptions,
            token: cancellationToken
        );

        if (store.TrafficViolations is { Count: 0 })
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(store);
    }
}

internal record TrafficIncidentApiResponse(ICollection<TrafficIncident>? TrafficViolations);

internal static class TrafficIncidentResponseExtensions
{
    internal static TrafficIncidentApiResponse ToTrafficIncidentApiResponse(this TrafficIncidentResponse response)
    {
        return new TrafficIncidentApiResponse(response.TrafficViolations);
    }
}