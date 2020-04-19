using CommonClasses;
using Marketplace.TO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Marketplace
{
    public partial class MarketplaceMessenger
    {
        private List<MarketplaceEvent> reactiveEventList = new List<MarketplaceEvent>();
        private IObservable<List<MarketplaceBetOrder>> eventListObservable;
        private ISubject<List<MarketplaceBetOrder>> observerList = new Subject<List<MarketplaceBetOrder>>();

        public IObservable<MarketplaceEvent> BetSubscription(MarketplaceEvent ev)
        {
            if (reactiveEventList.Find(x => x.MarketId == ev.MarketId) == null)
            {
                reactiveEventList.Add(ev);
            }
            return Observable.Create<MarketplaceEvent>(observer =>
            {
                var eO = eventListObservable
                    .Where(x => x.Select(y => y.MarketId).Contains(ev.MarketId))
                    .Select(x => x.Where(p => p.MarketId == ev.MarketId).ToList())
                    .Subscribe(e =>
                    {
                       //remove runners not in there any more
                       ev.Runners.RemoveAll(o => !e.Select(t => t.SelectionId.ToString()).Contains(o.SelectionID));

                       //update runners
                       foreach (MarketplaceBetOrder incomingRunn in e)
                        {
                            var run = ev.Runners.Find(k => k.SelectionID == incomingRunn.SelectionId.ToString());
                            if (run == null)
                            {
                               //add if not contains
                               ev.Runners.Add(new MarketplaceRunner(incomingRunn.SelectionName, incomingRunn.SelectionId.ToString(), incomingRunn.Odds != null ? incomingRunn.Odds : "0"));
                            }
                            else
                            {
                                run.Odds = incomingRunn.Odds;
                            }
                        }

                        observer.OnNext(ev);
                    });

                //If not longer in the list
                eventListObservable
                   .Where(x => !x.Select(y => y.MarketId).Contains(ev.MarketId))
                   .Subscribe(e =>
                   {
                       eO.Dispose();
                       reactiveEventList.RemoveAll(x => x.MarketId == ev.MarketId);
                       observer.OnCompleted();
                   });
                return Disposable.Empty;
            });
        }


        public IObservable<List<MarketplaceBetOrder>> EventListUpdateObservableSetup(int Interval_s)
        {
            if (eventListObservable != null)
            {
                return eventListObservable;
            }
            else
            {
                //Event list broadcast setup
                SetupListBroadcast(Interval_s);


                //Setup subsdcriptionm to list output
                eventListObservable = Observable.Create<List<MarketplaceBetOrder>>(
                    observer =>
                    {
                        observerList.Subscribe(observer.OnNext);
                        return Disposable.Empty;
                    });
                return eventListObservable;
            }

        }

        private void SetupListBroadcast(int Interval_s)
        {
            ISet<PriceData> priceData = new HashSet<PriceData>
                {
                    //get all prices from the exchange
                    PriceData.EX_BEST_OFFERS
                };

            PriceProjection priceProjection = new PriceProjection
            {
                PriceData = priceData
            };

            var timer = new System.Timers.Timer();
            timer.Interval = 1000 * Interval_s;
            timer.Elapsed += (s, e) =>
            {
                List<MarketplaceBetOrder> orderList = new List<MarketplaceBetOrder>();

                Console.WriteLine($"\nGetting prices for market at {DateTime.Now}");

                IList<MarketBook> marketBook = new List<MarketBook>();

                for (int i = 0; i < reactiveEventList.Count; i = i + 10)
                {
                    int j = i + 10 > reactiveEventList.Count ? reactiveEventList.Count : i + 10;

                    IList<MarketBook> incrementalBook = client.listMarketBook(reactiveEventList.Select(x => x.MarketId).Skip(i).Take(j - i).ToList(), priceProjection);

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
                        var currentEvent = reactiveEventList.FirstOrDefault(x => x.MarketId == book.MarketId);
                        if (currentEvent != null)
                        {
                            foreach (Runner runner in book.Runners)
                            {
                                try
                                {
                                    MarketplaceRunner currentRunner = currentEvent.Runners.Find(x => x.SelectionID == runner.SelectionId.ToString());
                                    if (currentRunner != null)
                                    {
                                        var ord = new MarketplaceBetOrder("test", book.MarketId, runner.SelectionId.ToString(), runner.LastPriceTraded ?? 0, currentRunner.Name)
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
                                    //observer.OnError(ex);
                                    Console.WriteLine(ex.ToString());
                                }
                            }
                        }
                    }

                    observerList.OnNext(orderList);
                }

            };
            timer.Start();
        }
    }
}