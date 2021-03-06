using System;
using System.Reactive.Linq;
using DynamicData;
using Financials.Common.Infrastucture;
using Financials.Common.Model;

namespace Financials.Common.Services
{
	public class TradesCache : ITradesCache, IDisposable
    {
		private readonly IDisposable _cleanup;
		public IObservableCache<Trade, long> Trades { get; }
		
		public TradesCache(IMessageListener<Trade> messageListener)
        {
	        if (messageListener == null) throw new ArgumentNullException("messageListener");

			// IMessageListener <Trade> acts as the back end for this example.
			Trades = messageListener.Messages
				.Buffer(TimeSpan.FromMilliseconds(50))	//add a small buffer so inital load (about 5000-10000) trades are batched
				.Where(buffer=> buffer.Count!=0)		//I hate the fact that empty buffers produce a notification
				.ToObservableChangeSet(trade=>trade.Id) //convert this into a dynamic data observable
				.AsObservableCache();					//cache it

			_cleanup = Trades;
        }
		
		public void Dispose()
        {
            _cleanup.Dispose();
        }
    }
}