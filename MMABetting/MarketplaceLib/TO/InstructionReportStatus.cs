using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Marketplace.TO
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum InstructionReportStatus
    {
        SUCCESS,
        FAILURE,
        TIMEOUT,
    }
}