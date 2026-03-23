using ShipmentsStatus.Contracts.Models;

namespace ShipmentsStatus.Contracts.Repositories
{
    public interface IShipmentRepository
    {
        public Task<IEnumerable<Shipment>> GetShipmentsFromDate(DateTime date, CancellationToken cancellationToken);
        public Task UpdateShipmentStatus(Shipment shipment, CancellationToken cancellationToken);
    }
}
