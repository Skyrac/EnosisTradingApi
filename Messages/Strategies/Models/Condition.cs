using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using Utils.Candles.Models;
using Utils.Trading.Enums;

namespace Utils.Strategies.Models
{
    public abstract class Condition
    {
        public abstract StrategyReturnModel IsTrue(Dictionary<KlineInterval, Dictionary<string, Dictionary<DateTime, Kline>>> candles, int index = -1);
        public abstract StrategyReturnModel GetDecimal(ESide side, Dictionary<KlineInterval, Dictionary<string, Dictionary<DateTime, Kline>>> candles, int index = -1);
        public abstract List<ConditionItem> GetConditionItems();
    }
}
