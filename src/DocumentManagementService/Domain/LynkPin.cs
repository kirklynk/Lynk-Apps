using Shared.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace DocumentManagementService.Domain
{
    public class LynkPin
    {
        [Required]
        public Guid ReferenceId { get; set; }
        public EntityType Entity { get; set; }
        [Required]
        public Guid UserSubscriptionId { get;  set; }
    }
}
