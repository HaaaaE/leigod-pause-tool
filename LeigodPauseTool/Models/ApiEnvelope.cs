using System.Text.Json.Serialization;

namespace LeigodPauseTool.Models;

public record ApiEnvelope
{
    [JsonPropertyName("code")]
    public int Code { get; init; }

    [JsonPropertyName("msg")]
    public required string Msg { get; init; }
}

public enum PauseStatus
{
    Paused,
    Resumed,
    NoAuth
}