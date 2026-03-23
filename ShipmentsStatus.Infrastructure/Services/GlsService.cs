using Microsoft.Extensions.Options;
using ShipmentsStatus.Contracts.DTOs;
using ShipmentsStatus.Contracts.DTOs.GLS_Api;
using ShipmentsStatus.Contracts.Services;
using ShipmentsStatus.Contracts.Settings;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ShipmentsStatus.Infrastructure.Services
{
    public class GlsService : ICourierService
    {
        private readonly HttpClient _httpClient;
        private readonly GlsSettings _settings;
        private TokenDto _authToken;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public GlsService(IOptions<GlsSettings> settings, HttpClient httpClient)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
        }
        public async Task<ShipmentStatusResponse> GetShipmentStatus(ShipmentStatusRequest request)
        {
            await EnsureLoggedIn();

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken.Token);

            var response = await _httpClient.GetAsync($"/track-and-trace-v1/tracking/simple/references/{request.TrackingNumber}?showLinks=false&showEvents=false");

            response.EnsureSuccessStatusCode();

            var trackingResponse = await response.Content.ReadFromJsonAsync<GlsTrackingResponse>(_jsonOptions);
            var parcel = trackingResponse?.Parcels?.FirstOrDefault();

            return new ShipmentStatusResponse
            {
                TrackingNumber = request.TrackingNumber,
                Date = ParseGlsDateTime(parcel?.StatusDateTime),
                Status = parcel?.Status ?? "Not found",
            };
        }

        private async Task EnsureLoggedIn()
        {
            if (_authToken != null && _authToken.ExpiresAt > DateTime.UtcNow)
                return;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}")));

            using var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials"
            });

            var response = await _httpClient.PostAsync("oauth2/v2/token", form);

            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadFromJsonAsync<GlsOAuthResponse>(_jsonOptions);

            _authToken = new TokenDto
            {
                Token = token.AccessToken,
                ExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn - 60)
            };
        }

        private static DateTime ParseGlsDateTime(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DateTime.Now;
            }

            var normalized = value.Trim();

            if (normalized.Length >= 5 &&
                (normalized[^5] == '+' || normalized[^5] == '-') &&
                normalized[^3] != ':')
            {
                normalized = normalized.Insert(normalized.Length - 2, ":");
            }

            if (DateTimeOffset.TryParse(
                    normalized,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces,
                    out var dto))
            {
                return dto.DateTime;
            }

            return DateTime.Now;
        }
    }
}
