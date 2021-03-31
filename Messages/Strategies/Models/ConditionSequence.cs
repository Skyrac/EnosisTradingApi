using Binance.Net.Enums;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Utils.Candles.Models;
using System.Linq;
using System;

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

        public override bool IsTrue(Dictionary<KlineInterval, Dictionary<string, Dictionary<DateTime, Kline>>> candles)
        {
            return Conditions.Any(item => !item.IsTrue(candles));
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
