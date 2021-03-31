using Binance.Net.Enums;
using Skender.Stock.Indicators;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Utils.Candles.Models;
using Utils.Indicators.Models;

namespace Utils.Strategies.Models
{
    public class ConditionItem
    {
        public string Name { get; set; }   //EMA_100 || Close (bei Candle)
        public KlineInterval Interval { get; set; }
        public string Symbol { get; set; }
        public IndicatorProperties Indicator { get; set; } //Empty if candle.close etc.
        public int Index { get; set; }
        private PropertyInfo _info;
        public decimal? GetValue(Dictionary<KlineInterval, Dictionary<string, Dictionary<DateTime, Kline>>> candles)
        {
            if(!candles.ContainsKey(Interval) || !candles[Interval].ContainsKey(Symbol))
            {
                return -1;
            }
            var klines = candles[Interval][Symbol].Values;
            var index = klines.Count - 1 - Index;
            if (Indicator != null)
            {
                var indicator = klines.ElementAt(index).GetIndicator<ResultBase>(Name);
                if(_info == null)
                {
                    _info = indicator.GetType().GetProperty(Indicator.WantedProperty);
                }
                return (decimal?)_info.GetValue(indicator, null);
            }
            var candle = klines.ElementAt(index);
            if (_info == null)
            {
                _info = candle.GetType().GetProperty(Indicator.WantedProperty);
            }
            return (decimal?)_info.GetValue(candle, null);
        }
        public void GenerateIndicators(Dictionary<KlineInterval, Dictionary<string, Dictionary<DateTime, Kline>>> candles)
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
