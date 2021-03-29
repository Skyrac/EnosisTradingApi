using Messages.Enums;
using Newtonsoft.Json;

namespace Messages
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
