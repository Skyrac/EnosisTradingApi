using Binance.Net.Enums;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Utils.Candles.Models;
using Utils.Strategies.Models;
using Utils.Trading;
using Utils.Trading.Enums;

namespace Utils.Strategies
{
    /// <summary>
    /// A Base Strategy can contain several LongConditions which default to one "core" symbol
    /// </summary>
    public class BaseStrategy
    {
        public Dictionary<string, ConditionSequence> LongConditions { get; set; }
        public Dictionary<string, ConditionSequence> ShortConditions { get; set; }

        [JsonConstructor]
        public BaseStrategy() { }

        public void AddConditionSequence(ESide side, string coreSymbol, ConditionSequence condition)
        {
            switch (side)
            {
                case ESide.Long:
                    if(LongConditions == null)
                    {
                        LongConditions = new Dictionary<string, ConditionSequence>();
                    }
                    if(!LongConditions.ContainsKey(coreSymbol))
                    {
                        LongConditions.Add(coreSymbol, condition);
                    }
                    break;
                case ESide.Short:
                    if (ShortConditions == null)
                    {
                        ShortConditions = new Dictionary<string, ConditionSequence>();
                    }
                    if (!ShortConditions.ContainsKey(coreSymbol))
                    {
                        ShortConditions.Add(coreSymbol, condition);
                    }
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Condition Items from all Core Symbols</returns>
        public List<ConditionItem> GetConditionItems()
        {
            var conditionItems = new List<ConditionItem>();
            if (LongConditions != null)
            {
                foreach (var coreSymbol in LongConditions.Keys)
                {
                    conditionItems.AddRange(LongConditions[coreSymbol].GetRequiredConditionItems());
                }
            }

            if (ShortConditions != null)
            {
                foreach (var coreSymbol in ShortConditions.Keys)
                {
                    conditionItems.AddRange(ShortConditions[coreSymbol].GetRequiredConditionItems());
                }
            }
            return conditionItems;
        }

        public List<ConditionItem> GetConditionItems(string coreSymbol)
        {
            var conditionItems = new List<ConditionItem>();
            if (LongConditions != null && LongConditions.ContainsKey(coreSymbol))
            {
                conditionItems.AddRange(LongConditions[coreSymbol].GetRequiredConditionItems());
            }

            if (ShortConditions != null && ShortConditions.ContainsKey(coreSymbol))
            {
                conditionItems.AddRange(ShortConditions[coreSymbol].GetRequiredConditionItems());
            }
            return conditionItems;
        }



        public virtual ECloseReason CloseLong(TradeInfo info, ConcurrentDictionary<KlineInterval, ConcurrentDictionary<string, Kline>> candles, int index = -1)
        {
            //var candle = index > 0 && index < klines.Count() ? klines.ElementAt(index) : klines.Last();
            //info.High = Math.Max(candle.High, info.High);
            //info.Low = Math.Min(candle.Low, info.Low);
            //if (candle.Low <= info.StopLoss)
            //{
            //    return CloseReason.StopLoss;
            //} else if(candle.High >= info.Stop)
            //{
            //    return CloseReason.TakeProfit;
            //}
            return ECloseReason.NoClose;
        }

        public virtual ECloseReason CloseShort(TradeInfo info, ConcurrentDictionary<KlineInterval, ConcurrentDictionary<string, Kline>> candles, int index = -1)
        {
            //var candle = index > 0 && index < klines.Count() ? klines.ElementAt(index) : klines.Last();
            //info.High = Math.Max(candle.High, info.High);
            //info.Low = Math.Min(candle.Low, info.Low);
            //if (candle.High >= info.StopLoss)
            //{
            //    return CloseReason.StopLoss;
            //}
            //else if (candle.Low <= info.Stop)
            //{
            //    return CloseReason.TakeProfit;
            //}
            return ECloseReason.NoClose;
        }
    }
}
