using System.Text.Json.Serialization;

namespace CatBox.NET.Responses;

/// <summary>
/// Generic CatBox API error response format
/// </summary>
internal sealed record CatBoxApiErrorResponse
{
    [JsonPropertyName("data")]
    public CatBoxApiErrorData? Data { get; init; }

    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("status")]
    public int Status { get; init; }
}

internal sealed record CatBoxApiErrorData
{
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("endpoint")]
    public string? Endpoint { get; init; }

    [JsonPropertyName("method")]
    public string? Method { get; init; }
}
