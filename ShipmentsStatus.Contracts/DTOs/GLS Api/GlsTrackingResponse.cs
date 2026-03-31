namespace ShipmentsStatus.Contracts.DTOs.GLS_Api
{
    public class GlsTrackingResponse
    {
        public List<GlsParcel> Parcels { get; set; }
    }
    public class GlsParcel
    {
        public DateTime Timestamp { get; set; }
        public string Trackid { get; set; }
        public string Status { get; set; }
    }
}
