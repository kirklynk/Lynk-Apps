using System.ComponentModel.DataAnnotations;

namespace DocumentManagementService.Domain
{
    internal class LynkFolder
    {
        public Guid Id { get; set; }

        [Required]
        public string? Name { get; set; }
        public ICollection<LynkDocument> Documents { get; set; } = new HashSet<LynkDocument>();

        [Required]
        public Guid SubscriptionId { get; set; }

        public Guid? ParentId { get; set; }

        public virtual LynkFolder? Parent { get; set; }

        public DateTime? ModifiedOn { get; set; }

    }
}
