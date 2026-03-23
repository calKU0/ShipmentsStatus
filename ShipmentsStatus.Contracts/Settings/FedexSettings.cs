namespace ShipmentsStatus.Contracts.Settings
{
    public class FedexSettings
    {
        public FedexRestSettings Rest { get; set; } = new();
        public FedexSoapSettings Soap { get; set; } = new();
    }

    public class FedexRestSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Account { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }

    public class FedexSoapSettings
    {
        public int CourierId { get; set; }
        public string AccessCode { get; set; } = string.Empty;
        public string DropshippingAccessCode { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
    }
}