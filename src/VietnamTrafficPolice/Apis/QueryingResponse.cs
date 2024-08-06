using System.Text.Json.Serialization;

namespace VietnamTrafficPolice.Apis;

internal record QueryingResponse
{
    [JsonPropertyName("success")] public string? Status { get; init; }
    [JsonPropertyName("href")] public string? RedirectUrl { get; init; }

    public bool Succeeded => Status is "true" && RedirectUrl is not null;
}