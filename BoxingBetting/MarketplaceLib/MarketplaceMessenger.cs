using Marketplace.TO;
using System;
using System.Collections.Generic;

namespace Marketplace
{
    public class MarketplaceMessenger
    {
        private static string appKey = "r9pFKKDlrpQknNvB";
        private static string sessionTokenStatic = "5clUqvyg1RczWBw1MpvY7Ug0otjZRvAjcwHFlXYbqa8=";
        private static string Url = "https://api.betfair.com/exchange/betting";
        public List<Dictionary<string, string>> eventList;
        private MarketFilter marketFilter;
        private IList<MarketCatalogue> marketCatalogues;

        //Betfair_Non_interactive_login.Program loginProgram;
        private IClient client;

        public MarketplaceMessenger()
        {
        }

        private string GetSessionID()
        {
            string sessionID = null;
            return sessionID;
        }

        public bool Initialise(string sessionToken)
        {
            var appkey = appKey;

            if (string.IsNullOrEmpty(appkey))
            {
                Console.WriteLine("No App Key");
                Environment.Exit(0);
            }

            string sessionID = GetSessionID();
            Console.WriteLine("App Key being used: " + appkey);
            //second argument is the sessionToken
            if (string.IsNullOrEmpty(sessionToken))
            {
                Console.WriteLine("No Session Token");
                Environment.Exit(0);
            }
            Console.WriteLine("Session token being used: " + sessionToken);
            //the third argument is which type of client to use, default is json-rpc
            client = null;
            string clientType = null;

            if (!string.IsNullOrEmpty(clientType) && clientType.Equals("rescript"))
            {
                Console.WriteLine("Using RescriptClient");
                client = new RescriptClient(Url, appkey, sessionToken);
            }
            else
            {
                Console.WriteLine("Using JsonRpcClient");
                client = new JsonRpcClient(Url, appkey, sessionToken);
            }
            Console.WriteLine("\nBeginning sample run!\n");
            try
            {
                marketFilter = new MarketFilter();
                //marketFilter.MarketIds = new HashSet<string>();
                //marketFilter.MarketIds.Add("26420387");
                marketCatalogues = new List<MarketCatalogue>();

                var eventTypes = client.listEventTypes(marketFilter);
                return true;
            }
            catch (APINGException apiExcepion)
            {
                Console.WriteLine("Got an exception from Api-NG: " + apiExcepion.ErrorCode);
                return false;
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Unknown exception from application: " + e.Message);
                return false;
            }
        }

        public void SetMarketFilter(string searchString)
        {
            // Start Here

            var time = new TimeRange();
            time.From = DateTime.Now;
            time.To = DateTime.Now.AddDays(100);
            marketFilter.MarketStartTime = time;
            var eventTypes = client.listEventTypes(marketFilter);
            // forming a eventype id set for the eventype id extracted from the result
            ISet<string> eventypeIds = new HashSet<string>();
            foreach (EventTypeResult eventType in eventTypes)
            {
                if (eventType.EventType.Name.Equals(searchString))
                {
                    Console.WriteLine("\nFound event type for " + searchString + " : " + Json.JsonConvert.Serialize<EventTypeResult>(eventType));
                    //extracting eventype id
                    eventypeIds.Add(eventType.EventType.Id);
                }
            }
            if (eventypeIds.Count > 0) marketFilter.EventTypeIds = eventypeIds;
        }

        public IDictionary<string, string> GetEventSelectionIDs(string evType)
        {
            ISet<MarketProjection> marketProjections = new HashSet<MarketProjection>();
            marketProjections.Add(MarketProjection.EVENT);
            marketProjections.Add(MarketProjection.COMPETITION);
            marketProjections.Add(MarketProjection.EVENT_TYPE);
            marketProjections.Add(MarketProjection.RUNNER_DESCRIPTION);
            marketProjections.Add(MarketProjection.RUNNER_METADATA);
            var marketSort = MarketSort.MAXIMUM_TRADED;
            var maxResults = "1000";

            Console.WriteLine("\nGetting the next available MMA Matches");
            marketCatalogues = client.listMarketCatalogue(marketFilter, marketProjections, marketSort, maxResults);
            //extract the marketId of the next horse race
            IDictionary<string, string> fighterDictionary = new Dictionary<string, string>();
            IList<string> marketIds = new List<string>();
            ISet<string> fightNames = new HashSet<string>();
            foreach (MarketCatalogue f in marketCatalogues)
            {
                fightNames.Add(f.Event.Name);
                if (f.MarketName == evType || f.MarketName == evType + " (UNMANAGED)")
                {
                    marketIds.Add(f.MarketId);
                    foreach (RunnerDescription runner in f.Runners)
                    {
                        if (!fighterDictionary.ContainsKey(runner.SelectionId.ToString()))
                            fighterDictionary.Add(runner.SelectionId.ToString(), f.Event.Name);
                    }
                }
            }

            return fighterDictionary;
        }

