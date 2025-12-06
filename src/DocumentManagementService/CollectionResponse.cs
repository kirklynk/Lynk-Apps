namespace DocumentManagementService
{
    public class CollectionResponse<T> where T : class
    {
        public ICollection<T> Results { get; set; } = [];
        public int TotalRecords { get; set; }
    }
}
