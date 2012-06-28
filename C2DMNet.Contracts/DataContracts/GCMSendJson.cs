using System.Collections.Generic;
using Newtonsoft.Json;

namespace C2DMNet.Contracts.DataContracts
{
    public class GCMSendJson
    {
        [JsonProperty(PropertyName = "registration_ids")]
        public IEnumerable<string> RegistrationIds { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "collapse_key")]
        public string CollapseKey { get; set; }

        [JsonProperty(PropertyName = "data")]
        public IDictionary<string,string> Data { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "delay_while_idle")]
        public bool? DelayWhileIdle { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "time_to_live")]
        public int? TimeToLive { get; set; }
    }
}