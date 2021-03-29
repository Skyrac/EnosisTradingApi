using Utils.Messages.Enums;
using Newtonsoft.Json;

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
