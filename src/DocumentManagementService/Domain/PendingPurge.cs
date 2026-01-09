using Shared.Common.Enums;

namespace DocumentManagementService.Domain
{
    public class PendingPurge
    {
        public Guid ReferenceId { get; set; }
        public Guid SubscriptionId { get; set; }
        public EntityType EntityType { get; set; } 
    }
}