        public IDictionary<string, string> GetRunnerSelectionIDs(string evType)
        {
            //as an example we requested runner metadata
            ISet<MarketProjection> marketProjections = new HashSet<MarketProjection>();
            marketProjections.Add(MarketProjection.EVENT);
            marketProjections.Add(MarketProjection.RUNNER_DESCRIPTION);
            var marketSort = MarketSort.MAXIMUM_TRADED;
            var maxResults = "100";

            Console.WriteLine("\nGetting the next available MMA Matches");
            marketCatalogues = client.listMarketCatalogue(marketFilter, marketProjections, marketSort, maxResults);
            //extract the marketId of the next horse race
            IDictionary<string, string> fighterDictionary = new Dictionary<string, string>();
            IList<string> marketIds = new List<string>();
            ISet<string> fightNames = new HashSet<string>();
            foreach (MarketCatalogue f in marketCatalogues)
            {
                fightNames.Add(f.Event.Name);
                if (f.MarketName == evType || f.MarketName == evType + " (UNMANAGED)")
                {
                    marketIds.Add(f.MarketId);
                    foreach (RunnerDescription runner in f.Runners)
                    {
                        if (!fighterDictionary.ContainsKey(runner.SelectionId.ToString()))
                            fighterDictionary.Add(runner.SelectionId.ToString(), runner.RunnerName);
                    }
                }
            }

            return fighterDictionary;
        }

        public IDictionary<string, string> GetBettingDictionary(string evType)
        {
            //as an example we requested runner metadata
            ISet<MarketProjection> marketProjections = new HashSet<MarketProjection>();
            marketProjections.Add(MarketProjection.EVENT);
            marketProjections.Add(MarketProjection.RUNNER_DESCRIPTION);
            var marketSort = MarketSort.MAXIMUM_TRADED;
            var maxResults = "100";

            Console.WriteLine("\nGetting the next available MMA Matches");
            marketCatalogues = client.listMarketCatalogue(marketFilter, marketProjections, marketSort, maxResults);
            //extract the marketId of the next horse race
            IDictionary<string, string> fighterDictionary = new Dictionary<string, string>();
            IList<string> marketIds = new List<string>();
            ISet<string> fightNames = new HashSet<string>();
            foreach (MarketCatalogue f in marketCatalogues)
            {
                fightNames.Add(f.Event.Name);
                if (f.MarketName == evType || f.MarketName == evType + " (UNMANAGED)")
                {
                    marketIds.Add(f.MarketId);
                    foreach (RunnerDescription runner in f.Runners)
                    {
                        if (!fighterDictionary.ContainsKey(runner.RunnerName))
                            fighterDictionary.Add(runner.RunnerName, f.Event.Name);
                    }
                }
            }

            return fighterDictionary;
        }

        public IDictionary<string, string> GetBettingInfo(string evType)
        {
            //as an example we requested runner metadata
            ISet<MarketProjection> marketProjections = new HashSet<MarketProjection>();
            marketProjections.Add(MarketProjection.EVENT);
            marketProjections.Add(MarketProjection.RUNNER_DESCRIPTION);
            var marketSort = MarketSort.MAXIMUM_TRADED;
            var maxResults = "100";

            Console.WriteLine("\nGetting the next available MMA Matches");
            marketCatalogues = client.listMarketCatalogue(marketFilter, marketProjections, marketSort, maxResults);
            //extract the marketId of the next horse race
            IDictionary<string, string> fighterDictionary = new Dictionary<string, string>();
            IList<string> marketIds = new List<string>();
            ISet<string> fightNames = new HashSet<string>();
            foreach (MarketCatalogue f in marketCatalogues)
            {
                fightNames.Add(f.Event.Name);
                if (f.MarketName == evType || f.MarketName == evType + " (UNMANAGED)")
                {
                    marketIds.Add(f.MarketId);
                    foreach (RunnerDescription runner in f.Runners)
                    {
                        if (!fighterDictionary.ContainsKey(runner.RunnerName))
                            fighterDictionary.Add(runner.RunnerName, f.Event.Name);
                    }
                    //if(!fighterDictionary.ContainsKey(f.Runners[0].RunnerName))
                    //    fighterDictionary.Add(f.Runners[0].RunnerName, f.Event.Name);
                    //if (!fighterDictionary.ContainsKey(f.Runners[1].RunnerName))
                    //    fighterDictionary.Add(f.Runners[1].RunnerName, f.Event.Name);
                }
            }

            return fighterDictionary;
        }

        private IList<MarketCatalogue> GetMarketCataloguesMM(string evType)
        {
            //as an example we requested runner metadata
            ISet<MarketProjection> marketProjections = new HashSet<MarketProjection>();
            marketProjections.Add(MarketProjection.EVENT);
            marketProjections.Add(MarketProjection.RUNNER_DESCRIPTION);
            var marketSort = MarketSort.MAXIMUM_TRADED;
            var maxResults = "100";

            Console.WriteLine("\nGetting the next available MMA Matches");
            var marketCatalogues = client.listMarketCatalogue(marketFilter, marketProjections, marketSort, maxResults);
            //extract the marketId of the next horse race
            IDictionary<string, long> fighterDictionary = new Dictionary<string, long>();
            IList<string> marketIds = new List<string>();
            ISet<string> fightNames = new HashSet<string>();
            foreach (MarketCatalogue f in marketCatalogues)
            {
                fightNames.Add(f.Event.Name);
                if (f.MarketName == evType || f.MarketName == evType + " (UNMANAGED)")
                {
                    marketIds.Add(f.MarketId);

                    if (f.Runners.Count == 2)
                    {
                        fighterDictionary.Add(f.Runners[0].RunnerName, f.Runners[0].SelectionId);
                        fighterDictionary.Add(f.Runners[1].RunnerName, f.Runners[1].SelectionId);
                    }
                }
            }
            return marketCatalogues;
        }

