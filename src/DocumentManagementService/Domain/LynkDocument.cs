using System.ComponentModel.DataAnnotations;

namespace DocumentManagementService.Domain
{
    internal record LynkDocument([Required] string Name)
    {
        public Guid Id { get; set; }

        public Guid? FolderId { get; set; }
        public ICollection<Tag> Tags { get; set; } = [];

        public LynkFolder? Folder { get; set; }

        [Required]
        public Guid SubscriptionId { get; set; }

        public DateTime? ModifiedOn { get; set; }
    }
}
