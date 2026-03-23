namespace ShipmentsStatus.Contracts.Settings
{
    public class AppSettings
    {
        public int LogsExpirationDays { get; set; }
        public int WorkingIntervalMinutes { get; set; }
        public int ShipmentsBackDays { get; set; }
        public int ApiConcurrencyLimit { get; set; }
        public int DbConcurrencyLimit { get; set; }
        public int TooManyRequestsRetryCount { get; set; }
        public int TooManyRequestsBaseDelaySeconds { get; set; }
    }
}
