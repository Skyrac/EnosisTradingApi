using Newtonsoft.Json;
using Utils.Messages.Enums;

namespace Utils
{
    public class BaseMessage
    {
        public EMessage Type { get; set; }

        [JsonConstructor]
        private BaseMessage() { }
        public BaseMessage(EMessage type)
        {
            Type = type;
        }
    }
}
