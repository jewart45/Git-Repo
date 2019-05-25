using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

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
