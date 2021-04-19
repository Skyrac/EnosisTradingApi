using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using TradingService.Trader;
using Utils.Clients;
using Utils.Enums;
using Utils.Indicators.Models;
using Utils.Strategies;
using Utils.Strategies.Enums;
using Utils.Strategies.Models;
using Utils.Trading.Enums;

namespace TradingService
{
    class Program
    {
        static void Main(string[] args)
        {
            var trader = new BaseTrader(new BinanceBaseClient(), EExchange.BinanceSpot);
            var strategy = new BaseStrategy();
            foreach (var coreSymbol in "MTLUSDT,HOTUSDT,CELRUSDT,CHZUSDT,DENTUSDT,MATICUSDT,ONEUSDT,BTTUSDT,RENUSDT,LINKUSDT,XRPUSDT,BANDUSDT,ETHUSDT,ZILUSDT,CVCUSDT,RLCUSDT,DOGEUSDT,WAVESUSDT,ATOMUSDT,XTZUSDT,IOSTUSDT,RVNUSDT,QTUMUSDT,IOTAUSDT,KAVAUSDT,BTCUSDT,ADAUSDT,ALGOUSDT,VETUSDT,EOSUSDT,TRXUSDT,XLMUSDT,HBARUSDT,FTMUSDT,NKNUSDT,BNBUSDT,BCHUSDT,DASHUSDT,LTCUSDT,ZECUSDT,ANKRUSDT,NEOUSDT,ICXUSDT,ETCUSDT,ZRXUSDT,ENJUSDT,ONTUSDT,BATUSDT,OMGUSDT".Split(","))
            {
                strategy.AddConditionSequence(ESide.Long, coreSymbol, GenerateLongCondition(coreSymbol, KlineInterval.OneMinute));
                strategy.AddConditionSequence(ESide.Short, coreSymbol, GenerateShortCondition(coreSymbol, KlineInterval.OneMinute));
                strategy.AddStopLossCondition(coreSymbol, ESide.Long, GenerateStopLoss(ESide.Long, coreSymbol, KlineInterval.OneMinute));
                strategy.AddTakeProfitCondition(coreSymbol, ESide.Long, GenerateTakeProfit(ESide.Long, coreSymbol, KlineInterval.OneMinute));
            }
            trader.AddStrategy(new Strategy()
            {
                RequiredCandles = 800,
                Name = "EmaCross",
                BalancePerTrade = 10,
                EntryStrategy = strategy
            });
            Console.ReadLine();
        }

        private static ConditionSequence GenerateStopLoss(ESide side, string symbol, KlineInterval interval)
        {
            var condition = new ConditionSequence();
            var subCondition = new ConditionSequence();
            var list = new List<EConditionOperator>();
            subCondition.AddCondition(new ConditionNode(
                new List<ConditionItem>() {
                    new ConditionItem()
                    {
                        Name = "Close",
                        Interval = interval,
                        Symbol = symbol,
                        Index = 0
                    },
                    new ConditionItem()
                    {
                        Name = "ATR_8",
                        Interval = interval,
                        Symbol = symbol,
                        Index = 0,
                        Indicator = new IndicatorProperties("Atr", "GetAtr", 8)
                    },
                    new ConditionItem()
                    {
                        Name = "Value",
                        Value = 3
                    }
                }, new List<EConditionOperator>()
                {
                    side == ESide.Long ? EConditionOperator.Subtract : EConditionOperator.Add,
                    EConditionOperator.Multiply
                }
            ));

            condition.AddCondition(subCondition);
            return condition;
        }

