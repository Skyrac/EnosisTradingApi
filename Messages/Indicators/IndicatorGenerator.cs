using Skender.Stock.Indicators;
using System.Collections.Generic;
using Utils.Candles.Models;
using Utils.Indicators.Enums;

namespace Utils.Indicators
{
    public static class IndicatorGenerator
    {
        public static IEnumerable<ResultBase> GenerateIndicator(IEnumerable<Kline> klines, EIndicator indicator, object[] values)
        {
            switch (indicator)
            {
                case EIndicator.Ema:
                    return Indicator.GetEma(klines, (int)values[0]);
                case EIndicator.Adx:
                    return Indicator.GetAdx(klines, (int)values[0]);
                case EIndicator.Atr:
                    return Indicator.GetAtr(klines, (int)values[0]);
                case EIndicator.VolSma:
                    return Indicator.GetVolSma(klines, (int)values[0]);
                default:
                    return null;
            }
        }
    }
}
