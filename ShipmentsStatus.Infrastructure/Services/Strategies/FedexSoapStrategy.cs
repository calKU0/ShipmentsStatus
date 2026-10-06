using Microsoft.Extensions.Options;
using ShipmentsStatus.Contracts.DTOs;
using ShipmentsStatus.Contracts.Settings;
using ShipmentsStatus.Infrastructure.FedexReference;

namespace ShipmentsStatus.Infrastructure.Services.Strategies
{
    public class FedexSoapStrategy : IFedexApiStrategy
    {
        private readonly FedexSoapSettings _settings;
        private readonly IklServiceClient _client;
        public FedexSoapStrategy(IOptions<CourierSettings> options)
        {
            _settings = options.Value.Fedex.Soap;
            _client = new IklServiceClient(IklServiceClient.EndpointConfiguration.IklServicePort);
        }

        public async Task<ShipmentStatusResponse> GetShipmentStatus(string trackingNumber, bool isDropshipping)
        {
            var result = await _client.pobierzStatusyPrzesylkiAsync(isDropshipping ? _settings.DropshippingAccessCode : _settings.AccessCode, trackingNumber, 1);
            var status = result.statusyPrzesylki?.FirstOrDefault();

            return new ShipmentStatusResponse
            {
                TrackingNumber = trackingNumber,
                Date = Convert.ToDateTime(status?.dataS ?? DateTime.Now.ToString()),
                Status = status?.skrot ?? "Not found",
                Description = status?.opis
            };
        }
    }
}
