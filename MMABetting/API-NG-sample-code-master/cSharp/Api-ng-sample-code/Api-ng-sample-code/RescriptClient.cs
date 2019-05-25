using Api_ng_sample_code.Json;
using Api_ng_sample_code.TO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Services.Protocols;

namespace Api_ng_sample_code
{
    public class RescriptClient : HttpWebClientProtocol, IClient
    {
        public string EndPoint { get; private set; }
        private static readonly IDictionary<string, Type> operationReturnTypeMap = new Dictionary<string, Type>();
        public const string APPKEY_HEADER = "X-Application";
        public const string SESSION_TOKEN_HEADER = "X-Authentication";
        public NameValueCollection CustomHeaders { get; set; }
        private static readonly string LIST_EVENT_TYPES_METHOD = "listEventTypes";
        private static readonly string LIST_MARKET_CATALOGUE_METHOD = "listMarketCatalogue";
        private static readonly string LIST_MARKET_TYPES_METHOD = "listMarketTypes";
        private static readonly string LIST_MARKET_BOOK_METHOD = "listMarketBook";
        private static readonly string PLACE_ORDERS_METHOD = "placeOrders";
        private static readonly string LIST_MARKET_PROFIT_AND_LOST_METHOD = "listMarketProfitAndLoss";
        private static readonly string LIST_CURRENT_ORDERS_METHOD = "listCurrentOrders";
        private static readonly string LIST_CLEARED_ORDERS_METHOD = "listClearedOrders";
        private static readonly string CANCEL_ORDERS_METHOD = "cancelOrders";
        private static readonly string REPLACE_ORDERS_METHOD = "replaceOrders";
        private static readonly string UPDATE_ORDERS_METHOD = "updateOrders";
        private static readonly string FILTER = "filter";
        private static readonly string LOCALE = "locale";
        private static readonly string CURRENCY_CODE = "currencyCode";
        private static readonly string MARKET_PROJECTION = "marketProjection";
        private static readonly string MATCH_PROJECTION = "matchProjection";
        private static readonly string ORDER_PROJECTION = "orderProjection";
        private static readonly string PRICE_PROJECTION = "priceProjection";
        private static readonly string SORT = "sort";
        private static readonly string MAX_RESULTS = "maxResults";
        private static readonly string MARKET_IDS = "marketIds";
        private static readonly string MARKET_ID = "marketId";
        private static readonly string INSTRUCTIONS = "instructions";
        private static readonly string CUSTOMER_REFERENCE = "customerRef";
        private static readonly string INCLUDE_SETTLED_BETS = "includeSettledBets";
        private static readonly string INCLUDE_BSP_BETS = "includeBspBets";
        private static readonly string NET_OF_COMMISSION = "netOfCommission";
        private static readonly string BET_IDS = "betIds";
        private static readonly string PLACED_DATE_RANGE = "placedDateRange";
        private static readonly string ORDER_BY = "orderBy";
        private static readonly string SORT_DIR = "sortDir";
        private static readonly string FROM_RECORD = "fromRecord";
        private static readonly string RECORD_COUNT = "recordCount";
        private static readonly string BET_STATUS = "betStatus";
        private static readonly string EVENT_TYPE_IDS = "eventTypeIds";
        private static readonly string EVENT_IDS = "eventIds";
        private static readonly string RUNNER_IDS = "runnerIds";
        private static readonly string SIDE = "side";
        private static readonly string SETTLED_DATE_RANGE = "settledDateRange";
        private static readonly string GROUP_BY = "groupBy";
        private static readonly string INCLUDE_ITEM_DESCRIPTION = "includeItemDescription";

        public RescriptClient(string endPoint, string appKey, string sessionToken)
        {
            EndPoint = endPoint + "/rest/v1/";
            CustomHeaders = new NameValueCollection();
            if (appKey != null)
            {
                CustomHeaders[APPKEY_HEADER] = appKey;
            }
            if (sessionToken != null)
            {
                CustomHeaders[SESSION_TOKEN_HEADER] = sessionToken;
            }
        }

