using Binance.Net.Enums;
using Newtonsoft.Json;
using Utils.Candles.Models;
using Utils.Enums;
using Utils.Messages.Enums;

namespace Utils.Messages.Models
{
    public class CandleServiceSubscriptionMessage : BaseMessage
    {
        public EExchange Exchange { get; set; }
        public WrappedIntervalCandles[] SubscriptionItems { get; set; }
        public int RequiredCandles { get; set; }

        [JsonConstructor]
        private CandleServiceSubscriptionMessage() : base(EMessage.CandleServiceSubscription) { }

        public CandleServiceSubscriptionMessage(EExchange exchange, int requiredCandles, params WrappedIntervalCandles[] items) : this()
        {
            Exchange = exchange;
            SubscriptionItems = items;
            RequiredCandles = requiredCandles;
        }
    }
}
