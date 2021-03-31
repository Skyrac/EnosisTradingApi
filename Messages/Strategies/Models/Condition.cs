using Binance.Net.Enums;
using System.Collections.Concurrent;
using Utils.Candles.Models;

namespace Utils.Strategies.Models
{
    public abstract class Condition
    {
        public abstract bool IsTrue(ConcurrentDictionary<KlineInterval, ConcurrentDictionary<string, Kline>> candles);
    }
}
