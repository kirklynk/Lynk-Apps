namespace DMS.WebClient.Models
{
    public class ProblemDetails
    {
        public string Title { get; set; } = string.Empty;
        public int? Status { get; set; }
        public string Detail { get; set; } = string.Empty;
    }
}
