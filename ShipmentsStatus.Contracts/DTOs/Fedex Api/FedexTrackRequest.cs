namespace ShipmentsStatus.Contracts.DTOs.Fedex_Api
{
    public class FedexTrackRequest
    {
        public bool IncludeDetailedScans { get; set; }
        public List<FedexTrackingInfo> TrackingInfo { get; set; }
    }

    public class FedexTrackingInfo
    {
        public FedexTrackingNumberInfo TrackingNumberInfo { get; set; }
    }

    public class FedexTrackingNumberInfo
    {
        public string TrackingNumber { get; set; }
    }
}