        private static ConditionSequence GenerateTakeProfit(ESide side, string symbol, KlineInterval interval, decimal riskRewardRatio = 2)
        {
            var condition = new ConditionSequence();
            var subCondition = new ConditionSequence();
            var list = new List<EConditionOperator>();
            subCondition.AddCondition(new ConditionNode(
                new List<ConditionItem>() {
                    new ConditionItem()
                    {
                        Name = "Close",
                        Interval = interval,
                        Symbol = symbol,
                        Index = 0
                    },
                    new ConditionItem()
                    {
                        Name = "ATR_8",
                        Interval = interval,
                        Symbol = symbol,
                        Index = 0,
                        Indicator = new IndicatorProperties("Atr", "GetAtr", 8)
                    },
                    new ConditionItem()
                    {
                        Name = "Value",
                        Value = 3
                    },
                    new ConditionItem()
                    {
                        Name = "Value",
                        Value = riskRewardRatio
                    }
                }, new List<EConditionOperator>()
                {
                    side == ESide.Long ? EConditionOperator.Add : EConditionOperator.Subtract,
                    EConditionOperator.Multiply,
                    EConditionOperator.Multiply
                }
            ));

            condition.AddCondition(subCondition);
            return condition;
        }
        private static ConditionSequence GenerateShortCondition(string symbol, KlineInterval interval)
        {
            var condition = new ConditionSequence(interval, symbol);
            var subCondition = new ConditionSequence();
            var conditionNode = new ConditionNode(new List<ConditionItem>(), new List<EConditionOperator>());

            conditionNode.AddCondition(new ConditionItem()
            {
                Name = "ADX_9",
                Interval = interval,
                Symbol = symbol,
                Index = 0,
                Indicator = new IndicatorProperties("Adx", "GetAdx", 9)
            }, EConditionOperator.GreaterThan, new ConditionItem()
            {
                Name = "Value",
                Interval = interval,
                Symbol = symbol,
                Index = 0,
                Value = 25
            }, null);

            conditionNode.AddCondition(new ConditionItem()
            {
                Name = "ADX_9",
                Interval = interval,
                Symbol = symbol,
                Index = 0,
                Indicator = new IndicatorProperties("Mdi", "GetAdx", 9)
            }, EConditionOperator.GreaterThan, new ConditionItem()
            {
                Name = "ADX_9",
                Interval = interval,
                Symbol = symbol,
                Index = 0,
                Indicator = new IndicatorProperties("Pdi", "GetAdx", 9)
            }, EConditionOperator.And);

            conditionNode.AddCondition(new ConditionItem()
            {
                Name = "VOL_SMA_48",
                Interval = interval,
                Symbol = symbol,
                Index = 0,
                Indicator = new IndicatorProperties("VolSma", "GetVolSma", 48)
            }, EConditionOperator.LowerThan, new ConditionItem()
            {
                Name = "Volume",
                Interval = interval,
                Symbol = symbol,
                Index = 0
            }, EConditionOperator.And);

            conditionNode.AddCondition(new ConditionItem()
            {
                Name = "EMA_8",
                Interval = interval,
                Symbol = symbol,
                Index = 0,
                Indicator = new IndicatorProperties("Ema", "GetEma", 8)
            }, EConditionOperator.LowerThan, new ConditionItem()
            {
                Name = "EMA_21",
                Interval = interval,
                Symbol = symbol,
                Index = 0,
                Indicator = new IndicatorProperties("Ema", "GetEma", 21)
            }, EConditionOperator.And);

            conditionNode.AddCondition(
                new ConditionItem()
                {
                    Name = "EMA_21",
                    Interval = interval,
                    Symbol = symbol,
                    Index = 0,
                    Indicator = new IndicatorProperties("Ema", "GetEma", 21)
                },
                EConditionOperator.LowerThan,
                new ConditionItem()
                {
                    Name = "EMA_34",
                    Interval = interval,
                    Symbol = symbol,
                    Index = 0,
                    Indicator = new IndicatorProperties("Ema", "GetEma", 34)
                },
                EConditionOperator.And
            );

            conditionNode.AddCondition(
                new ConditionItem()
                {
                    Name = "Close",
                    Interval = interval,
                    Symbol = symbol,
                    Index = 0
                },
                EConditionOperator.LowerThan,
                new ConditionItem()
                {
                    Name = "Close",
                    Interval = interval,
                    Symbol = symbol,
                    Index = 24,
                },
                EConditionOperator.And
            );

            conditionNode.AddCondition(
                new ConditionItem()
                {
                    Name = "EMA_34",
                    Interval = interval,
                    Symbol = symbol,
                    Index = 0,
                    Indicator = new IndicatorProperties("Ema", "GetEma", 34)
                },
                EConditionOperator.GreaterThan,
                new ConditionItem()
                {
                    Name = "Close",
                    Interval = interval,
                    Symbol = symbol,
                    Index = 0,
                },
                EConditionOperator.And
            );

            subCondition.AddCondition(conditionNode);

            var conditionNode2 = new ConditionNode(new List<ConditionItem>(), new List<EConditionOperator>());
            conditionNode2.AddCondition(new ConditionItem()
            {
                Name = "EMA_8",
                Interval = interval,
                Symbol = symbol,
                Index = 1,
                Indicator = new IndicatorProperties("Ema", "GetEma", 8)
            }, EConditionOperator.GreaterThan, new ConditionItem()
            {
                Name = "EMA_21",
                Interval = interval,
                Symbol = symbol,
                Index = 1,
                Indicator = new IndicatorProperties("Ema", "GetEma", 21)
            }, null);

            conditionNode2.AddCondition(
                new ConditionItem()
                {
                    Name = "EMA_21",
                    Interval = interval,
                    Symbol = symbol,
                    Index = 1,
                    Indicator = new IndicatorProperties("Ema", "GetEma", 21)
                },
                EConditionOperator.GreaterThan,
                new ConditionItem()
                {
                    Name = "EMA_34",
                    Interval = interval,
                    Symbol = symbol,
                    Index = 1,
                    Indicator = new IndicatorProperties("Ema", "GetEma", 34)
                },
                EConditionOperator.Or
            );

            conditionNode2.AddCondition(
                new ConditionItem()
                {
                    Name = "EMA_34",
                    Interval = interval,
                    Symbol = symbol,
                    Index = 1,
                    Indicator = new IndicatorProperties("Ema", "GetEma", 34)
                },
                EConditionOperator.LowerThan,
                new ConditionItem()
                {
                    Name = "Close",
                    Interval = interval,
                    Symbol = symbol,
                    Index = 1,
                },
                EConditionOperator.Or
            );
            subCondition.AddCondition(conditionNode2);
            condition.AddCondition(subCondition);
            return condition;
        }
        private static ConditionSequence GenerateLongCondition(string symbol, KlineInterval interval)
        {
            var condition = new ConditionSequence(interval, symbol);
            var subCondition = new ConditionSequence();
            var conditionNode = new ConditionNode(new List<ConditionItem>(), new List<EConditionOperator>());

            conditionNode.AddCondition(new ConditionItem()
            {
                Name = "ADX_9",
                Interval = interval,
                Symbol = symbol,
                Index = 0,
                Indicator = new IndicatorProperties("Adx", "GetAdx", 9)
            }, EConditionOperator.GreaterThan, new ConditionItem()
            {
                Name = "Value",
                Interval = interval,
                Symbol = symbol,
                Index = 0,
                Value = 25
            }, null);

            conditionNode.AddCondition(new ConditionItem()
            {
                Name = "ADX_9",
                Interval = interval,
                Symbol = symbol,
                Index = 0,
                Indicator = new IndicatorProperties("Mdi", "GetAdx", 9)
            }, EConditionOperator.LowerThan, new ConditionItem()
            {
                Name = "ADX_9",
                Interval = interval,
                Symbol = symbol,
                Index = 0,
                Indicator = new IndicatorProperties("Pdi", "GetAdx", 9)
            }, EConditionOperator.And);

            conditionNode.AddCondition(new ConditionItem()
            {
                Name = "VOL_SMA_48",
                Interval = interval,
                Symbol = symbol,
                Index = 0,
                Indicator = new IndicatorProperties("VolSma", "GetVolSma", 48)
            }, EConditionOperator.LowerThan, new ConditionItem()
            {
                Name = "Volume",
                Interval = interval,
                Symbol = symbol,
                Index = 0
            }, EConditionOperator.And);

            conditionNode.AddCondition(new ConditionItem()
            {
                Name = "EMA_8",
                Interval = interval,
                Symbol = symbol,
                Index = 0,
                Indicator = new IndicatorProperties("Ema", "GetEma", 8)
            }, EConditionOperator.GreaterThan, new ConditionItem()
            {
                Name = "EMA_21",
                Interval = interval,
                Symbol = symbol,
                Index = 0,
                Indicator = new IndicatorProperties("Ema", "GetEma", 21)
            }, EConditionOperator.And);

            conditionNode.AddCondition(
                new ConditionItem()
                {
                    Name = "EMA_21",
                    Interval = interval,
                    Symbol = symbol,
                    Index = 0,
                    Indicator = new IndicatorProperties("Ema", "GetEma", 21)
                },
                EConditionOperator.GreaterThan,
                new ConditionItem()
                {
                    Name = "EMA_34",
                    Interval = interval,
                    Symbol = symbol,
                    Index = 0,
                    Indicator = new IndicatorProperties("Ema", "GetEma", 34)
                },
                EConditionOperator.And
            );

            conditionNode.AddCondition(
                new ConditionItem()
                {
                    Name = "Close",
                    Interval = interval,
                    Symbol = symbol,
                    Index = 0
                },
                EConditionOperator.GreaterThan,
                new ConditionItem()
                {
                    Name = "Close",
                    Interval = interval,
                    Symbol = symbol,
                    Index = 24,
                },
                EConditionOperator.And
            );

            conditionNode.AddCondition(
                new ConditionItem()
                {
                    Name = "EMA_34",
                    Interval = interval,
                    Symbol = symbol,
                    Index = 0,
                    Indicator = new IndicatorProperties("Ema", "GetEma", 34)
                },
                EConditionOperator.LowerThan,
                new ConditionItem()
                {
                    Name = "Close",
                    Interval = interval,
                    Symbol = symbol,
                    Index = 0,
                },
                EConditionOperator.And
            );

            subCondition.AddCondition(conditionNode);

            var conditionNode2 = new ConditionNode(new List<ConditionItem>(), new List<EConditionOperator>());
            conditionNode2.AddCondition(new ConditionItem()
            {
                Name = "EMA_8",
                Interval = interval,
                Symbol = symbol,
                Index = 1,
                Indicator = new IndicatorProperties("Ema", "GetEma", 8)
            }, EConditionOperator.LowerThan, new ConditionItem()
            {
                Name = "EMA_21",
                Interval = interval,
                Symbol = symbol,
                Index = 1,
                Indicator = new IndicatorProperties("Ema", "GetEma", 21)
            }, null);

            conditionNode2.AddCondition(
                new ConditionItem()
                {
                    Name = "EMA_21",
                    Interval = interval,
                    Symbol = symbol,
                    Index = 1,
                    Indicator = new IndicatorProperties("Ema", "GetEma", 21)
                },
                EConditionOperator.LowerThan,
                new ConditionItem()
                {
                    Name = "EMA_34",
                    Interval = interval,
                    Symbol = symbol,
                    Index = 1,
                    Indicator = new IndicatorProperties("Ema", "GetEma", 34)
                },
                EConditionOperator.Or
            );

            conditionNode2.AddCondition(
                new ConditionItem()
                {
                    Name = "EMA_34",
                    Interval = interval,
                    Symbol = symbol,
                    Index = 1,
                    Indicator = new IndicatorProperties("Ema", "GetEma", 34)
                },
                EConditionOperator.GreaterThan,
                new ConditionItem()
                {
                    Name = "Close",
                    Interval = interval,
                    Symbol = symbol,
                    Index = 1,
                },
                EConditionOperator.Or
            );
            subCondition.AddCondition(conditionNode2);
            condition.AddCondition(subCondition);
            return condition;
        }
    }
}
