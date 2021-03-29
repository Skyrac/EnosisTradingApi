using Newtonsoft.Json;

namespace Utils.Candles.Models
{
    public class WrappedSymbolCandle
    {
        public string Symbol { get; set; }
        public Kline Kline { get; set; }

        [JsonConstructor]
        private WrappedSymbolCandle() { }

        public WrappedSymbolCandle(string symbol, Kline kline)
        {
            Symbol = symbol;
            Kline = kline;
        }
    }
}
