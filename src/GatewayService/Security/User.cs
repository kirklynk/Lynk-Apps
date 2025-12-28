using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GatewayService.Security
{
    public class User : IdentityUser
    {
        public ICollection<Subscription> Subscriptions { get; set; } = new HashSet<Subscription>();
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get => $"{FirstName} {LastName}".Trim(); }
    }
}
