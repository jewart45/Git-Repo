using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BetHistoryImport.Classes
{
    public partial class ImportEvent
    {
        [JsonProperty("op")]
        public string Op { get; set; }

        [JsonProperty("clk")]
        public long Clk { get; set; }

        [JsonProperty("pt")]
        public long Pt { get; set; }

        [JsonProperty("mc")]
        public Mc1[] Mc { get; set; }
    }

    public partial class Mc1
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("marketDefinition")]
        public MarketDefinition MarketDefinition { get; set; }
    }

    public partial class MarketDefinition
    {
        [JsonProperty("bspMarket")]
        public bool BspMarket { get; set; }

        [JsonProperty("turnInPlayEnabled")]
        public bool TurnInPlayEnabled { get; set; }

        [JsonProperty("persistenceEnabled")]
        public bool PersistenceEnabled { get; set; }

        [JsonProperty("marketBaseRate")]
        public long MarketBaseRate { get; set; }

        [JsonProperty("eventId")]
        public long EventId { get; set; }

        [JsonProperty("eventTypeId")]
        public long EventTypeId { get; set; }

        [JsonProperty("numberOfWinners")]
        public long NumberOfWinners { get; set; }

        [JsonProperty("bettingType")]
        public string BettingType { get; set; }

        [JsonProperty("marketType")]
        public string MarketType { get; set; }

        [JsonProperty("marketTime")]
        public DateTimeOffset MarketTime { get; set; }

        [JsonProperty("suspendTime")]
        public DateTimeOffset SuspendTime { get; set; }

        [JsonProperty("bspReconciled")]
        public bool BspReconciled { get; set; }

        [JsonProperty("complete")]
        public bool Complete { get; set; }

        [JsonProperty("inPlay")]
        public bool InPlay { get; set; }

        [JsonProperty("crossMatching")]
        public bool CrossMatching { get; set; }

        [JsonProperty("runnersVoidable")]
        public bool RunnersVoidable { get; set; }

        [JsonProperty("numberOfActiveRunners")]
        public long NumberOfActiveRunners { get; set; }

        [JsonProperty("betDelay")]
        public long BetDelay { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("runners")]
        public Runner[] Runners { get; set; }

        [JsonProperty("regulators")]
        public string[] Regulators { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("discountAllowed")]
        public bool DiscountAllowed { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("openDate")]
        public DateTimeOffset OpenDate { get; set; }

        [JsonProperty("version")]
        public long Version { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("eventName")]
        public string EventName { get; set; }
    }

    public partial class Runner
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("sortPriority")]
        public long SortPriority { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class ImportPoint
    {
        [JsonProperty("op")]
        public string Op { get; set; }

        [JsonProperty("clk")]
        public long Clk { get; set; }

        [JsonProperty("pt")]
        public long Pt { get; set; }

        [JsonProperty("mc")]
        public Mc[] Mc { get; set; }
    }

    public partial class Mc
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("rc")]
        public Rc[] Rc { get; set; }
    }

    public partial class Rc
    {
        [JsonProperty("ltp")]
        public double Ltp { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }
    }
}
