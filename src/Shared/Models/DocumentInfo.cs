namespace Shared.Models
{
    public class DocumentInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsContainer { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime? DeletedOn { get; set; }
        public virtual DocumentInfo? Parent { get; set; }
        public string? Type { get; set; }
        public bool? IsPinned { get; set; }
        public long Size { get; set; } = 0L;
        public string? Extension { get; set; }
    }
}
