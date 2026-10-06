namespace ShipmentsStatus.Contracts.DTOs.Fedex_Api
{
    public class FedexTrackResponse
    {
        public string TransactionId { get; set; }
        public FedexTrackOutput Output { get; set; }
    }

    public class FedexTrackOutput
    {
        public List<FedexCompleteTrackResult> CompleteTrackResults { get; set; }
    }

    public class FedexCompleteTrackResult
    {
        public string TrackingNumber { get; set; }
        public List<FedexTrackResult> TrackResults { get; set; }
    }

    public class FedexTrackResult
    {
        public FedexStatusDetail LatestStatusDetail { get; set; }
        public List<FedexScanEvent> ScanEvents { get; set; }
        public List<FedexDateAndTime> DateAndTimes { get; set; }
        public FedexError Error { get; set; }
    }

    public class FedexStatusDetail
    {
        public string Code { get; set; }
        public string DerivedCode { get; set; }
        public string StatusByLocale { get; set; }
        public string Description { get; set; }
    }

    public class FedexScanEvent
    {
        public string Date { get; set; }
        public string EventType { get; set; }
        public string EventDescription { get; set; }
        public string DerivedStatusCode { get; set; }
    }

    public class FedexDateAndTime
    {
        public string Type { get; set; }
        public string DateTime { get; set; }
    }

    public class FedexError
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }
}
