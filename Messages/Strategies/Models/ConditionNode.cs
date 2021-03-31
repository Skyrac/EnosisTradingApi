using Binance.Net.Enums;
using System.Collections.Concurrent;
using Utils.Candles.Models;

namespace Utils.Strategies.Models
{
    public class ConditionNode : Condition
    {
        public override bool IsTrue(ConcurrentDictionary<KlineInterval, ConcurrentDictionary<string, Kline>> candles)
        {
            throw new System.NotImplementedException();
        }
    }
}
