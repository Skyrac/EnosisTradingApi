using Binance.Net.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Utils.Candles.Models;

namespace Utils.Strategies.Models
{
    public abstract class Condition
    {
        public abstract bool IsTrue(ConcurrentDictionary<KlineInterval, ConcurrentDictionary<string, ConcurrentDictionary<DateTime, Kline>>> candles);
        public abstract List<ConditionItem> GetConditionItems();
    }
}
