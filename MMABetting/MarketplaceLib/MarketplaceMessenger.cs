using CommonClasses;
using Marketplace.TO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Marketplace
{
    public partial class MarketplaceMessenger
    {
        private static readonly string appKey = "r9pFKKDlrpQknNvB";
        private static readonly string sessionTokenStatic = "5clUqvyg1RczWBw1MpvY7Ug0otjZRvAjcwHFlXYbqa8=";
        private static readonly string Url = "https://api.betfair.com/exchange/betting";
        private static readonly string UrlAccount = "https://api.betfair.com/exchange/account";

        public List<Dictionary<string, string>> eventList;
        private MarketFilter marketFilter;
        private IList<MarketCatalogue> marketCatalogues;

        private PriceProjection priceProjection = new PriceProjection
        {
            ExBestOffersOverrides = new ExBestOffersOverrides() { BestPricesDepth = 1 },
            PriceData = new HashSet<PriceData>(),
            Virtualise = false
        };

        //Betfair_Non_interactive_login.Program loginProgram;
        private IClient client;

        private IClient clientAccount;

        public List<MarketplaceBetResult> GetSettledBets(TimeSpan timeSpan)
        {
            var range = new TimeRange();
            var now = DateTime.Now;
            range.From = now.Subtract(timeSpan);
            range.To = now;
            var results = new List<MarketplaceBetResult>();
            var report = client.listClearedOrders(BetStatus.SETTLED);

            foreach (var order in report.ClearedOrders)
            {
                var n = new MarketplaceBetResult(order.EventId, marketId: order.MarketId)
                {
                    PlacedDate = order.PlacedDate,
                    SettledDate = order.SettledDate,
                    MatchedAmount = order.SizeSettled,
                    Commission = order.Commission,
                    Profit = order.Profit,

                    Win = order.Profit > 0
                };
                results.Add(n);
            }

            return results;
        }

        public List<MarketplaceBetOrder> GetCurrentOffers(string eveType, bool virtualise, List<MarketplaceEvent> eventList)
        {
            List<MarketplaceBetOrder> orderList = new List<MarketplaceBetOrder>();
            IList<string> marketIds = new List<string>();
            foreach (MarketCatalogue f in marketCatalogues)
            {
                //Removed 2 runners condition
                if (f.MarketName == eveType || f.MarketName == eveType + " (UNMANAGED)" || f.MarketName.Trim() == eveType + " - Unmanaged" || eveType == "All")
                {
                    marketIds.Add(f.MarketId);
                }
            }

            ISet<PriceData> priceData = new HashSet<PriceData>
            {
                //get all prices from the exchange
                PriceData.EX_BEST_OFFERS
            };

            PriceProjection priceProjection = new PriceProjection
            {
                PriceData = priceData,
                Virtualise = virtualise
            };

            Console.WriteLine("\nGetting prices for market");

            IList<MarketBook> marketBook = new List<MarketBook>();

            for (int i = 0; i < marketIds.Count; i = i + 10)
            {
                int j = i + 10 > marketIds.Count ? marketIds.Count : i + 10;

                IList<MarketBook> incrementalBook = client.listMarketBook(marketIds.Skip(i).Take(j - i).ToList(), priceProjection);

                foreach (MarketBook price in incrementalBook)
                {
                    marketBook.Add(price);
                }
            }

            if (marketBook.Count != 0)
            {
                foreach (MarketBook book in marketBook)
                {
                    //Only look at events in the list
                    var currentEvent = eventList.FirstOrDefault(x => x.MarketId == book.MarketId);
                    if (currentEvent != null)
                    {
                        foreach (Runner runner in book.Runners)
                        {
                            try
                            {
                                MarketplaceRunner currentRunner = currentEvent.Runners.Find(x => x.SelectionID == runner.SelectionId.ToString());
                                if (currentRunner != null && runner.ExchangePrices.AvailableToBack != null)
                                {
                                    var ord = new MarketplaceBetOrder("test", book.MarketId, runner.SelectionId.ToString(), runner.ExchangePrices.AvailableToBack.First().Price, currentRunner.Name)
                                    {
                                        EventStart = currentEvent.Date,
                                        EventName = currentEvent.Name,
                                        LastTradedOddsDecimal = runner.LastPriceTraded != null ? (double)runner.LastPriceTraded : 0,
                                    };
                                    orderList.Add(ord);
                                }
                            }
                            catch (System.Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        }
                    }
                }
            }
            return orderList;
        }

        public List<string> PlaceOrders(List<MarketplaceBetOrder> selections, double sizeToBet)
        {
            var placed = new List<string>();
            try
            {
                foreach (var selId in selections)
                {
                    IList<PlaceInstruction> placeInstructions = new List<PlaceInstruction>();
                    var placeInstruction = new PlaceInstruction
                    {
                        Handicap = 0,
                        Side = Side.BACK,
                        OrderType = OrderType.LIMIT
                    };

                    var limitOrder = new LimitOrder
                    {
                        PersistenceType = PersistenceType.LAPSE,
                        Price = selId.OddsDecimal > selId.LastTradedOddsDecimal ? selId.OddsDecimal : selId.LastTradedOddsDecimal,
                        Size = sizeToBet
                     
                    };

                    placeInstruction.LimitOrder = limitOrder;
                    placeInstruction.SelectionId = selId.SelectionId;
                    placeInstructions.Add(placeInstruction);

                    var customerRef = selId.MarketId;
                    //This Is Where it orders!
                    var placeExecutionReport = client.placeOrders(selId.MarketId, customerRef, placeInstructions);
                    if (placeExecutionReport.Status == ExecutionReportStatus.SUCCESS)
                    {
                        placed.Add(selId.EventName);
                    }
                    else
                    {
                        //error
                    }
                }

                return placed;
            }
            catch (System.Exception Ex)
            {
                MessageBox.Show(Ex.ToString());
                return placed;
            }
        }

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
            string appkey = appKey;

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
                clientAccount = new JsonRpcClient(UrlAccount, appkey, sessionToken);
            }
            Console.WriteLine("\nBeginning sample run!\n");
            try
            {
                marketFilter = new MarketFilter();
                //marketFilter.MarketIds = new HashSet<string>();
                //marketFilter.MarketIds.Add("26420387");
                marketCatalogues = new List<MarketCatalogue>();

                IList<EventTypeResult> eventTypes = client.listEventTypes(marketFilter);
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
            marketFilter = new MarketFilter();
            TimeRange time = new TimeRange
            {
                From = DateTime.Now,
                To = DateTime.Now.AddDays(100)
            };
            marketFilter.MarketStartTime = time;
            IList<EventTypeResult> eventTypes = client.listEventTypes(marketFilter);
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
            if (eventypeIds.Count > 0)
            {
                marketFilter.EventTypeIds = eventypeIds;
            }
        }

        public IDictionary<string, string> GetEventSelectionIDs(string evType)
        {
            ISet<MarketProjection> marketProjections = new HashSet<MarketProjection>
            {
                MarketProjection.EVENT,
                MarketProjection.COMPETITION,
                MarketProjection.EVENT_TYPE,
                MarketProjection.RUNNER_DESCRIPTION,
            };
            MarketSort marketSort = MarketSort.MAXIMUM_TRADED;
            string maxResults = "1000";
            Console.WriteLine("\nGetting the next available MMA Matches");
            marketCatalogues = client.listMarketCatalogue(marketFilter, marketProjections, marketSort, maxResults);
            //extract the marketId of the next horse race
            IDictionary<string, string> fighterDictionary = new Dictionary<string, string>();
            IList<string> marketIds = new List<string>();
            ISet<string> EventNames = new HashSet<string>();
            foreach (MarketCatalogue f in marketCatalogues)
            {
                EventNames.Add(f.Event.Name.Trim());
                if (f.MarketName == evType || f.MarketName == evType + " (UNMANAGED)" || f.MarketName.Trim() == evType + " - Unmanaged")
                {
                    marketIds.Add(f.MarketId);
                    MarketplaceEvent ev = new MarketplaceEvent(f.Event.Name.Trim(), f.MarketId, f.MarketName.Trim().Replace(" - Unmanaged", "").Replace(" (UNMANAGED)", ""));
                    foreach (RunnerDescription runner in f.Runners)
                    {
                        ev.Runners.Add(new MarketplaceRunner(runner.RunnerName, runner.SelectionId.ToString()));
                        if (!fighterDictionary.ContainsKey(runner.SelectionId.ToString()))
                        {
                            fighterDictionary.Add(runner.SelectionId.ToString(), f.Event.Name);
                        }
                    }
                }
            }

            return fighterDictionary;
        }

        public List<MarketplaceEvent> GetEventSelectionIDs(string evType, string competition)
        {
            List<MarketplaceEvent> listToReturn = new List<MarketplaceEvent>();
            ISet<MarketProjection> marketProjections = new HashSet<MarketProjection>
            {
                MarketProjection.EVENT,
                MarketProjection.COMPETITION,
                MarketProjection.EVENT_TYPE,
                MarketProjection.RUNNER_DESCRIPTION,
            };
            MarketSort marketSort = MarketSort.MAXIMUM_TRADED;
            string maxResults = "1000";
            Console.WriteLine("\nGetting the next available MMA Matches");
            marketCatalogues = client.listMarketCatalogue(marketFilter, marketProjections, marketSort, maxResults);
            //extract the marketId of the next horse race
            IDictionary<string, string> fighterDictionary = new Dictionary<string, string>();
            IList<string> marketIds = new List<string>();
            ISet<string> EventNames = new HashSet<string>();
            foreach (MarketCatalogue f in marketCatalogues)
            {
                EventNames.Add(f.Event.Name.Trim());
                if ((f.MarketName == evType || f.MarketName == evType + " (UNMANAGED)" || f.MarketName.Trim() == evType + " - Unmanaged" || evType == "All") && (f.Competition.Name == competition || competition == "All"))
                {
                    marketIds.Add(f.MarketId);
                    MarketplaceEvent ev = new MarketplaceEvent(f.Event.Name.Trim(), (DateTime)f.Event.OpenDate, f.MarketId, f.MarketName.Trim().Replace(" - Unmanaged", "").Replace(" (UNMANAGED)", ""));

                    foreach (RunnerDescription runner in f.Runners)
                    {
                        ev.Runners.Add(new MarketplaceRunner(runner.RunnerName, runner.SelectionId.ToString()));

                        if (!fighterDictionary.ContainsKey(runner.SelectionId.ToString()))
                        {
                            fighterDictionary.Add(runner.SelectionId.ToString(), f.Event.Name);
                        }
                    }
                    listToReturn.Add(ev);
                }
            }

            return listToReturn;
        }

        public IDictionary<string, string> GetRunnerSelectionIDs(string evType)
        {
            //as an example we requested runner metadata
            ISet<MarketProjection> marketProjections = new HashSet<MarketProjection>
            {
                MarketProjection.EVENT,
                MarketProjection.RUNNER_DESCRIPTION
            };
            MarketSort marketSort = MarketSort.MAXIMUM_TRADED;
            string maxResults = "1000";

            Console.WriteLine("\nGetting the next available MMA Matches");
            marketCatalogues = client.listMarketCatalogue(marketFilter, marketProjections, marketSort, maxResults);
            //extract the marketId of the next horse race
            IDictionary<string, string> fighterDictionary = new Dictionary<string, string>();
            IList<string> marketIds = new List<string>();
            ISet<string> EventNames = new HashSet<string>();
            foreach (MarketCatalogue f in marketCatalogues)
            {
                EventNames.Add(f.Event.Name);
                if (f.MarketName == evType || f.MarketName == evType + " (UNMANAGED)" || f.MarketName.Trim() == evType + " - Unmanaged" || evType == "All")
                {
                    marketIds.Add(f.MarketId);
                    foreach (RunnerDescription runner in f.Runners)
                    {
                        if (!fighterDictionary.ContainsKey(runner.RunnerName))
                        {
                            fighterDictionary.Add(runner.RunnerName, runner.SelectionId.ToString());
                        }
                    }
                }
            }

            return fighterDictionary;
        }

        public IDictionary<string, string> GetBettingDictionary(string evType)
        {
            List<MarketplaceEvent> EventListMarket = new List<MarketplaceEvent>();
            //as an example we requested runner metadata
            ISet<MarketProjection> marketProjections = new HashSet<MarketProjection>
            {
                MarketProjection.EVENT,
                MarketProjection.RUNNER_DESCRIPTION,
                MarketProjection.COMPETITION,

            };
            MarketSort marketSort = MarketSort.MAXIMUM_TRADED;
            string maxResults = "100";

            Console.WriteLine("\nGetting the next available MMA Matches");
            marketCatalogues = client.listMarketCatalogue(marketFilter, marketProjections, marketSort, maxResults);
            //extract the marketId of the next horse race
            IDictionary<string, string> fighterDictionary = new Dictionary<string, string>();
            IList<string> marketIds = new List<string>();
            ISet<string> EventNames = new HashSet<string>();
            foreach (MarketCatalogue f in marketCatalogues)
            {
                EventNames.Add(f.Event.Name);
                if (f.MarketName == evType || f.MarketName == evType + " (UNMANAGED)" || f.MarketName.Trim() == evType + " - Unmanaged" || evType == "All")
                {
                    marketIds.Add(f.MarketId);
                    MarketplaceEvent test = new MarketplaceEvent(f.Event.Name, f.MarketId, f.MarketName.Trim().Replace(" - Unmanaged", "").Replace(" (UNMANAGED)", ""));
                    foreach (RunnerDescription runner in f.Runners)
                    {
                        test.Runners.Add(new MarketplaceRunner(runner.RunnerName, runner.SelectionId.ToString()));
                        if (!fighterDictionary.ContainsKey(runner.SelectionId.ToString()))
                        {
                            fighterDictionary.Add(runner.SelectionId.ToString(), f.Event.Name);
                        }
                    }
                    EventListMarket.Add(test);
                }
            }

            return fighterDictionary;
        }

        public IDictionary<string, string> GetBettingInfo(string evType)
        {
            //as an example we requested runner metadata
            ISet<MarketProjection> marketProjections = new HashSet<MarketProjection>
            {
                MarketProjection.EVENT,
                MarketProjection.RUNNER_DESCRIPTION
            };
            MarketSort marketSort = MarketSort.MAXIMUM_TRADED;
            string maxResults = "100";

            Console.WriteLine("\nGetting the next available MMA Matches");
            marketCatalogues = client.listMarketCatalogue(marketFilter, marketProjections, marketSort, maxResults);
            //extract the marketId of the next horse race
            IDictionary<string, string> fighterDictionary = new Dictionary<string, string>();
            IList<string> marketIds = new List<string>();
            ISet<string> EventNames = new HashSet<string>();
            foreach (MarketCatalogue f in marketCatalogues)
            {
                EventNames.Add(f.Event.Name);
                if (f.MarketName == evType || f.MarketName == evType + " (UNMANAGED)" || f.MarketName.Trim() == evType + " - Unmanaged" || evType == "All")
                {
                    marketIds.Add(f.MarketId);
                    foreach (RunnerDescription runner in f.Runners)
                    {
                        if (!fighterDictionary.ContainsKey(runner.RunnerName))
                        {
                            fighterDictionary.Add(runner.RunnerName, f.Event.Name);
                        }
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
            ISet<MarketProjection> marketProjections = new HashSet<MarketProjection>
            {
                MarketProjection.EVENT,
                MarketProjection.RUNNER_DESCRIPTION,
            };
            MarketSort marketSort = MarketSort.MAXIMUM_TRADED;
            string maxResults = "100";

            Console.WriteLine("\nGetting the next available MMA Matches");
            IList<MarketCatalogue> marketCatalogues = client.listMarketCatalogue(marketFilter, marketProjections, marketSort, maxResults);
            //extract the marketId of the next horse race
            IDictionary<string, long> fighterDictionary = new Dictionary<string, long>();
            IList<string> marketIds = new List<string>();
            ISet<string> EventNames = new HashSet<string>();
            foreach (MarketCatalogue f in marketCatalogues)
            {
                EventNames.Add(f.Event.Name);
                if (f.MarketName == evType || f.MarketName == evType + " (UNMANAGED)" || f.MarketName.Trim() == evType + " - Unmanaged" || evType == "All")
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
            ISet<MarketProjection> marketProjections = new HashSet<MarketProjection>
            {
                MarketProjection.EVENT,
                MarketProjection.RUNNER_DESCRIPTION
            };
            MarketSort marketSort = MarketSort.MAXIMUM_TRADED;
            string maxResults = "100";

            Console.WriteLine("\nGetting the next available MMA Matches");
            IList<MarketCatalogue> marketCatalogues = client.listMarketCatalogue(marketFilter, marketProjections, marketSort, maxResults);

            foreach (MarketCatalogue f in marketCatalogues)
            {
                if (!fightEventDictionary.ContainsKey(f.Event.Name) && (f.MarketName == searchString || f.MarketName == searchString + " (UNMANAGED)") || f.MarketName.Trim() == searchString + " - Unmanaged")
                {
                    fightEventDictionary.Add(f.Event.Name, f.MarketId);
                }
            }
            return fightEventDictionary;
        }

        public double GetIndividualOdds(string selectionID, string eveType)
        {
            IList<string> marketIds = new List<string>();
            foreach (MarketCatalogue f in marketCatalogues)
            {
                if ((f.MarketName == eveType || f.MarketName == eveType + " (UNMANAGED)" || f.MarketName.Trim() == eveType + " - Unmanaged" || eveType == "All") && f.Runners.Count == 2)
                {
                    marketIds.Add(f.MarketId);
                }
            }

            ISet<PriceData> priceData = new HashSet<PriceData>();
            //get all prices from the exchange
            //priceData.Add(PriceData.EX_ALL_OFFERS);

            PriceProjection priceProjection = new PriceProjection
            {
                PriceData = priceData,
                ExBestOffersOverrides = new ExBestOffersOverrides(),
                Virtualise = true
            };

            Console.WriteLine("\nGetting prices for market");
            IList<MarketBook> marketBook = client.listMarketBook(marketIds, priceProjection);

            if (marketBook.Count != 0)
            {
                foreach (MarketBook book in marketBook)
                {
                    foreach (Runner runner in book.Runners)
                    {
                        if (runner.SelectionId.ToString() == selectionID)
                        {
                            return Convert.ToDouble(runner.LastPriceTraded);
                        }
                    }
                }
            }
            //If unable to find return 0
            return 0;
        }

        public IDictionary<string, string> GetAllOddsOld(IList<string> selectionIDs, string eveType)
        {
            Dictionary<string, string> oddsDictionary = new Dictionary<string, string>();
            IList<string> marketIds = new List<string>();
            foreach (MarketCatalogue f in marketCatalogues)
            {
                //Removed 2 runners condition
                if (f.MarketName == eveType || f.MarketName == eveType + " (UNMANAGED)" || f.MarketName.Trim() == eveType + " - Unmanaged" || eveType == "All")
                {
                    marketIds.Add(f.MarketId);
                }
            }

            ISet<PriceData> priceData = new HashSet<PriceData>();
            //get all prices from the exchange
            //priceData.Add(PriceData.EX_ALL_OFFERS);

            PriceProjection priceProjection = new PriceProjection
            {
                PriceData = priceData
            };

            Console.WriteLine("\nGetting prices for market");

            IList<MarketBook> marketBook = new List<MarketBook>();

            for (int i = 0; i < marketIds.Count; i = i + 50)
            {
                int j = i + 50 > marketIds.Count ? marketIds.Count : i + 50;

                IList<MarketBook> incrementalBook = client.listMarketBook(marketIds.Skip(i).Take(j - i).ToList(), priceProjection);

                foreach (MarketBook price in incrementalBook)
                {
                    marketBook.Add(price);
                }
            }

            if (marketBook.Count != 0)
            {
                foreach (MarketBook book in marketBook)
                {
                    foreach (Runner runner in book.Runners)
                    {
                        if (selectionIDs.Contains(runner.SelectionId.ToString()) && !oddsDictionary.ContainsKey(runner.SelectionId.ToString()) && runner.LastPriceTraded != null)
                        {
                            oddsDictionary.Add(runner.SelectionId.ToString(), runner.LastPriceTraded.ToString());
                        }
                    }
                }
            }
            //If unable to find return empty dictionary
            return oddsDictionary;
        }

        public IDictionary<string, string> GetAllOddsOld(IList<string> selectionIDs, string eveType, bool virtualise)
        {
            Dictionary<string, string> oddsDictionary = new Dictionary<string, string>();
            IList<string> marketIds = new List<string>();
            foreach (MarketCatalogue f in marketCatalogues)
            {
                //Removed 2 runners condition
                if (f.MarketName == eveType || f.MarketName == eveType + " (UNMANAGED)" || f.MarketName.Trim() == eveType + " - Unmanaged" || eveType == "All")
                {
                    marketIds.Add(f.MarketId);
                }
            }

            ISet<PriceData> priceData = new HashSet<PriceData>
            {
                //get all prices from the exchange
                PriceData.EX_ALL_OFFERS
            };

            PriceProjection priceProjection = new PriceProjection
            {
                PriceData = priceData,
                Virtualise = virtualise,
            };

            Console.WriteLine("\nGetting prices for market");

            IList<MarketBook> marketBook = new List<MarketBook>();

            for (int i = 0; i < marketIds.Count; i = i + 5)
            {
                int j = i + 5 > marketIds.Count ? marketIds.Count : i + 5;

                IList<MarketBook> incrementalBook = client.listMarketBook(marketIds.Skip(i).Take(j - i).ToList(), priceProjection);

                foreach (MarketBook price in incrementalBook)
                {
                    marketBook.Add(price);
                }
            }

            if (marketBook.Count != 0)
            {
                foreach (MarketBook book in marketBook)
                {
                    foreach (Runner runner in book.Runners)
                    {
                        if (selectionIDs.Contains(runner.SelectionId.ToString()) && !oddsDictionary.ContainsKey(runner.SelectionId.ToString()) && runner.LastPriceTraded != null)
                        {
                            oddsDictionary.Add(runner.SelectionId.ToString(), runner.LastPriceTraded.ToString());
                        }
                        var p = runner.ExchangePrices?.AvailableToBack;
                        if (p != null)
                        {
                            foreach (var k in p)
                            {
                                var test = k.Price;
                                var test2 = k.Size;
                            }
                        }
                    }
                }
            }
            //If unable to find return empty dictionary
            return oddsDictionary;
        }

        public List<MarketplaceEvent> GetAllOdds(List<MarketplaceEvent> eventList, string eveType, string competition)
        {
            IList<string> marketIds = new List<string>();
            foreach (MarketCatalogue f in marketCatalogues)
            {
                //Removed 2 runners condition
                if ((f.MarketName == eveType || f.MarketName == eveType + " (UNMANAGED)" || f.MarketName.Trim() == eveType + " - Unmanaged" || eveType == "All") && (f.Competition.Name == competition || competition == "All"))
                {
                    marketIds.Add(f.MarketId);
                }
            }

            //get all prices from the exchange
            //priceData.Add(PriceData.EX_ALL_OFFERS);

           

            Console.WriteLine("\nGetting prices for market");

            IList<MarketBook> marketBook = new List<MarketBook>();
            IList<MarketBook> incrementalBook;
            for (int i = 0; i < marketIds.Count; i = i + 50)
            {

                int j = i + 50 > marketIds.Count ? marketIds.Count : i + 50;
                try
                {
                    incrementalBook = client.listMarketBook(marketIds.Skip(i).Take(j - i).ToList(), priceProjection);
                }
                catch(System.Exception ex)
                {
                    incrementalBook = new List<MarketBook>();
                    Console.WriteLine(ex);
                }
                foreach (MarketBook price in incrementalBook)
                {
                    marketBook.Add(price);
                }
            }

            if (marketBook.Count != 0)
            {
                foreach (MarketBook book in marketBook)
                {
                    MarketplaceEvent currentEvent = eventList.FirstOrDefault(x => x.MarketId == book.MarketId);
                    if (currentEvent != null)
                    {
                        foreach (Runner runner in book.Runners)
                        {
                            MarketplaceRunner currentRunner = currentEvent.Runners.Find(x => x.SelectionID == runner.SelectionId.ToString());
                            if (currentRunner != null && runner.LastPriceTraded != null)
                            {
                                currentRunner.Odds = runner.LastPriceTraded.ToString();
                                if (runner.Status == RunnerStatus.WINNER)
                                {
                                    currentEvent.Winner = currentRunner.Name;
                                }
                            }
                            else
                            {

                            }
                        }
                    }
                    else
                    {

                    }
                }
            }
            return eventList;
        }

        public DateTime GetDate(string EventName)
        {
            DateTime date = new DateTime();

            foreach (MarketCatalogue f in marketCatalogues)
            {
                if (EventName == f.Event.Name)
                {
                    return (DateTime)f.Event.OpenDate;
                }
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

        public List<string> GetEventTypes(string searchString)
        {
            SetMarketFilter(searchString);

            //as an example we requested runner metadata
            ISet<MarketProjection> marketProjections = new HashSet<MarketProjection>
            {
                MarketProjection.EVENT,
                MarketProjection.RUNNER_DESCRIPTION
            };
            MarketSort marketSort = MarketSort.MAXIMUM_TRADED;
            string maxResults = "1000";

            Console.WriteLine("\nGetting the next available MMA Matches");
            marketCatalogues = client.listMarketCatalogue(marketFilter, marketProjections, marketSort, maxResults);
            //extract the marketId of the next horse race
            List<string> eventTypes = new List<string>();
            IList<string> marketIds = new List<string>();
            ISet<string> EventNames = new HashSet<string>();
            foreach (MarketCatalogue f in marketCatalogues)
            {
                //EventNames.Add(f.Event.Name);
                if (!eventTypes.Contains(f.MarketName.Replace(" (UNMANAGED)", "").Replace(" - Unmanaged","")))
                {
                    eventTypes.Add(f.MarketName.Replace(" (UNMANAGED)", "").Replace(" - Unmanaged", ""));
                }
            }

            return eventTypes;
        }

        public MarketplaceAccountBalance GetAccountBalance()
        {
            MarketplaceAccountBalance bal;

            try
            {
                var b = clientAccount.getAccountFunds(Wallet.UK);

                bal = new MarketplaceAccountBalance { CurrentAvailable = b.AvailableToBetBalance, Exposure = b.Exposure };
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                bal = null;
            }

            return bal;
        }

        public double GetCommissionRate()
        {
            double rate;

            try
            {
                var b = clientAccount.getAccountFunds(Wallet.UK);

                rate = (5 * (1 - (b.DiscountRate / 100))) / 100;

                //rate = 0.05;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                rate = 0;
            }

            return rate;
        }

        public List<string> GetCompetitionTypes()
        {

            //as an example we requested runner metadata
            ISet<MarketProjection> marketProjections = new HashSet<MarketProjection>
            {
                MarketProjection.EVENT,
                MarketProjection.RUNNER_DESCRIPTION,
                MarketProjection.COMPETITION
            };
            MarketSort marketSort = MarketSort.MAXIMUM_TRADED;
            string maxResults = "1000";

            Console.WriteLine("\nGetting the next available MMA Matches");
            marketCatalogues = client.listMarketCatalogue(marketFilter, marketProjections, marketSort, maxResults);
            //extract the marketId of the next horse race
            List<string> compTypes = new List<string>();
            IList<string> marketIds = new List<string>();
            ISet<string> EventNames = new HashSet<string>();
            foreach (MarketCatalogue f in marketCatalogues)
            {
                //EventNames.Add(f.Event.Name);
                if (!compTypes.Contains(f.Competition.Name.Replace(" (UNMANAGED)", "").Replace(" - Unmanaged", "")))
                {
                    compTypes.Add(f.Competition.Name.Replace(" (UNMANAGED)", "").Replace(" - Unmanaged", ""));
                }
            }

            return compTypes;
        }
    }
}