using Binance.Net.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        public Dictionary<string, Dictionary<ESide, ConditionSequence>> StopLossConditions { get; set; }
        public Dictionary<string, Dictionary<ESide, ConditionSequence>> TakeProfitConditions { get; set; }

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
                    else
                    {
                        LongConditions[coreSymbol] = condition;
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
                    } else
                    {
                        ShortConditions[coreSymbol] = condition;
                    }
                    break;
            }
        }

        public void AddStopLossCondition(string coreSymbol, ESide side, ConditionSequence sequence)
        {
            if(StopLossConditions == null)
            {
                StopLossConditions = new Dictionary<string, Dictionary<ESide, ConditionSequence>>();
            }
            if(!StopLossConditions.ContainsKey(coreSymbol))
            {
                StopLossConditions.Add(coreSymbol, new Dictionary<ESide, ConditionSequence>() { { side, sequence } });
            }
            else if(!StopLossConditions[coreSymbol].ContainsKey(side)) 
            {
                StopLossConditions[coreSymbol].Add(side, sequence);
            } else
            {
                StopLossConditions[coreSymbol][side] = sequence;
            }
        }

        public void AddTakeProfitCondition(string coreSymbol, ESide side, ConditionSequence sequence)
        {
            if (TakeProfitConditions == null)
            {
                TakeProfitConditions = new Dictionary<string, Dictionary<ESide, ConditionSequence>>();
            }
            if (!TakeProfitConditions.ContainsKey(coreSymbol))
            {
                TakeProfitConditions.Add(coreSymbol, new Dictionary<ESide, ConditionSequence>() { { side, sequence } });
            }
            else if (!TakeProfitConditions[coreSymbol].ContainsKey(side))
            {
                TakeProfitConditions[coreSymbol].Add(side, sequence);
            }
            else
            {
                TakeProfitConditions[coreSymbol][side] = sequence;
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

            if(TakeProfitConditions != null)
            {
                foreach (var coreSymbol in TakeProfitConditions.Keys)
                {
                    foreach(var conditionSequence in TakeProfitConditions[coreSymbol].Values)
                    {
                        conditionItems.AddRange(conditionSequence.GetConditionItems());
                    }
                }
            }

            if (StopLossConditions != null)
            {
                foreach (var coreSymbol in StopLossConditions.Keys)
                {
                    foreach (var conditionSequence in StopLossConditions[coreSymbol].Values)
                    {
                        conditionItems.AddRange(conditionSequence.GetConditionItems());
                    }
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

            if (TakeProfitConditions != null)
            {
                foreach (var conditionSequence in TakeProfitConditions[coreSymbol].Values)
                {
                    conditionItems.AddRange(conditionSequence.GetConditionItems());
                }
            }

            if (StopLossConditions != null)
            {
                foreach (var conditionSequence in StopLossConditions[coreSymbol].Values)
                {
                    conditionItems.AddRange(conditionSequence.GetConditionItems());
                }
            }
            return conditionItems;
        }

        public List<TradeInfo> EnterTrade(Dictionary<KlineInterval, Dictionary<string, Dictionary<DateTime, Kline>>> candles, int index = -1)
        {
            var infos = new List<TradeInfo>();
            foreach(var coreSymbol in LongConditions.Keys)
            {
                var side = ESide.Long;
                var canEnter = false;
                if(LongConditions != null && LongConditions.ContainsKey(coreSymbol) && LongConditions[coreSymbol].IsTrue(candles, index))
                {
                    side = ESide.Long;
                    canEnter = true;
                } else if(ShortConditions != null && ShortConditions.ContainsKey(coreSymbol) && ShortConditions[coreSymbol].IsTrue(candles, index))
                {
                    //side = ESide.Short;
                    canEnter = true;
                }
                if (canEnter)
                {
                    var stopLoss = StopLossConditions != null && StopLossConditions.ContainsKey(coreSymbol) && StopLossConditions[coreSymbol].ContainsKey(side) ? StopLossConditions[coreSymbol][side].GetDecimal(side, candles, index) : -1;
                    var takeProfit = TakeProfitConditions != null && TakeProfitConditions.ContainsKey(coreSymbol) && TakeProfitConditions[coreSymbol].ContainsKey(side) ? TakeProfitConditions[coreSymbol][side].GetDecimal(side, candles, index) : -1;
                    infos.Add(new TradeInfo()
                    {
                        Symbol = coreSymbol,
                        StopLoss = stopLoss,
                        TakeProfit = takeProfit,
                        Opened = DateTime.Now,
                        Side = side.ToString()
                    });
                }
            }
            return infos;
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
