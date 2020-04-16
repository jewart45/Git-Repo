using CommonClasses;
using Marketplace.TO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Marketplace
{
    public partial class MarketplaceMessenger
    {
        public IObservable<MarketplaceEvent> BetSubscription(MarketplaceEvent ev, int Interval_s)
        {
            return Observable.Create<MarketplaceEvent>(observer =>
           {
               var timer = new System.Timers.Timer();
               timer.Interval = 1000 * Interval_s;
               timer.Elapsed += (s, e) =>
               {
                   try
                   {
                        //get prices
                        var books = client.listMarketBook(new List<string>() { ev.MarketId }, priceProjection);
                       if (books.Count <= 0)
                       {
                           observer.OnCompleted();
                       }
                       else if (books.First().MarketId == ev.MarketId)
                       {
                            //run through runners
                            var book = books.First();
                           foreach (Runner runner in book.Runners)
                           {
                               MarketplaceRunner currentRunner = ev.Runners.Find(x => x.SelectionID == runner.SelectionId.ToString());
                               if (currentRunner != null && runner.LastPriceTraded != null)
                               {
                                   currentRunner.Odds = runner.LastPriceTraded.ToString();
                                   if (runner.Status == RunnerStatus.WINNER)
                                   {
                                       ev.Winner = currentRunner.Name;
                                   }
                               }
                           }
                            //pass on new event
                            observer.OnNext(ev);
                       }
                   }
                   catch (System.Exception ex)
                   {
                       observer.OnError(ex);
                   }
               };
               timer.Start();
               return Disposable.Empty;
           });
        }
    }
}