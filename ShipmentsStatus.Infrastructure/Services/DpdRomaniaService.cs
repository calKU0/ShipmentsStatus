using Microsoft.Extensions.Options;
using ShipmentsStatus.Contracts.DTOs;
using ShipmentsStatus.Contracts.Services;
using ShipmentsStatus.Contracts.Settings;

namespace ShipmentsStatus.Infrastructure.Services
{
    public class DpdRomaniaService : ICourierService
    {
        private readonly HttpClient _httpClient;
        private readonly DpdSettings _settings;
        public DpdRomaniaService(HttpClient httpClient, IOptions<DpdSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public Task<ShipmentStatusResponse> GetShipmentStatus(ShipmentStatusRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
