using Binance.Net.Enums;
using Flee.PublicTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utils.Candles.Models;
using Utils.Strategies.Enums;
using Utils.Trading.Enums;

namespace Utils.Strategies.Models
{
    public class ConditionNode : Condition
    {
        public List<ConditionItem> ConditionItems { get; set; }
        public List<EConditionOperator> ConditionOperators { get; set; }

        [JsonConstructor]
        private ConditionNode() { }

        public ConditionNode(List<ConditionItem> condtitions, List<EConditionOperator> operators)
        {
            ConditionItems = condtitions;
            ConditionOperators = operators;
        }

        public void AddCondition(ConditionItem firstItem, EConditionOperator conditionOperator, ConditionItem secondItem, EConditionOperator? connector)
        {
            ConditionItems.Add(firstItem);
            ConditionItems.Add(secondItem);
            if(connector != null)
            {
                ConditionOperators.Add((EConditionOperator) connector);
            }
            ConditionOperators.Add(conditionOperator);
        }


        public override StrategyReturnModel IsTrue(Dictionary<KlineInterval, Dictionary<string, Dictionary<DateTime, Kline>>> candles, int index = -1)
        {
            var sentence = "";
            var context = new ExpressionContext();
            for (var i = 0; i < ConditionItems.Count; i++) {
                var name = ConditionItems[i].Name + "_" + i;
                var value = ConditionItems[i].GetValue(candles, index);
                if(value == -1)
                {
                    return StrategyReturnModel.False;
                }
                context.Variables.Add(name, value);
                var operatorString = ConditionOperators.Count > i ? OperatorToString(ConditionOperators[i]) : "";
                sentence = string.Format("{0} {1} {2} ", sentence, name, operatorString);
            }

            return context.CompileGeneric<bool>(sentence).Evaluate() ? StrategyReturnModel.True : StrategyReturnModel.False;
            
        }

        private string OperatorToString(EConditionOperator conditionOperator)
        {
            switch (conditionOperator)
            {
                case EConditionOperator.GreaterOrEquals:
                    return ">=";
                case EConditionOperator.LowerOrEquals:
                    return "<=";
                case EConditionOperator.Equals:
                    return "==";
                case EConditionOperator.Unlike:
                    return "!=";
                case EConditionOperator.GreaterThan:
                    return ">";
                case EConditionOperator.LowerThan:
                    return "<";
                case EConditionOperator.Multiply:
                    return "*";
                case EConditionOperator.Divide:
                    return "/";
                case EConditionOperator.Add:
                    return "+";
                case EConditionOperator.Subtract:
                    return "-";
                case EConditionOperator.And:
                    return "AND";
                case EConditionOperator.Or:
                    return "OR";
            }
            return "";
        }

        public override StrategyReturnModel GetDecimal(ESide side, Dictionary<KlineInterval, Dictionary<string, Dictionary<DateTime, Kline>>> candles, int index = -1)
        {
            var sentence = "";
            var context = new ExpressionContext();
            //var variables = context.Variables;
            for (var i = 0; i < ConditionItems.Count; i++)
            {
                //var name = ConditionItems[i].Name + "_" + i;
                //variables.Add(name, ConditionItems[i].GetValue(candles, index));
                var operatorString = ConditionOperators.Count > i ? OperatorToString(ConditionOperators[i]) : "";
                sentence = string.Format("{0} {1} {2} ", sentence, ConditionItems[i].GetValue(candles, index), operatorString);
            }
            var result = new decimal(-1);
            try
            {
                result = context.CompileGeneric<decimal>(sentence).Evaluate();
            } catch(Exception ex)
            {
                Console.WriteLine("ERROR CONVERTING EXPRESSION {0} TO DECIMAL", sentence);
            }
            return new StrategyReturnModel() { Value = result };
        }

        public override List<ConditionItem> GetConditionItems()
        {
            return ConditionItems;
        }
    }
}
