using System.ComponentModel.DataAnnotations;

namespace DocumentManagementService.Domain
{
    internal class LynkContainer
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
      
        [Required]
        public Guid UserSubscriptionId { get; set; }

        public Guid? ParentId { get; set; }


        public virtual LynkContainer? Parent { get; set; }

       
        public DateTime? ModifiedOn { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedOn { get;  set; }

        public virtual ICollection<LynkShare> Shares { get; set; } = new HashSet<LynkShare>();

        public virtual ICollection<LynkContainer> Children { get; set; } = new HashSet<LynkContainer>();

        public virtual ICollection<LynkDocument> Documents { get; set; } = new HashSet<LynkDocument>();
    }
}
