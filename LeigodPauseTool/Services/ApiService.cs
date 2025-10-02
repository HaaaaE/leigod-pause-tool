using LeigodPauseTool.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LeigodPauseTool.Services;

public class ApiService : IApiService
{
    private const string BaseUrl = "https://webapi.leigod.com";
    private const string PausePath = "/api/user/pause";
    private const string InfoPath = "/api/user/info";

    private static readonly HttpClient HttpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public async Task<ApiEnvelope> Pause(string accountToken)
    {
        try
        {
            var body = new { account_token = accountToken, lang = "zh_CN" };
            var payload = JsonSerializer.Serialize(body, JsonOptions);

            var response = await HttpClient.PostAsync(
                $"{BaseUrl}{PausePath}",
                new StringContent(payload, Encoding.UTF8, "application/json")
            );

            var text = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ApiEnvelope
                {
                    Code = (int)response.StatusCode,
                    Msg = $"HTTP {(int)response.StatusCode}: {text}",
                };
            }

            return JsonSerializer.Deserialize<ApiEnvelope>(text, JsonOptions) ?? throw new Exception();
        }
        catch (Exception ex)
        {
            return new ApiEnvelope
            {
                Code = -1,
                Msg = $"Exception: {ex.Message}",
            };
        }
    }

    public async Task<PauseStatus> Info(string accountToken)
    {
        try
        {
            var body = new { account_token = accountToken, lang = "zh_CN" };
            var payload = JsonSerializer.Serialize(body, JsonOptions);

            var response = await HttpClient.PostAsync(
                $"{BaseUrl}{InfoPath}",
                new StringContent(payload, Encoding.UTF8, "application/json")
            );

            var text = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return PauseStatus.NoAuth;
            }

            using var doc = JsonDocument.Parse(text);
            if (doc.RootElement.TryGetProperty("data", out var data) &&
                data.TryGetProperty("pause_status_id", out var pauseStatus) &&
                pauseStatus.TryGetInt32(out var intValue))
            {
                return intValue switch
                {
                    1 => PauseStatus.Paused,
                    0 => PauseStatus.Resumed,
                    _ => PauseStatus.NoAuth
                };
            }

            return PauseStatus.NoAuth;
        }
        catch
        {
            return PauseStatus.NoAuth;
        }
    }
}