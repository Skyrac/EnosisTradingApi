using Binance.Net.Enums;
using Skender.Stock.Indicators;
using System;
using System.Collections.Concurrent;
using System.Linq;
using Utils.Candles.Models;
using Utils.Indicators.Models;

namespace Utils.Strategies.Models
{
    public class ConditionItem
    {
        public string Name { get; set; }   //EMA_100 z. B. 
        public KlineInterval Interval { get; set; }
        public string Symbol { get; set; }
        public IndicatorProperties Indicator { get; set; } //User Selects Indicator -> Ema:100
        public int Index { get; set; }
        public decimal? GetValue(ConcurrentDictionary<KlineInterval, ConcurrentDictionary<string, ConcurrentDictionary<DateTime, Kline>>> candles)
        {
            if(!candles.ContainsKey(Interval) || !candles[Interval].ContainsKey(Symbol))
            {
                return -1;
            }
            var klines = candles[Interval][Symbol].Values;
            var index = klines.Count - 1 - Index;
            var indicator = klines.ElementAt(index).GetIndicator<ResultBase>(Name);
            return (decimal?) indicator.GetType().GetProperty(Indicator.WantedProperty).GetValue(indicator, null);
        }
        public void GenerateIndicators(ConcurrentDictionary<KlineInterval, ConcurrentDictionary<string, ConcurrentDictionary<DateTime, Kline>>> candles)
        {
            if(!candles.ContainsKey(Interval) || !candles[Interval].ContainsKey(Symbol))
            {
                return;
            }
            var klines = candles[Interval][Symbol];
            foreach(var indicator in Indicator.GenerateIndicators(klines.Values))
            {
                if (klines.ContainsKey(indicator.Date))
                {
                    klines[indicator.Date].FillIndicator(Name, indicator);
                }
            }
        }
    }
}
