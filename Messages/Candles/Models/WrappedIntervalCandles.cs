using Binance.Net.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Utils.Candles.Models
{
    public class WrappedIntervalCandles
    {
        public KlineInterval Interval { get; set; }
        public List<WrappedSymbolCandle> Candles { get; set; }

        [JsonConstructor]
        private WrappedIntervalCandles() { }
        public WrappedIntervalCandles(KlineInterval interval)
        {
            Interval = interval;
            Candles = new List<WrappedSymbolCandle>();
        }
        public WrappedIntervalCandles(KlineInterval interval, List<WrappedSymbolCandle> candles)
        {
            Interval = interval;
            Candles = candles;
        }
    }
}
