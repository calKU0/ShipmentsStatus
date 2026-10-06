using ShipmentsStatus.Contracts.DTOs;

namespace ShipmentsStatus.Infrastructure.Services.Strategies
{
    public interface IFedexApiStrategy
    {
        Task<ShipmentStatusResponse> GetShipmentStatus(string trackingNumber, bool isDropshipping);
    }
}