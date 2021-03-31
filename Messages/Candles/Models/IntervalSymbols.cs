using Binance.Net.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils.Candles.Models
{
    public class IntervalSymbols
    {
        public KlineInterval Interval { get; set; }
        public List<SymbolCandles> Symbols { get; set; }
        [JsonConstructor]
        private IntervalSymbols() { }
        public IntervalSymbols(KlineInterval interval, KeyValuePair<string, List<Kline>> value)
        {
            Interval = interval;
            Symbols = new List<SymbolCandles>();
            Symbols.Add(new SymbolCandles(value.Key, value.Value));
        }
        public bool ContainsSymbol(string symbol)
        {
            return Symbols.Any(item => item.Symbol == symbol);
        }

        public SymbolCandles GetSymbolCandle(string symbol)
        {
            return Symbols.FirstOrDefault(item => item.Symbol == symbol);
        }

        public void AddSymbol(KeyValuePair<string, List<Kline>> symbolCandle)
        {
            Symbols.Add(new SymbolCandles(symbolCandle.Key, symbolCandle.Value));
        }
    }
}
