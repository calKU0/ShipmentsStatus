using System.ComponentModel;

namespace ShipmentsStatus.Contracts.Data.Enums
{
    public enum Courier
    {
        [Description("DPD")]
        DPD,

        [Description("GLS")]
        GLS,

        [Description("Fedex")]
        Fedex,

        [Description("DPD-Romania")]
        DPD_Romania,

        [Description("Nieznany")]
        Unknown
    }
}
