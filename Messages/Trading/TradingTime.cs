using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Utils.Trading
{
    public class TradingTime
    {
        private const string Path = "TradingTime.json";
        public Dictionary<DayOfWeek, List<int>> tradingActive = new Dictionary<DayOfWeek, List<int>>();
        public TradingTime(bool load = false)
        {
            if (load && File.Exists(Path))
            {
                tradingActive = JsonConvert.DeserializeObject<Dictionary<DayOfWeek, List<int>>>(File.ReadAllText(Path));
            }
        }

        public TradingTime(params DayOfWeek[] days)
        {
            foreach (var day in days)
            {
                Add(day);
            }
        }

        public void Write()
        {
            File.WriteAllText(Path, JsonConvert.SerializeObject(tradingActive));
        }

        public void Add(DayOfWeek day, bool addAll = true, params int[] hours)
        {
            if (!tradingActive.ContainsKey(day))
            {
                tradingActive.Add(day, hours.ToList());
            }
            else
            {
                tradingActive[day].AddRange(hours);
            }
            if (hours.Length == 0 && addAll && tradingActive[day].Count == 0)
            {
                for (var i = 0; i < 24; i++)
                {
                    tradingActive[day].Add(i);
                }
            }
        }

        public string Print()
        {
            var text = "Trading Times:\n";
            foreach (var pair in tradingActive)
            {
                text += pair.Key.ToString() + " - ";
                foreach (var hour in pair.Value)
                {
                    text += hour.ToString() + ", ";
                }
                text = text.Remove(text.Length - 2);
                text += "\n";
            }
            Console.WriteLine(text);
            return text;
        }

        public bool CanTrade(DayOfWeek day, int hour)
        {
            return tradingActive.ContainsKey(day) && tradingActive[day].Contains(hour);
        }
    }
}
