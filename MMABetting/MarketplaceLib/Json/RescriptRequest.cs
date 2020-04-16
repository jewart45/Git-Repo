using Newtonsoft.Json;
using System.Collections.Generic;

namespace Marketplace.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RescriptRequest
    {
        [JsonProperty(PropertyName = "")]
        public IDictionary<string, object> args { get; set; }

        public RescriptRequest(IDictionary<string, object> args)
        {
            args = args;
        }
    }
}