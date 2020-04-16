using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Marketplace.TO
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RollUpModel
    {
        STAKE, PAYOUT, MANAGED_LIABILITY, NONE
    }
}