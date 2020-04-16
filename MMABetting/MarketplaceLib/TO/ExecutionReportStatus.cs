using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Marketplace.TO
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ExecutionReportStatus
    {
        SUCCESS,
        FAILURE,
        PROCESSED_WITH_ERRORS,
        TIMEOUT
    }
}