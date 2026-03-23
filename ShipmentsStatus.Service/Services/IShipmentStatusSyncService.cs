namespace ShipmentsStatus.Service.Services
{
    public interface IShipmentStatusSyncService
    {
        Task SyncRecentShipmentsAsync(CancellationToken cancellationToken);
    }
}
