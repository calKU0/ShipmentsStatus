using ShipmentsStatus.Contracts.Data.Enums;

namespace ShipmentsStatus.Infrastructure.Mapping
{
    public static class ShipmentStatusMapper
    {
        public static ShipmentStatus MapToContractEnum(string status)
        {
            if (string.IsNullOrEmpty(status) || status == "Not found") return ShipmentStatus.NotFound;

            var s = status.Trim().ToUpper();

            // =========================
            // WAITS FOR COURIER
            // =========================
            if (s.Contains("REGISTERED PARCEL DATA") ||
                s.Contains("COLLECTION REQUEST") ||
                s.Contains("NOT DISPATCHED") ||
                s.Contains("SHIPMENT NOT PREPARED") ||
                s.Contains("SENDER ABSENT") ||
                s.Contains("NO SHIPMENT AT THE SENDER") ||
                s.Contains("ORDER NOT COMPLETED") ||
                s.Contains("ORDER NOT EXECUTED") ||
                s.Contains("PLANNEDPICKUP") ||
                s.Contains("INPICKUP") ||
                s.Contains("NOTPICKEDUP"))
                return ShipmentStatus.WaitsForCourier;

            // =========================
            // SHIPPED (label created / handed over)
            // =========================
            if (s.Contains("COLLECTED BY COURIER") ||
                s.Contains("COLLECTED BY PICKUP POINT") ||
                s.Contains("DROPPED-OFF") ||
                s.Contains("HANDED OVER TO COURIER") ||
                s.Contains("SENT ABROAD") ||
                s.Contains("PREADVICE"))
                return ShipmentStatus.Shipped;

            // =========================
            // IN TRANSIT
            // =========================
            if (s.Contains("RECEIVED BY DPD DEPOT") ||
                s.Contains("IN TRANSIT") ||
                s.Contains("INTRANSIT") ||
                s.Contains("INDELIVERY") ||
                s.Contains("INWAREHOUSE") ||
                s.Contains("DISPATCHED TO BE DELIVERED") ||
                s.Contains("SORTING") ||
                s.Contains("REPACKED") ||
                s.Contains("STORED IN DEPOT") ||
                s.Contains("REDIRECTION") ||
                s.Contains("TRANSHIPMENT") ||
                s.Contains("CUSTOMS") ||
                s.Contains("HELD IN CUSTOMS") ||
                s.Contains("EXPORT / IMPORT CLEARED") ||
                s.Contains("ABROAD DELIVERY DEPOT") ||
                s == "WE" ||
                s == "WK")
                return ShipmentStatus.InTransit;

            // =========================
            // DELIVERED
            // =========================
            if (s.Contains("PARCEL DELIVERED") ||
                s.Contains("DELIVERED TO RECEIVER") ||
                s.Contains("DELIVERED IN PUDO") ||
                s.Contains("DELIVERED - COD COLLECTED") ||
                s.Contains("DELIVEREDPS") ||
                s.Contains("DELIVERED") ||
                s.Contains("FINAL") ||
                s.Contains("DELIVERED INDIRECTLY") ||
                s == "DS")
                return ShipmentStatus.Delivered;

            // =========================
            // CALL (problem / failed delivery / action needed)
            // =========================
            if (s.Contains("NOT DELIVERED") ||
                s.Contains("UNDELIVERED") ||
                s.Contains("FAILED") ||
                s.Contains("REFUSAL") ||
                s.Contains("WRONG ADDRESS") ||
                s.Contains("NO COD") ||
                s.Contains("RECIPIENT NOT AVAILABLE") ||
                s.Contains("RECIPIENT RESIGNED") ||
                s.Contains("MISSING") ||
                s.Contains("DAMAGED") ||
                s.Contains("RETURN TO SENDER") ||
                s.Contains("RETURNED") ||
                s.Contains("LOST") ||
                s.Contains("DISPOSAL") ||
                s.Contains("CANCELLED") ||
                s.Contains("CANCELLATION"))
                return ShipmentStatus.Call;

            return ShipmentStatus.Other;
        }
    }
}
