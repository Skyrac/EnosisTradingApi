using Skender.Stock.Indicators;
using Utils.Candles.Models;
using Utils.Indicators.Enums;

namespace Utils.Indicators
{
    public static class IndicatorValuePicker
    {
        public static decimal? GetValue(EIndicator indicator, Kline candle, string name, string parameter)
        {
            switch (indicator)
            {
                case EIndicator.Volume: return candle.Volume;
                case EIndicator.Open: return candle.Open;
                case EIndicator.High: return candle.High;
                case EIndicator.Low: return candle.Low;
                case EIndicator.Close: return candle.Close;
                case EIndicator.Ema:
                    return GetEmaParameter(candle.GetIndicator<EmaResult>(name), parameter);

                case EIndicator.Adx:
                    return GetAdxParameter(candle.GetIndicator<AdxResult>(name), parameter);

                case EIndicator.Atr:
                    return GetAtrParameter(candle.GetIndicator<AtrResult>(name), parameter);
                case EIndicator.VolSma:
                    return GetVolSmaParameter(candle.GetIndicator<VolSmaResult>(name), parameter);
            }
            return -1;
        }

        private static decimal? GetVolSmaParameter(VolSmaResult result, string parameter)
        {
            switch (parameter)
            {
                case "VolSma": return result?.VolSma;
                case "Volume": return result?.Volume;
            }
            return null;
        }

        private static decimal? GetAdxParameter(AdxResult result, string parameter)
        {
            switch (parameter)
            {
                case "Adx": return result?.Adx;
                case "Mdi": return result?.Mdi;
                case "Pdi": return result?.Pdi;
            }
            return null;
        }

        private static decimal? GetEmaParameter(EmaResult result, string parameter)
        {
            switch (parameter)
            {
                case "Ema": return result?.Ema;
            }
            return null;
        }

        private static decimal? GetAtrParameter(AtrResult result, string parameter)
        {
            switch (parameter)
            {
                case "Atr": return result?.Atr;
                case "Atrp": return result?.Atrp;
                case "Tr": return result?.Tr;
            }
            return null;
        }
    }
}
