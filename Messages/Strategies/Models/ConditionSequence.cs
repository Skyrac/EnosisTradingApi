using Binance.Net.Enums;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Utils.Candles.Models;
using System.Linq;
namespace Utils.Strategies.Models
{
    public class ConditionSequence : Condition
    {
        public List<Condition> Conditions { get; set; }

        public override bool IsTrue(ConcurrentDictionary<KlineInterval, ConcurrentDictionary<string, Kline>> candles)
        {
            return Conditions.Any(item => !item.IsTrue(candles));
        }
    }
}
