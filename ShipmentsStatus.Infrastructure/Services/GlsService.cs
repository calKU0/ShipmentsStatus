using ShipmentsStatus.Contracts.DTOs;
using ShipmentsStatus.Contracts.DTOs.GLS_Api;
using ShipmentsStatus.Contracts.Services;
using System.Net.Http.Json;
using System.Text.Json;

namespace ShipmentsStatus.Infrastructure.Services
{
    public class GlsService : ICourierService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public GlsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<ShipmentStatusResponse> GetShipmentStatus(ShipmentStatusRequest request)
        {
            var response = await _httpClient.GetAsync($"public/v1/tracking/references/{request.TrackingNumber}");

            response.EnsureSuccessStatusCode();

            var trackingResponse = await response.Content.ReadFromJsonAsync<GlsTrackingResponse>(_jsonOptions);
            var parcel = trackingResponse?.Parcels?.FirstOrDefault();

            return new ShipmentStatusResponse
            {
                TrackingNumber = request.TrackingNumber,
                Date = parcel?.Timestamp ?? DateTime.Now,
                Status = parcel?.Status ?? "Not found",
            };
        }
    }
}
