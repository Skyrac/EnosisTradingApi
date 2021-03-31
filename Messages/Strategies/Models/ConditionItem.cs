using Binance.Net.Enums;
using System;
using System.Collections.Concurrent;
using Utils.Candles.Models;
using Utils.Indicators.Models;

namespace Utils.Strategies.Models
{
    public class ConditionItem
    {
        public KlineInterval Interval { get; set; }
        public string Symbol { get; set; }
        public IndicatorProperties Indicator { get; set; }
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
                    klines[indicator.Date].FillIndicator(Indicator.Name, indicator);
                }
            }
        }
    }
}
