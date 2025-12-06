namespace Shared.Models
{
    public class QuerySet<T> where T : class
    {
        public string? Name { get; set; }
        public T? Parent { get; set; }
        public List<T>? Items { get; set; }
        public int TotalCount { get; set; }
    }
}
