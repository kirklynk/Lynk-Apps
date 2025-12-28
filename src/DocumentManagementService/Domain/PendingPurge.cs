namespace DocumentManagementService.Domain
{
    public class PendingPurge
    {
        public Guid ReferenceId { get; set; }
        public Guid SubscriptionId { get; set; }
        public PendingPurgeEntityType EntityType { get; set; } = PendingPurgeEntityType.Unknown;
    }

    public enum PendingPurgeEntityType
    {
        Unknown = 0,
        Document = 1,
        Container = 2
    }
}
