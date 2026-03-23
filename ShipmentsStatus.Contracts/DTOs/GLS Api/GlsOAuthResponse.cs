using System.Text.Json.Serialization;

namespace ShipmentsStatus.Contracts.DTOs.GLS_Api
{
    public class GlsOAuthResponse
    {
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
