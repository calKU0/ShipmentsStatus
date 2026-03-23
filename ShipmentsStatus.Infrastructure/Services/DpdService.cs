using Microsoft.Extensions.Options;
using ShipmentsStatus.Contracts.DTOs;
using ShipmentsStatus.Contracts.Services;
using ShipmentsStatus.Contracts.Settings;
using ShipmentsStatus.Infrastructure.DpdReference;

namespace ShipmentsStatus.Infrastructure.Services
{
    public class DpdService : ICourierService
    {
        private readonly DpdSettings _settings;
        private readonly DPDInfoServicesObjEventsClient _infoServices = new DPDInfoServicesObjEventsClient();
        public DpdService(IOptions<DpdSettings> settings)
        {
            _settings = settings.Value;
        }
        public async Task<ShipmentStatusResponse> GetShipmentStatus(ShipmentStatusRequest request)
        {
            var response = await _infoServices.getEventsForWaybillV1Async(request.TrackingNumber, eventsSelectTypeEnum.ALL, "EN", GetAuthData());

            customerEventV3? parcel = response.@return.eventsList.Where(e => e.description != "Mail notification").OrderByDescending(e => e.eventTime).FirstOrDefault();

            return new ShipmentStatusResponse
            {
                TrackingNumber = request.TrackingNumber,
                Date = parcel?.eventTime != null ? DateTime.Parse(parcel.eventTime) : DateTime.Now,
                Status = parcel?.description ?? "Not found"
            };
        }

        private authDataV1 GetAuthData()
        {
            return new authDataV1
            {
                login = _settings.Username,
                password = _settings.Password,
                channel = _settings.Channel
            };
        }
    }
}
