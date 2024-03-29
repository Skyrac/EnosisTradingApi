﻿using Binance.Net.Enums;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Utils.Candles.Models;
using Utils.Indicators;
using Utils.Indicators.Enums;
using Utils.Indicators.Models;

namespace Utils.Strategies.Models
{
    public class ConditionItem
    {

        public EIndicator Type { get; set; }
        public string Name { get; set; }   //EMA_100 || Close (bei Candle) || Value bei Set Value
        public KlineInterval Interval { get; set; }
        public string Symbol { get; set; }
        public IndicatorProperties Indicator { get; set; } //Empty if candle.close etc.
        public decimal? Value { get; set; }  //Lower valued as Indicator, but higher than candle close
        /// <summary>
        /// Index from Last Candle (1 = Previous Candle)
        /// </summary>
        public int Index { get; set; }
        private PropertyInfo _info;
        public decimal? GetValue(Dictionary<KlineInterval, Dictionary<string, Dictionary<DateTime, Kline>>> candles, int index = -1)
        {
            if (Type == EIndicator.Value)
            {
                return Value;
            }
            if (Symbol == null || !candles.ContainsKey(Interval) || !candles[Interval].ContainsKey(Symbol))
            {
                return -1;
            }
            var klines = candles[Interval][Symbol].Values;

            index = index == -1 ? klines.Count - 1 - Index : index - Index;
            if (index >= klines.Count || index < 0)
            {
                return -1;
            }
            var candle = klines.ElementAt(index);
            if (Indicator != null)
            {
                //var indicator = klines.ElementAt(index).GetIndicator<ResultBase>(Name);
                //if(indicator == null)
                //{
                //    //Maybe return false -1 instead?
                //    return -1;
                //}
                //if(_info == null)
                //{
                //    _info = indicator.GetType().GetProperty(Indicator.WantedProperty);
                //}
                return IndicatorValuePicker.GetValue(Type, candle, Name, Indicator.WantedProperty);
            }
            return IndicatorValuePicker.GetValue(Type, candle, Name, string.Empty);
            if (_info == null)
            {
                _info = candle.GetType().GetProperty(Name);
            }
            return (decimal?)_info.GetValue(candle, null);
        }
        public void GenerateIndicators(Dictionary<KlineInterval, Dictionary<string, Dictionary<DateTime, Kline>>> candles, int requiredCandles)
        {
            if(Symbol == null || !candles.ContainsKey(Interval) || !candles[Interval].ContainsKey(Symbol) || Indicator == null)
            {
                return;
            }
            var klines = candles[Interval][Symbol];
            if (klines.Count >= requiredCandles)
            {
                foreach (var indicator in Indicator.GenerateIndicators(klines.Values, requiredCandles, Type))
                {
                    if(indicator == null)
                    {
                        continue;
                    }
                    if (klines.ContainsKey(indicator.Date))
                    {
                        klines[indicator.Date].FillIndicator(Name, indicator);
                    }
                }
            }
        }
    }
}
