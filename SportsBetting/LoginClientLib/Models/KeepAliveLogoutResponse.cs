using System.Runtime.Serialization;

namespace LoginClientLib.Models
{
    [DataContract]
    internal class KeepAliveLogoutResponse
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