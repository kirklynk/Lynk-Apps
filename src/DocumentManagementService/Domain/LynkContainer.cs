using System.ComponentModel.DataAnnotations;

namespace DocumentManagementService.Domain
{
    internal class LynkContainer
    {
        public Guid Id { get; set; }

        [Required]
        public string? Name { get; set; }
        public ICollection<LynkDocument> Documents { get; set; } = new HashSet<LynkDocument>();

        [Required]
        public Guid SubscriptionId { get; set; }

        public Guid? ParentId { get; set; }

        public virtual LynkContainer? Parent { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedOn { get;  set; }

    }
}
