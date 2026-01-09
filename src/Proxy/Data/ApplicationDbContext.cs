using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Proxy.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User>(options)
    {
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
