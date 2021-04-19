using Newtonsoft.Json;

namespace Utils.Tools
{
    public static class Tools
    {
        public static T DeepCopy<T>(T item)
        {
            var json = JsonConvert.SerializeObject(item);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
