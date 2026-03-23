namespace ShipmentsStatus.Contracts.DTOs.DPD_Romania_Api
{
    public class DpdRomaniaTrackParcelRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Language { get; set; }
        public long ClientSystemId { get; set; }
        public List<DpdRomaniaShipmentParcelReference> Parcels { get; set; }
        public bool LastOperationOnly { get; set; }
    }
    public class DpdRomaniaShipmentParcelReference
    {
        public string Id { get; set; }
        public string Ref { get; set; }
    }
}