        public IList<EventTypeResult> listEventTypes(MarketFilter marketFilter, string locale = null)
        {
            var args = new Dictionary<string, object>
            {
                [FILTER] = marketFilter,
                [LOCALE] = locale
            };
            return Invoke<List<EventTypeResult>>(LIST_EVENT_TYPES_METHOD, args);
        }

        public IList<MarketCatalogue> listMarketCatalogue(MarketFilter marketFilter, ISet<MarketProjection> marketProjections, MarketSort marketSort, string maxResult = "1", string locale = null)
        {
            var args = new Dictionary<string, object>
            {
                [FILTER] = marketFilter,
                [MARKET_PROJECTION] = marketProjections,
                [SORT] = marketSort,
                [MAX_RESULTS] = maxResult,
                [LOCALE] = locale
            };
            return Invoke<List<MarketCatalogue>>(LIST_MARKET_CATALOGUE_METHOD, args);
        }

        public IList<MarketBook> listMarketBook(IList<string> marketIds, PriceProjection priceProjection, OrderProjection? orderProjection = null, MatchProjection? matchProjection = null, string currencyCode = null, string locale = null)
        {
            var args = new Dictionary<string, object>
            {
                [MARKET_IDS] = marketIds,
                [PRICE_PROJECTION] = priceProjection,
                [ORDER_PROJECTION] = orderProjection,
                [MATCH_PROJECTION] = matchProjection,
                [LOCALE] = locale,
                [CURRENCY_CODE] = currencyCode
            };
            return Invoke<List<MarketBook>>(LIST_MARKET_BOOK_METHOD, args);
        }

        public PlaceExecutionReport placeOrders(string marketId, string customerRef, IList<PlaceInstruction> placeInstructions, string locale = null)
        {
            var args = new Dictionary<string, object>
            {
                [MARKET_ID] = marketId,
                [INSTRUCTIONS] = placeInstructions,
                [CUSTOMER_REFERENCE] = customerRef,
                [LOCALE] = locale
            };

            return Invoke<PlaceExecutionReport>(PLACE_ORDERS_METHOD, args);
        }

        protected HttpWebRequest CreateWebRequest(string restEndPoint)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(restEndPoint);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = 0;
            request.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");
            request.Accept = "application/json";
            request.Headers.Add(CustomHeaders);
            ServicePointManager.Expect100Continue = false;
            return request;
        }

        public T Invoke<T>(string method, IDictionary<string, object> args = null)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            if (method.Length == 0)
            {
                throw new ArgumentException(null, "method");
            }

            var restEndpoint = EndPoint + method + "/";
            var request = CreateWebRequest(restEndpoint);

            var postData = JsonConvert.Serialize<IDictionary<string, object>>(args) + "}";

            Console.WriteLine("\nCalling: " + method + " With args: " + postData);

