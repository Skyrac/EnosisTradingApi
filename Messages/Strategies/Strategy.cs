using System.Collections.Generic;
using Utils.Candles.Models;
using Utils.Trading;

namespace Utils.Strategies
{
    public class Strategy
    {
        public int Id { get; set; }
        public int RequiredCandles { get; set; }
        public string Name { get; set; }
        public decimal BalancePerTrade { get; set; }
        public List<WrappedIntervalCandles> RequiredIntervalSymbols { get; set; }
        public BaseEntryStrategy EntryStrategy { get; set; }
        public BaseStrategy ExitStrategy { get; set; }
        public Dictionary<string, TradeInfo> OpenTrades { get; set; } = new Dictionary<string, TradeInfo>();
    }
}
