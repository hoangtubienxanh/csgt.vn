using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using AngleSharp.Html.Parser;

using Microsoft.Extensions.Options;

using VietnamTrafficPolice.Apis;
using VietnamTrafficPolice.Apis.Page;
using VietnamTrafficPolice.Apis.Supplement;
using VietnamTrafficPolice.Configurations;

namespace VietnamTrafficPolice;

/// <inheritdoc cref="IVietnamTrafficPoliceClient" />
public class VietnamTrafficPoliceClient : IVietnamTrafficPoliceClient, IProxifiedVietnamTrafficPoliceClient
{
    private const string TrafficIncidentRequestIdentifier = "/?mod=contact&task=tracuu_post&ajax";
    private const string TrafficIncidentResponseIdentifier = "/tra-cuu-phuong-tien-vi-pham.html";

    private static readonly string SdkVersion =
        typeof(VietnamTrafficPoliceClient).Assembly.GetName().Version?.ToString(3) ??
        // This should never happen, unless the assembly had its metadata trimmed
        "unknown";

    private readonly HttpClient _http;
    private readonly VietnamTrafficPoliceCitizenPortalOptions _options;
    private readonly IHtmlParser _parser;

    public VietnamTrafficPoliceClient(HttpClient http,
        IOptionsMonitor<VietnamTrafficPoliceCitizenPortalOptions> options,
        IHtmlParser parser)
    {
        _http = http;
        _options = options.CurrentValue;
        _parser = parser;

        _http.BaseAddress = new Uri(_options.ApiUrl);
        _http.DefaultRequestHeaders.Add("Client-Version", $".NET-{SdkVersion}");
        _http.DefaultRequestVersion = HttpVersion.Version11; // Required for compatibility
    }

    public string TrafficIncidentCaptchaSolution { get; private set; } = string.Empty;

    /// <inheritdoc />
    public async Task<TrafficIncidentResponse> GetTrafficIncidentById(TrafficIncidentRequest request)
    {
        var dictionary = request.ToDictionary();

        // Required parameter for query. Without this, the query will fail.
        dictionary.Add("ipClient", "9.9.9.91");
        dictionary.Add("captcha", TrafficIncidentCaptchaSolution);

        if (await DoQuerying(dictionary, TrafficIncidentRequestIdentifier) is not { Succeeded: true })
        {
            return new TrafficIncidentResponse(false, default, default);
        }

        var response = await _http.GetAsync(TrafficIncidentResponseIdentifier);
        response.EnsureSuccessStatusCode();

        var htmlized = await _parser.ParseDocumentAsync(await response.Content.ReadAsStringAsync());
        var contents = TrafficIncidentPage.CrawlTrafficIncidentData(htmlized);

        return new TrafficIncidentResponse(true,
            response.Headers.Date?.UtcDateTime ?? default,
            contents.Select(data => data.ToTrafficIncident()).ToList());
    }

    /// <inheritdoc />
    public async Task<TrafficIncidentResponse> GetTrafficIncidentById(TrafficIncidentRequest request,
        string trafficIncidentCaptchaSolution)
    {
        TrafficIncidentCaptchaSolution = trafficIncidentCaptchaSolution;
        return await GetTrafficIncidentById(request);
    }

    /// <inheritdoc />
    public async Task<(byte[], string)> SolveTrafficIncidentCaptchaChallenge(
        Func<byte[], Task<string>> onChallengeReceived)
    {
        var challenge = await GetTrafficIncidentCaptchaChallenge();
        var answer = await onChallengeReceived(challenge);

        TrafficIncidentCaptchaSolution = answer;

        return (challenge, answer);
    }

    private async Task<QueryingResponse?> DoQuerying(Dictionary<string, string> dictionary, string requestUri)
    {
        FormUrlEncodedContent formUrlEncodedContent = new(dictionary);
        var response = await _http.PostAsync(requestUri, formUrlEncodedContent);
        response.EnsureSuccessStatusCode();

        QueryingResponse? queryingResponse = default;
        try
        {
            queryingResponse =
                await response.Content.ReadFromJsonAsync(VietnamTrafficPoliceContext.Default.QueryingResponse);
        }
        catch (Exception e) when (e is JsonException)
        {
            // Possible text/HTML content.
            // A "404" response indicates either invalid form data or a failed CAPTCHA challenge.
            // If not a "404", the query string is likely incorrect. 
        }

        return queryingResponse;
    }

    private async Task<byte[]> GetTrafficIncidentCaptchaChallenge()
    {
        return await _http.GetByteArrayAsync("/lib/captcha/captcha.class.php");
    }
}