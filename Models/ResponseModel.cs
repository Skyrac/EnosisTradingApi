namespace API.Models
{
    public class ResponseModel
    {
        public Status status { get; set; }
    }

    public enum Status
    {
        Success,
        Failed,
        Denied
    }
}
