using System.Collections.Generic;
using Newtonsoft.Json;

namespace C2DMNet.Contracts.DataContracts
{
    public class GCMResultJson
    {
        [JsonProperty(PropertyName = "multicast_id")]
        public long MulticastId { get; set; }

        [JsonProperty(PropertyName = "success")]
        public long Success { get; set; }

        [JsonProperty(PropertyName = "failure")]
        public long Failure { get; set; }

        [JsonProperty(PropertyName = "canonical_ids")]
        public long CanonicalIds { get; set; }

        [JsonProperty(PropertyName = "results")]
        public IEnumerable<GCMResultInnerJson> Results { get; set; }
    }

    public class GCMResultInnerJson
    {
        [JsonProperty(PropertyName = "message_id")]
        public string MessageId { get; set; }

        [JsonProperty(PropertyName = "registration_id")]
        public string RegistrationId { get; set; }

        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }
    }
}