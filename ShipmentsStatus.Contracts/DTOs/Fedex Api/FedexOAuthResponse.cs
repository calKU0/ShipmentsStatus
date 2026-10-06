using System.Text.Json.Serialization;

namespace ShipmentsStatus.Contracts.DTOs.Fedex_Api
{
    public class FedexOAuthResponse
    {
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
