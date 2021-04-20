using Newtonsoft.Json;
using Skender.Stock.Indicators;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Utils.Candles.Models;
using Utils.Indicators.Enums;

namespace Utils.Indicators.Models
{
    public class IndicatorProperties
    {
        public string WantedProperty { get; set; }
        public string MethodName { get; set; }
        public string[] RequiredValueNames { get; set; }
        public object[] Values { get; set; }
        private MethodInfo _info;
        [JsonConstructor]
        private IndicatorProperties() { }
        public IndicatorProperties(string wantedProperty, string methodName, params object[] values)
        {
            WantedProperty = wantedProperty;
            MethodName = methodName;
            Values = values;
        }


        public IEnumerable<ResultBase> GenerateIndicators(IEnumerable<Kline> klines, int requiredCandles, EIndicator type)
        {
            //if (_info == null)
            //{
            //    _info = typeof(Indicator).GetMethod(MethodName, BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(new[] { typeof(Kline) }); ;
            //}
            if(klines.Count() < requiredCandles)
            {
                return null;
            }
            //var values = new object[Values.Length + 1];
            //values[0] = klines;
            //for (var i = 1; i < values.Length; i++)
            //{
            //    values[i] = Values[i - 1];
            //}
            //var result = _info.Invoke(null, values);
            return IndicatorGenerator.GenerateIndicator(klines, type, Values);
        }
    }
}
