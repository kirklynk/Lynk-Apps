using Shared.Common.Enums;

namespace DocumentManagementService.Domain
{
    public class LynkPin
    {
        public Guid ReferenceId { get; set; }
        public EntityType Entity { get; set; }
        public Guid UserId { get; set; }
        public Guid SubscriptionId { get; internal set; }
    }
}