        public Dictionary<string, string> GetFightEvents(string searchString)
        {
            Dictionary<string, string> fightEventDictionary = new Dictionary<string, string>();
            //as an example we requested runner metadata
            ISet<MarketProjection> marketProjections = new HashSet<MarketProjection>();
            marketProjections.Add(MarketProjection.EVENT);
            marketProjections.Add(MarketProjection.RUNNER_DESCRIPTION);
            var marketSort = MarketSort.MAXIMUM_TRADED;
            var maxResults = "100";

            Console.WriteLine("\nGetting the next available MMA Matches");
            var marketCatalogues = client.listMarketCatalogue(marketFilter, marketProjections, marketSort, maxResults);

            foreach (MarketCatalogue f in marketCatalogues)
            {
                if (!fightEventDictionary.ContainsKey(f.Event.Name) && (f.MarketName == searchString || f.MarketName == searchString + " (UNMANAGED)"))
                    fightEventDictionary.Add(f.Event.Name, f.MarketId);
            }
            return fightEventDictionary;
        }

        public double GetIndividualOdds(string selectionID, string eveType)
        {
            IList<string> marketIds = new List<string>();
            foreach (MarketCatalogue f in marketCatalogues)
            {
                if ((f.MarketName == eveType || f.MarketName == eveType + " (UNMANAGED)") && f.Runners.Count == 2)
                {
                    marketIds.Add(f.MarketId);
                }
            }

            ISet<PriceData> priceData = new HashSet<PriceData>();
            //get all prices from the exchange
            //priceData.Add(PriceData.EX_ALL_OFFERS);

            var priceProjection = new PriceProjection();
            priceProjection.PriceData = priceData;
            priceProjection.ExBestOffersOverrides = new ExBestOffersOverrides();

            Console.WriteLine("\nGetting prices for market");
            var marketBook = client.listMarketBook(marketIds, priceProjection);

            if (marketBook.Count != 0)
            {
                foreach (MarketBook book in marketBook)
                {
                    foreach (Runner runner in book.Runners)
                    {
                        if (runner.SelectionId.ToString() == selectionID)
                            return Convert.ToDouble(runner.LastPriceTraded);
                    }
                }
            }
            //If unable to find return 0
            return 0;
        }

        public IDictionary<string, string> GetAllOdds(IList<string> selectionIDs, string eveType)
        {
            Dictionary<string, string> oddsDictionary = new Dictionary<string, string>();
            IList<string> marketIds = new List<string>();
            foreach (MarketCatalogue f in marketCatalogues)
            {
                //Removed 2 runners condition
                if (f.MarketName == eveType || f.MarketName == eveType + " (UNMANAGED)")
                {
                    marketIds.Add(f.MarketId);
                }
            }

            ISet<PriceData> priceData = new HashSet<PriceData>();
            //get all prices from the exchange
            //priceData.Add(PriceData.EX_ALL_OFFERS);

            var priceProjection = new PriceProjection();

            priceProjection.PriceData = priceData;

            Console.WriteLine("\nGetting prices for market");

            var marketBook = client.listMarketBook(marketIds, priceProjection);

            if (marketBook.Count != 0)
            {
                foreach (MarketBook book in marketBook)
                {
                    foreach (Runner runner in book.Runners)
                    {
                        if (selectionIDs.Contains(runner.SelectionId.ToString()) && !oddsDictionary.ContainsKey(runner.SelectionId.ToString()) && runner.LastPriceTraded != null)
                            oddsDictionary.Add(runner.SelectionId.ToString(), runner.LastPriceTraded.ToString());
                    }
                }
            }
            //If unable to find return empty dictionary
            return oddsDictionary;
        }

        public DateTime GetDate(string fightName)
        {
            DateTime date = new DateTime();

            foreach (MarketCatalogue f in marketCatalogues)
            {
                if (fightName == f.Event.Name)
                    return (DateTime)f.Event.OpenDate;
            }

            return date;
        }

        public IDictionary<string, string> GetAllDates(List<string> eventDictionary)
        {
            Dictionary<string, string> dateDictionary = new Dictionary<string, string>();
            //Make this use selection ID as key
            foreach (MarketCatalogue f in marketCatalogues)
            {
                if (eventDictionary.Contains(f.Event.Name) && !dateDictionary.ContainsKey(f.Event.Name))
                {
                    dateDictionary.Add(f.Event.Name, f.Event.OpenDate.ToString());
                }
            }
            return dateDictionary;
        }
    }
}