using Newtonsoft.Json;
using System.Collections.Generic;
using Utils.Candles.Models;
using Utils.Enums;
using Utils.Messages.Enums;

namespace Utils.Messages.Models
{
    public class CandleServiceUpdateMessage : BaseMessage
    {
        public EExchange Exchange { get; set; }
        public List<WrappedIntervalCandles> IntervalCandles { get; set; }

        [JsonConstructor]
        private CandleServiceUpdateMessage() : base(EMessage.CandleServiceUpdate) { }

        public CandleServiceUpdateMessage(EExchange exchange, List<WrappedIntervalCandles> intervalCandles) : this()
        {
            Exchange = exchange;
            IntervalCandles = intervalCandles;
        }
    }
}
