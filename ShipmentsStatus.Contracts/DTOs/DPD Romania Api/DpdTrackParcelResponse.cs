using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ShipmentsStatus.Contracts.DTOs.DPD_Romania_Api
{
    public class DpdRomaniaTrackParcelResponse
    {
        public List<DpdRomaniaParcel> Parcels { get; set; }
        public DpdRomaniaError Error { get; set; }
    }

    public class DpdRomaniaParcel
    {
        public string ParcelId { get; set; } = null!;

        public List<string>? ExternalCarrierParcelNumbers { get; set; }

        public List<DpdRomaniaTrackedParcelOperation> Operations { get; set; } = new();

        public Dictionary<string, DpdRomaniaExternalCarrierParcelNumberDetails>? ExternalCarrierParcelNumbersDetails { get; set; }

        public Error? Error { get; set; }
    }

    public class DpdRomaniaTrackedParcelOperation
    {
        public DateTime DateTime { get; set; }

        public int OperationCode { get; set; }

        public string? Place { get; set; }

        public string Description { get; set; } = null!;

        public string? Comment { get; set; }

        public List<string>? ExceptionCodes { get; set; }

        public string? ReturnShipmentId { get; set; }

        public string? RedirectShipmentId { get; set; }

        public string? Consignee { get; set; }

        public string? PodImageURL { get; set; }

        public DpdRomaniaTrackedParcelOperationAdditionalInfo? AdditionalInfo { get; set; }
    }

    public class DpdRomaniaTrackedParcelOperationAdditionalInfo
    {
        public string? OfficeURL { get; set; }

        public string? GeoPUDOId { get; set; }

        public DpdRomaniaTrackedParcelOperationAdditionalInfoPredict? Predict { get; set; }
    }

    public class DpdRomaniaTrackedParcelOperationAdditionalInfoPredict
    {
        public DateTime PredictedVisitDateTimeFrom { get; set; }

        public DateTime PredictedVisitDateTimeTo { get; set; }

        public int? IncludedDelayInMinutes { get; set; }

        public bool Canceled { get; set; }
    }

    // Placeholder classes (not defined in your spec but referenced)
    public class DpdRomaniaExternalCarrierParcelNumberDetails
    {
        // Add properties as needed
    }

    public class DpdRomaniaError
    {
    }
}
