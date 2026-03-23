using ShipmentsStatus.Contracts.DTOs;

namespace ShipmentsStatus.Contracts.Services
{
    public interface ICourierService
    {
        Task<ShipmentStatusResponse> GetShipmentStatus(ShipmentStatusRequest request);
    }
}