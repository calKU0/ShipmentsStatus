using ShipmentsStatus.Contracts.DTOs;
using ShipmentsStatus.Contracts.Services;
using ShipmentsStatus.Infrastructure.Services.Strategies;

namespace ShipmentsStatus.Infrastructure.Services
{
    public class FedexService : ICourierService
    {
        private readonly IFedexApiStrategy _soapStrategy;
        private readonly IFedexApiStrategy _restStrategy;

        public FedexService(FedexSoapStrategy soapStrategy, FedexRestStrategy restStrategy)
        {
            _soapStrategy = soapStrategy;
            _restStrategy = restStrategy;
        }

        private IFedexApiStrategy GetStrategy(string country)
        {
            return country == "PL" ? _soapStrategy : _restStrategy;
        }

        public Task<ShipmentStatusResponse> GetShipmentStatus(ShipmentStatusRequest request)
        {
            return GetStrategy(request.Country).GetShipmentStatus(request.TrackingNumber);
        }
    }
}
