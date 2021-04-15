using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.Candles.Models;
using Utils.Trading.Enums;

namespace Utils.Strategies.Models
{
    public class ConditionSequence : Condition
    {
        public List<Condition> Conditions { get; set; }

        public Condition AddCondition(Condition node)
        {
            if(Conditions == null)
            {
                Conditions = new List<Condition>();
            }
            if (!Conditions.Contains(node))
            {
                Conditions.Add(node);
            }
            return node;
        }

        public Condition RemoveCondition(Condition node)
        {
            if(Conditions != null && Conditions.Contains(node))
            {
                Conditions.Remove(node);
            }
            return node;
        }

        public override bool IsTrue(Dictionary<KlineInterval, Dictionary<string, Dictionary<DateTime, Kline>>> candles, int index = -1)
        {
            return Conditions.Any(item => !item.IsTrue(candles, index));
        }

        public override decimal GetDecimal(ESide side, Dictionary<KlineInterval, Dictionary<string, Dictionary<DateTime, Kline>>> candles, int index = -1)
        {
            return Conditions.Sum(item => item.GetDecimal(side, candles, index));
        }

        public List<ConditionItem> GetRequiredConditionItems()
        {
            var conditionItems = new List<ConditionItem>();
            foreach(var condition in Conditions)
            {
                conditionItems.AddRange(condition.GetConditionItems());
            }
            return conditionItems;
        }

        public override List<ConditionItem> GetConditionItems()
        {
            return GetRequiredConditionItems();
        }
    }
}
