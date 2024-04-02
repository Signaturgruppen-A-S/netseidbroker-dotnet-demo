using System.Text.Json.Serialization;

namespace NetsBrokerIntegration.DotNet.Models
{
    public class TokenEndpointResponse
    {
        [JsonPropertyName("access_token")]
        public string Access_token { get; set; }

        [JsonPropertyName("token_type")]
        public string Token_type { get; set; }
    }
}