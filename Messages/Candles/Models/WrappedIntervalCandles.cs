using Binance.Net.Enums;
using Newtonsoft.Json;
using System;
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

        internal Dictionary<string, Dictionary<DateTime, Kline>> ConvertCandlesToDictionary()
        {
            var candles = new Dictionary<string, Dictionary<DateTime, Kline>>(Candles.Count);
            foreach(var candle in Candles)
            {
                if(!candles.ContainsKey(candle.Symbol))
                {
                    candles.Add(candle.Symbol, new Dictionary<DateTime, Kline>() { { candle.Kline.Date, candle.Kline } });
                } else
                {
                    candles[candle.Symbol].Add(candle.Kline.Date, candle.Kline);
                }
            }

            return candles;
        }
    }
}
