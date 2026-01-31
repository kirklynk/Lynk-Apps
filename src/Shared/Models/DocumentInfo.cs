using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models
{
    public class DocumentInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        [NotMapped]
        public bool IsContainer { get; set; }
        
        public DateTime? ModifiedOn { get; set; }
        [NotMapped]
        public DateTime? DeletedOn { get; set; }
        [NotMapped]
        public virtual DocumentInfo? Parent { get; set; }
        [NotMapped]
        public string? Type { get; set; }
        [NotMapped]
        public bool? IsPinned { get; set; }
        [NotMapped]
        public long Size { get; set; } = 0L;
        [NotMapped]
        public string? Extension { get; set; }
    }
}
