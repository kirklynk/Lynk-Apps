namespace GatewayService.Security
{
    public class Subscription
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }

       // public ICollection<string> AllowedServices { get; set; } = new HashSet<string>();
        public ICollection<User> Users { get; set; } = new HashSet<User>();
    }
}
