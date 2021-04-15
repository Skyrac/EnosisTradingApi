using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.Candles.Models;
using Utils.Strategies.Models;
using Utils.Trading;

namespace Utils.Strategies
{
    public class Strategy
    {
        public int Id { get; set; }
        public int RequiredCandles { get; set; }
        public string Name { get; set; }
        public decimal BalancePerTrade { get; set; }
        public BaseStrategy EntryStrategy { get; set; }
        public BaseStrategy ExitStrategy { get; set; }
        public Dictionary<string, TradeInfo> OpenTrades { get; set; } = new Dictionary<string, TradeInfo>();
        private Dictionary<KlineInterval, List<string>> _requiredCandles;
        public Dictionary<KlineInterval, List<string>> GetRequiredIntervalCandles()
        {
            _requiredCandles = new Dictionary<KlineInterval, List<string>>();
            var conditionItems = new List<ConditionItem>();
            if(EntryStrategy != null)
            {
                conditionItems.AddRange(EntryStrategy.GetConditionItems());
            }
            if (ExitStrategy != null)
            {
                conditionItems.AddRange(ExitStrategy.GetConditionItems());
            }
            foreach (var conditionItem in conditionItems)
            {
                if (!_requiredCandles.ContainsKey(conditionItem.Interval))
                {
                    _requiredCandles.Add(conditionItem.Interval, new List<string>());
                }
                if (!_requiredCandles[conditionItem.Interval].Contains(conditionItem.Symbol))
                {
                    _requiredCandles[conditionItem.Interval].Add(conditionItem.Symbol);
                }
            }
            return _requiredCandles;
        }

        public void FillIndicators(Dictionary<KlineInterval, Dictionary<string, Dictionary<DateTime, Kline>>> candles)
        {
            var conditionItems = new List<ConditionItem>();
            if (EntryStrategy != null)
            {
                conditionItems.AddRange(EntryStrategy.GetConditionItems());
            }

            if (ExitStrategy != null)
            {
                conditionItems.AddRange(ExitStrategy.GetConditionItems());
            }

            var groupedByName = conditionItems.GroupBy(item => item.Name);
            foreach (var item in groupedByName)
            {
                item.First().GenerateIndicators(candles, RequiredCandles);
            }
        }

        public void SetupIndicators(Dictionary<KlineInterval, Dictionary<string, Dictionary<DateTime, Kline>>> candles)
        {
            var items = new Dictionary<KlineInterval, Dictionary<string, Dictionary<DateTime, Kline>>>();
            foreach (var interval in _requiredCandles.Keys)
            {
                if (!candles.ContainsKey(interval))
                {
                    continue;
                }
                items.Add(interval, new Dictionary<string, Dictionary<DateTime, Kline>>());
                foreach(var symbol in _requiredCandles[interval].Where(item => !string.IsNullOrEmpty(item)))
                {
                    if(!candles[interval].ContainsKey(symbol))
                    {
                        continue;
                    }
                    items[interval].Add(symbol, candles[interval][symbol]);
                }
            }
            FillIndicators(items);
        }
    }
}