            var bytes = Encoding.GetEncoding("UTF-8").GetBytes(postData);
            request.ContentLength = bytes.Length;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)GetWebResponse(request))

            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var jsonResponse = reader.ReadToEnd();
                Console.WriteLine("\nGot response: " + jsonResponse);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw ReconstituteException(JsonConvert.Deserialize<Api_ng_sample_code.TO.Exception>(jsonResponse));
                }
                return JsonConvert.Deserialize<T>(jsonResponse);
            }
        }

        private static System.Exception ReconstituteException(Api_ng_sample_code.TO.Exception ex)
        {
            var data = ex.Detail;

            // API-NG exception -- it must have "data" element to tell us which exception
            var exceptionName = data.Property("exceptionname").Value.ToString();
            var exceptionData = data.Property(exceptionName).Value.ToString();
            return JsonConvert.Deserialize<APINGException>(exceptionData);
        }

        public IList<MarketProfitAndLoss> listMarketProfitAndLoss(IList<string> marketIds, bool includeSettledBets = false, bool includeBspBets = false, bool netOfCommission = false)
        {
            var args = new Dictionary<string, object>
            {
                [MARKET_IDS] = marketIds,
                [INCLUDE_SETTLED_BETS] = includeSettledBets,
                [INCLUDE_BSP_BETS] = includeBspBets,
                [NET_OF_COMMISSION] = netOfCommission
            };

            return Invoke<List<MarketProfitAndLoss>>(LIST_MARKET_PROFIT_AND_LOST_METHOD, args);
        }

        public CurrentOrderSummaryReport listCurrentOrders(ISet<string> betIds, ISet<string> marketIds, OrderProjection? orderProjection = null, TimeRange placedDateRange = null, OrderBy? orderBy = null, SortDir? sortDir = null, int? fromRecord = null, int? recordCount = null)
        {
            var args = new Dictionary<string, object>
            {
                [BET_IDS] = betIds,
                [MARKET_IDS] = marketIds,
                [ORDER_PROJECTION] = orderProjection,
                [PLACED_DATE_RANGE] = placedDateRange,
                [ORDER_BY] = orderBy,
                [SORT_DIR] = sortDir,
                [FROM_RECORD] = fromRecord,
                [RECORD_COUNT] = recordCount
            };

            return Invoke<CurrentOrderSummaryReport>(LIST_CURRENT_ORDERS_METHOD, args);
        }

        public ClearedOrderSummaryReport listClearedOrders(BetStatus betStatus, ISet<string> eventTypeIds = null, ISet<string> eventIds = null, ISet<string> marketIds = null, ISet<RunnerId> runnerIds = null, ISet<string> betIds = null, Side? side = null, TimeRange settledDateRange = null, GroupBy? groupBy = null, bool? includeItemDescription = null, string locale = null, int? fromRecord = null, int? recordCount = null)
        {
            var args = new Dictionary<string, object>
            {
                [BET_STATUS] = betStatus,
                [EVENT_TYPE_IDS] = eventTypeIds,
                [EVENT_IDS] = eventIds,
                [MARKET_IDS] = marketIds,
                [RUNNER_IDS] = runnerIds,
                [BET_IDS] = betIds,
                [SIDE] = side,
                [SETTLED_DATE_RANGE] = settledDateRange,
                [GROUP_BY] = groupBy,
                [INCLUDE_ITEM_DESCRIPTION] = includeItemDescription,
                [LOCALE] = locale,
                [FROM_RECORD] = fromRecord,
                [RECORD_COUNT] = recordCount
            };

            return Invoke<ClearedOrderSummaryReport>(LIST_CLEARED_ORDERS_METHOD, args);
        }

        public CancelExecutionReport cancelOrders(string marketId, IList<CancelInstruction> instructions, string customerRef)
        {
            var args = new Dictionary<string, object>
            {
                [MARKET_ID] = marketId,
                [INSTRUCTIONS] = instructions,
                [CUSTOMER_REFERENCE] = customerRef
            };

            return Invoke<CancelExecutionReport>(CANCEL_ORDERS_METHOD, args);
        }

        public ReplaceExecutionReport replaceOrders(string marketId, IList<ReplaceInstruction> instructions, string customerRef)
        {
            var args = new Dictionary<string, object>
            {
                [MARKET_ID] = marketId,
                [INSTRUCTIONS] = instructions,
                [CUSTOMER_REFERENCE] = customerRef
            };

            return Invoke<ReplaceExecutionReport>(REPLACE_ORDERS_METHOD, args);
        }

        public UpdateExecutionReport updateOrders(string marketId, IList<UpdateInstruction> instructions, string customerRef)
        {
            var args = new Dictionary<string, object>
            {
                [MARKET_ID] = marketId,
                [INSTRUCTIONS] = instructions,
                [CUSTOMER_REFERENCE] = customerRef
            };

            return Invoke<UpdateExecutionReport>(UPDATE_ORDERS_METHOD, args);
        }

        public IList<MarketTypeResult> listMarketTypes(MarketFilter marketFilter, string stringLocale)
        {
            var args = new Dictionary<string, object>
            {
                [FILTER] = marketFilter,
                [LOCALE] = stringLocale
            };
            return Invoke<List<MarketTypeResult>>(LIST_MARKET_TYPES_METHOD, args);
        }

        public AccountFundsResponse getAccountFunds(Wallet wallet) => throw new NotImplementedException();
    }
}