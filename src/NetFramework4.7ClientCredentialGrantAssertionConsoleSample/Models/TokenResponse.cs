using System.Runtime.Serialization;

namespace NetFrameworkClientCredentialGrantAssertionConsoleSample.Models
{
    [DataContract]
    public class TokenResponse
    {
        [DataMember]
        public string access_token { get; set; }

        [DataMember]
        public string token_type { get; set; }

        [DataMember]
        public int? expires_in { get; set; }

        [DataMember]
        public string error { get; set; }

        [DataMember]
        public string error_description { get; set; }

    }
}
