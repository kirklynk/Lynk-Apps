using System.ComponentModel.DataAnnotations;

namespace DocumentManagementService.Domain
{
    internal record LynkDocument([Required] string Name)
    {
        public Guid Id { get; set; }

        public Guid? ContainerId { get; set; }

        public ICollection<LynkTag> Tags { get; set; } = [];

        public virtual LynkContainer? Container { get; set; }

        [Required]
        public Guid UserSubscriptionId { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public DateTime? DeletedOn { get;  set; }

        public bool IsDeleted { get; set; } = false;

        public string Location { get; set; } = string.Empty;

        [Required]
        public string? Type { get;  set; }

        [Required]
        public string? Extension { get;  set; }

        public long? Size { get; set; }
    }
}
