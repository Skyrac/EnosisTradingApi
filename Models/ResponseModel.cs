namespace API.Models
{
    public class ResponseModel
    {
        public InfoStatus status { get; set; }
        public string text { get; set; }
    }

    public enum InfoStatus
    {
        Info,
        Warning,
        Error
    }
}
