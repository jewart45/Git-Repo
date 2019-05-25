using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace LoginClientLib.Models
{
    [DataContract]
    class KeepAliveLogoutResponse
    {
        [DataMember(Name = "token")]
        public string Token { get; set; }
        [DataMember(Name = "product")]
        public string Product { get; set; }
        [DataMember(Name = "status")]
        public string Status { get; set; }
        [DataMember(Name = "error")]
        public string Error { get; set; }
    }
}
