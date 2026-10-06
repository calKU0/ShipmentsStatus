namespace ShipmentsStatus.Contracts.DTOs
{
    public class ShipmentStatusResponse
    {
        public string TrackingNumber { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string? Description { get; set; }
    }
}
