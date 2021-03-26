namespace API.Models
{
    public class ResponseModel
    {
        public InfoStatus status { get; set; }
        public string text { get; set; } = "unknown";
    }

    public enum InfoStatus
    {
        Info,
        Warning,
        Error
    }
}
