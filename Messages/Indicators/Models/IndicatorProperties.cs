using Skender.Stock.Indicators;
using System.Collections.Generic;
using System.Reflection;
using Utils.Candles.Models;

namespace Utils.Indicators.Models
{
    public class IndicatorProperties
    {
        public string Name { get; set; }
        public string MethodName { get; set; }
        public string[] PropertyNames { get; set; }
        public string[] RequiredValueNames { get; set; }
        public int[] Values { get; set; }
        private MethodInfo _info;
        public IEnumerable<ResultBase> GenerateIndicators(IEnumerable<Kline> klines)
        {
            if (_info == null)
            {
                _info = typeof(Indicator).GetMethod(MethodName, BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(new[] { typeof(Kline) }); ;
            }
            var values = new object[Values.Length + 1];
            values[0] = klines;
            for (var i = 1; i < values.Length; i++)
            {
                values[i] = Values[i - 1];
            }
            var result = _info.Invoke(null, values);
            return (IEnumerable<ResultBase>)result;
        }
    }
}
