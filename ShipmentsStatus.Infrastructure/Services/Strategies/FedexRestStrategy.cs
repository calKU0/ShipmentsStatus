using ShipmentsStatus.Contracts.DTOs;

namespace ShipmentsStatus.Infrastructure.Services.Strategies
{
    public class FedexRestStrategy : IFedexApiStrategy
    {
        private readonly HttpClient _httpClient;

        public FedexRestStrategy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<ShipmentStatusResponse> GetShipmentStatus(string trackingNumber)
        {
            throw new NotImplementedException();
        }
    }
}
