using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils.Candles.Models
{
    public class SymbolCandles
    {
        public string Symbol { get; set; }
        public List<Kline> Klines { get; set; }
        [JsonConstructor]
        private SymbolCandles() { }
        public SymbolCandles(string symbol, List<Kline> klines)
        {
            Symbol = symbol;
            Klines = klines;
        }
        public bool ContainsDate(DateTime date)
        {
            return Klines.Any(item => item.Date == date);
        }
    }
}
