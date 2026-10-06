using Microsoft.Extensions.Options;
using ShipmentsStatus.Contracts.DTOs;
using ShipmentsStatus.Contracts.DTOs.Fedex_Api;
using ShipmentsStatus.Contracts.Settings;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ShipmentsStatus.Infrastructure.Services.Strategies
{
    public class FedexRestStrategy : IFedexApiStrategy
    {
        // Token jest ważny 60 min - współdzielony między instancjami (typed HttpClient jest transient)
        private static readonly SemaphoreSlim _tokenLock = new SemaphoreSlim(1, 1);
        private static TokenDto? _token;

        private readonly HttpClient _httpClient;
        private readonly FedexRestSettings _settings;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public FedexRestStrategy(HttpClient httpClient, IOptions<CourierSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value.Fedex.Rest;
        }

        public async Task<ShipmentStatusResponse> GetShipmentStatus(string trackingNumber, bool isDropshipping)
        {
            var response = await SendTrackRequest(trackingNumber, forceTokenRefresh: false);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Dispose();
                response = await SendTrackRequest(trackingNumber, forceTokenRefresh: true);
            }

            using (response)
            {
                // 404 = numer nieznany w FedEx (TRACKING.TRACKINGNUMBER.NOTFOUND)
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return NotFound(trackingNumber);
                }

                response.EnsureSuccessStatusCode();

                var trackResponse = await response.Content.ReadFromJsonAsync<FedexTrackResponse>(_jsonOptions);
                var trackResult = trackResponse?.Output?.CompleteTrackResults?.FirstOrDefault()?.TrackResults?.FirstOrDefault();
                var status = trackResult?.LatestStatusDetail?.DerivedCode ?? trackResult?.LatestStatusDetail?.Code;

                if (trackResult == null || trackResult.Error != null || string.IsNullOrEmpty(status))
                {
                    return NotFound(trackingNumber);
                }

                return new ShipmentStatusResponse
                {
                    TrackingNumber = trackingNumber,
                    Date = GetStatusDate(trackResult) ?? DateTime.Now,
                    Status = status,
                    Description = trackResult.LatestStatusDetail?.Description
                };
            }
        }

        private async Task<HttpResponseMessage> SendTrackRequest(string trackingNumber, bool forceTokenRefresh)
        {
            var token = await GetToken(forceTokenRefresh);

            var request = new HttpRequestMessage(HttpMethod.Post, "track/v1/trackingnumbers")
            {
                Content = JsonContent.Create(new FedexTrackRequest
                {
                    IncludeDetailedScans = true,
                    TrackingInfo =
                    [
                        new FedexTrackingInfo
                        {
                            TrackingNumberInfo = new FedexTrackingNumberInfo { TrackingNumber = trackingNumber }
                        }
                    ]
                }, options: _jsonOptions)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _httpClient.SendAsync(request);
        }

        private async Task<string> GetToken(bool forceRefresh)
        {
            await _tokenLock.WaitAsync();
            try
            {
                if (!forceRefresh && _token != null && _token.ExpiresAt > DateTime.UtcNow)
                {
                    return _token.Token;
                }

                using var response = await _httpClient.PostAsync("oauth/token", new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = _settings.ClientId,
                    ["client_secret"] = _settings.ClientSecret
                }));

                response.EnsureSuccessStatusCode();

                var oauth = await response.Content.ReadFromJsonAsync<FedexOAuthResponse>(_jsonOptions)
                    ?? throw new InvalidOperationException("Empty FedEx OAuth response.");

                _token = new TokenDto
                {
                    Token = oauth.AccessToken,
                    // margines, aby nie użyć tokenu wygasającego w trakcie zapytania
                    ExpiresAt = DateTime.UtcNow.AddSeconds(oauth.ExpiresIn - 120)
                };

                return _token.Token;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        private static DateTime? GetStatusDate(FedexTrackResult trackResult)
        {
            // Najnowszy skan (zdarzenie odpowiadające ostatniemu statusowi)
            var lastScan = trackResult.ScanEvents?
                .Select(e => ParseDate(e.Date))
                .Where(d => d.HasValue)
                .Max();

            if (lastScan.HasValue)
            {
                return lastScan;
            }

            var date = trackResult.DateAndTimes?.FirstOrDefault(d => d.Type == "ACTUAL_DELIVERY")
                ?? trackResult.DateAndTimes?.FirstOrDefault(d => d.Type == "ACTUAL_PICKUP")
                ?? trackResult.DateAndTimes?.FirstOrDefault(d => d.Type == "SHIP");

            return ParseDate(date?.DateTime);
        }

        private static DateTime? ParseDate(string? value)
        {
            return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var date)
                ? date.LocalDateTime
                : null;
        }

        private static ShipmentStatusResponse NotFound(string trackingNumber) => new ShipmentStatusResponse
        {
            TrackingNumber = trackingNumber,
            Date = DateTime.Now,
            Status = "Not found"
        };
    }
}
