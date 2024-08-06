using System.Diagnostics;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using Polly.Registry;

using TesseractCSharp;

using VietnamTrafficPolice.Apis.Supplement;

namespace VietnamTrafficPolice.WebApi.Services.VietnamTrafficPolice;

internal readonly struct TrafficIncidentService(
    HttpContext httpContext,
    [FromServices] ILogger<TrafficIncidentService> logger,
    [FromServices] IProxifiedVietnamTrafficPoliceClient client,
    [FromServices] TesseractEngine ocrEngine,
    [FromServices] IMemoryCache cache,
    [FromServices] ResiliencePipelineProvider<string> pipelineProvider)
{
    public static string ActivitySourceName = "TrafficIncidentService";

    private readonly ActivitySource _activitySource = new(ActivitySourceName);

    private readonly IHttpActivityFeature? _httpActivityFeature = httpContext.Features.Get<IHttpActivityFeature>();

    private readonly MemoryCacheEntryOptions _cacheEntryOptions = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(15), AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(45)
    };

    public HttpContext HttpContext { get; init; } = httpContext;
    public ILogger<TrafficIncidentService> Logger { get; init; } = logger;
    public IProxifiedVietnamTrafficPoliceClient Client { get; } = client;
    public TesseractEngine OcrEngine { get; } = ocrEngine;
    public IMemoryCache Cache { get; } = cache;
    public ResiliencePipelineProvider<string> PipelineProvider { get; } = pipelineProvider;

    public async Task<TrafficIncidentResponse> GetTrafficIncidentById(TrafficIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var service = this;
        var pipeline = PipelineProvider.GetPipeline("vietnam-traffic-police-captcha-retry");

        // Rider suggests "Closure can be eliminated: method has overload to avoid closure creation"
        // PR Welcome :)
        return await pipeline.ExecuteAsync(async _ => await service.Handler(request));
    }

    private async ValueTask<TrafficIncidentResponse> Handler(TrafficIncidentRequest request)
    {
        using var activity =
            _activitySource.StartActivity("Execute operation GET traffic incident data", ActivityKind.Client);

        if (Cache.TryGetValue("traffic-incident-captcha", out string? cachedToken) && cachedToken is not null)
        {
            var partialRequestResponse = await Client.GetTrafficIncidentById(request, cachedToken);
            if (partialRequestResponse is { Succeeded: true })
            {
                activity?.SetTag("challenge.solution", cachedToken);
                return partialRequestResponse;
            }

            Cache.Remove("traffic-incident-captcha");
            throw new ApiException("Couldn't GET traffic incident data (token expired)");
        }

        var service = this;
        var (rawImageData, token) = await Client.SolveTrafficIncidentCaptchaChallenge(bytes =>
        {
            using var pix = Pix.LoadFromMemory(bytes);
            using var page = service.OcrEngine.Process(pix);
            return Task.FromResult(page.GetText()!.Trim());
        });

        activity?.SetTag("challenge.body", Convert.ToBase64String(rawImageData));
        activity?.SetTag("challenge.solution", token);

        var response = await Client.GetTrafficIncidentById(request);
        if (response is not { Succeeded: true })
        {
            throw new ApiException("Couldn't GET traffic incident data");
        }

        Cache.Set("traffic-incident-captcha", token, _cacheEntryOptions);
        return response;
    }
}