using ShipmentsStatus.Contracts.Data.Enums;
using ShipmentsStatus.Contracts.Models;
using ShipmentsStatus.Contracts.Repositories;
using ShipmentsStatus.Infrastructure.Data;
using ShipmentsStatus.Infrastructure.Helpers;
using System.Data;

namespace ShipmentsStatus.Infrastructure.Repositories
{
    public class ShipmentRepository : IShipmentRepository
    {
        private readonly IDbExecutor _dbExecutor;
        public ShipmentRepository(IDbExecutor dbExecutor)
        {
            _dbExecutor = dbExecutor;
        }
        public async Task<IEnumerable<Shipment>> GetShipmentsFromDate(DateTime date, CancellationToken cancellationToken)
        {
            const string procedureName = "kp.GetShipmentsFromDate";

            var dbShipments = await _dbExecutor.QueryAsync<ShipmentDbModel>(
                procedureName,
                new { DateFrom = date },
                CommandType.StoredProcedure);

            return dbShipments.Select(x => new Shipment
            {
                Id = x.Id,
                TrackingNumber = x.TrackingNumber,
                Type = x.Type,
                Courier = MapCourier(x.Courier),
                IsDropshipping = x.IsDropshipping,
                Status = x.Status,
                Country = x.Country,
                StatusDate = x.StatusDate,
                CreatedAt = x.CreatedAt
            });
        }

        public async Task UpdateShipmentStatus(Shipment shipment, CancellationToken cancellationToken)
        {
            const string procedureName = "kp.UpdateShipmentStatus";
            await _dbExecutor.QueryAsync<Shipment>(procedureName, new
            {
                ShipmentId = shipment.Id,
                ShipmentType = shipment.Type,
                Status = shipment.Status,
                StatusDate = shipment.StatusDate
            }, CommandType.StoredProcedure);
        }

        private static Courier MapCourier(string courier)
        {
            if (string.IsNullOrWhiteSpace(courier))
            {
                throw new ArgumentException("Courier value from database is empty.", nameof(courier));
            }

            if (Enum.TryParse<Courier>(courier, true, out var byName))
            {
                return byName;
            }

            foreach (var value in Enum.GetValues<Courier>())
            {
                if (value.GetDescription().Equals(courier, StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }
            }

            return Courier.Unknown;
        }

        private class ShipmentDbModel
        {
            public int Id { get; set; }
            public string TrackingNumber { get; set; } = string.Empty;
            public int Type { get; set; }
            public string Courier { get; set; } = string.Empty;
            public ShipmentStatus Status { get; set; }
            public bool IsDropshipping { get; set; }
            public string Country { get; set; } = string.Empty;
            public DateTime StatusDate { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
