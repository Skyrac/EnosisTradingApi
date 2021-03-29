using Binance.Net.Enums;
using Newtonsoft.Json;
using Utils.Enums;
using Utils.Messages.Enums;

namespace Utils.Messages.Models
{
    public class CandleServiceSubscriptionMessage : BaseMessage
    {
        public KlineInterval Interval { get; set; }
        public EExchange Exchange { get; set; }
        public string[] Symbols { get; set; }
        [JsonConstructor]
        private CandleServiceSubscriptionMessage() : base(EMessage.Subscription) { }

        public CandleServiceSubscriptionMessage(EExchange exchange, KlineInterval interval, params string[] symbols) : this()
        {
            Exchange = exchange;
            Interval = interval;
            Symbols = symbols;
        }
    }
}
