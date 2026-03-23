namespace ShipmentsStatus.Contracts.DTOs.GLS_Api
{
    public class GlsTrackingResponse
    {
        public List<GlsParcel> Parcels { get; set; }
    }
    public class GlsParcel
    {
        public string Requested { get; set; }
        public string Unitno { get; set; }
        public string Status { get; set; }
        public string StatusDateTime { get; set; }
    }
}
