namespace Shared.Models
{
    public class LynkFileInfo
    {
        public Guid Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public bool IsFolder { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public virtual LynkFileInfo? Parent { get; set; }
    }
}
