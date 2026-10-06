namespace ShipmentsStatus.Contracts.DTOs
{
    public class ShipmentStatusRequest
    {
        public string TrackingNumber { get; set; }
        public bool IsDropshipping { get; set; }
        public string Country { get; set; }
    }
}
