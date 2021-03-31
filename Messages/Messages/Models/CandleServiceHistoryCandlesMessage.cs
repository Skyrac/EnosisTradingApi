using Binance.Net.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utils.Candles.Models;
using Utils.Messages.Enums;

namespace Utils.Messages.Models
{
    public class CandleServiceHistoryCandlesMessage : BaseMessage
    {
        public List<IntervalSymbols> Candles { get; set; }
        [JsonConstructor]
        public CandleServiceHistoryCandlesMessage() : base(EMessage.CandleServiceHistoryCandles)
        {
        }

        public CandleServiceHistoryCandlesMessage(List<IntervalSymbols> candles) : this()
        {
            Candles = candles;
        }
    }
}
