using ShipmentsStatus.Contracts.Data.Enums;

namespace ShipmentsStatus.Contracts.Models
{
    public class Shipment
    {
        public int Id { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public int Type { get; set; }
        public Courier Courier { get; set; }
        public ShipmentStatus Status { get; set; }
        public string Country { get; set; }
        public DateTime StatusDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
