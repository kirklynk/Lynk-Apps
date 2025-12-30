using Azure;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace GatewayService.Security
{
    public class SecurityDbContext : IdentityDbContext<User>
    {
        public SecurityDbContext(DbContextOptions<SecurityDbContext> options) : base(options)
        {

        }
        public DbSet<Subscription> Subscriptions { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Subscription>(e =>
            {
                e.HasKey(s => s.Id);
                e.HasMany(s => s.Users)
                 .WithMany(u => u.Subscriptions)
                 .UsingEntity("UserSubscriptions",
                    r => r.HasOne(typeof(User)).WithMany().HasForeignKey("UserId"),
                    l => l.HasOne(typeof(Subscription)).WithMany().HasForeignKey("SubscriptionId"));
            });
            
            base.OnModelCreating(builder);

        }
    }
}
