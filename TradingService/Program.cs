﻿using Binance.Net.Enums;
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

                strategy.AddConditionSequence(Utils.Trading.Enums.ESide.Long, coreSymbol, GenerateLongCondition(coreSymbol, KlineInterval.OneMinute));
            }
            trader.AddStrategy(new Strategy()
            {
                RequiredCandles = 400,
                Name = "EmaCross",
                BalancePerTrade = 10,
                EntryStrategy = strategy
            });
            Console.ReadKey();
        }

        private static ConditionSequence GenerateStopLoss(ESide side, string symbol, KlineInterval interval)
        {
            var condition = new ConditionSequence();
            var subCondition = new ConditionSequence();
            condition.AddCondition(subCondition);
            subCondition.AddCondition(new ConditionNode(
                new List<ConditionItem>() {
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
                        Name = "ATR_8",
                        Interval = interval,
                        Symbol = symbol,
                        Index = 0,
                        Indicator = new IndicatorProperties("Atr", "GetAtr", 8)
                    }
                }, new List<EConditionOperator>()
                {
                    EConditionOperator.GreaterThan
                }
            ));
            return condition;
        }

        private static ConditionSequence GenerateLongCondition(string symbol, KlineInterval interval)
        {
            var condition = new ConditionSequence();
            var subCondition = new ConditionSequence();
            var conditionNode = new ConditionNode(new List<ConditionItem>(), new List<EConditionOperator>());
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
            }, null);

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
            condition.AddCondition(subCondition);
            return condition;
        }
    }
}
