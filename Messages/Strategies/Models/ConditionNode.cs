using Binance.Net.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Utils.Candles.Models;
using Utils.Strategies.Enums;

namespace Utils.Strategies.Models
{
    public class ConditionNode : Condition
    {
        public ConditionItem FirstItem { get; set; }
        public BoolOperator Operator { get; set; }
        public ConditionItem SecondItem { get; set; }
        public override bool IsTrue(ConcurrentDictionary<KlineInterval, ConcurrentDictionary<string, ConcurrentDictionary<DateTime, Kline>>> candles)
        {
            switch (Operator)
            {
                case BoolOperator.GreaterOrEquals:
                    return FirstItem.GetValue(candles) >= SecondItem.GetValue(candles);

                case BoolOperator.LowerOrEquals:
                    return FirstItem.GetValue(candles) <= SecondItem.GetValue(candles);

                case BoolOperator.Equals:
                    return FirstItem.GetValue(candles) == SecondItem.GetValue(candles);

                case BoolOperator.Unlike:
                    return FirstItem.GetValue(candles) != SecondItem.GetValue(candles);

                case BoolOperator.GreaterThan:
                    return FirstItem.GetValue(candles) > SecondItem.GetValue(candles);

                case BoolOperator.LowerThan:
                    return FirstItem.GetValue(candles) < SecondItem.GetValue(candles);

                default:
                    return false;
            }
        }

        public override List<ConditionItem> GetConditionItems()
        {
            return new List<ConditionItem>() { FirstItem, SecondItem };
        }
    }
}
