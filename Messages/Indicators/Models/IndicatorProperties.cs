using Newtonsoft.Json;
using Skender.Stock.Indicators;
using System.Collections.Generic;
using System.Reflection;
using Utils.Candles.Models;

namespace Utils.Indicators.Models
{
    public class IndicatorProperties
    {
        public string WantedProperty { get; set; }
        public string MethodName { get; set; }
        public string[] PropertyNames { get; set; }
        public string[] RequiredValueNames { get; set; }
        public int[] Values { get; set; }
        private MethodInfo _info;
        [JsonConstructor]
        private IndicatorProperties() { }
        public IndicatorProperties(string wantedProperty, string methodName, params int[] values)
        {
            WantedProperty = wantedProperty;
            MethodName = methodName;
            Values = values;
        }
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
