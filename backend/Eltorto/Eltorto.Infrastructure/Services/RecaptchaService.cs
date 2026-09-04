using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Eltorto.Application.Interfaces.Services;
using Eltorto.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eltorto.Infrastructure.Services;

public class RecaptchaService : IRecaptchaService
{
    private readonly HttpClient _httpClient;
    private readonly RecaptchaSettings _settings;
    private readonly ILogger<RecaptchaService> _logger;

    private const string VerifyUrl = "https://www.google.com/recaptcha/api/siteverify";

    public RecaptchaService(
        IHttpClientFactory httpClientFactory,
        IOptions<RecaptchaSettings> settings,
        ILogger<RecaptchaService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> VerifyTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("[RECAPTCHA] Empty token received");
            return false;
        }

        try
        {
            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", _settings.SecretKey),
                new KeyValuePair<string, string>("response", token)
            });

            var response = await _httpClient.PostAsync(VerifyUrl, requestBody, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<RecaptchaResponse>(cancellationToken: cancellationToken);

            if (result == null)
            {
                _logger.LogWarning("[RECAPTCHA] Null response from Google");
                return false;
            }

            if (!result.Success)
            {
                _logger.LogWarning("[RECAPTCHA] Verification failed: {Errors}", string.Join(", ", result.ErrorCodes ?? []));
                return false;
            }

            _logger.LogInformation("[RECAPTCHA] Verified: score={Score}, action={Action}", result.Score, result.Action);

            if (result.Score < _settings.ScoreThreshold)
            {
                _logger.LogWarning("[RECAPTCHA] Rejected: score={Score} < threshold={Threshold}", result.Score, _settings.ScoreThreshold);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RECAPTCHA] Token verification error");
            return false;
        }
    }
}

internal class RecaptchaResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("score")]
    public double Score { get; set; }

    [JsonPropertyName("action")]
    public string? Action { get; set; }

    [JsonPropertyName("error-codes")]
    public List<string>? ErrorCodes { get; set; }
}
