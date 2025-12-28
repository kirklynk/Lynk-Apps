using DocumentManagementService.Domain;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagementService.Data
{
    internal class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<LynkDocument> Documents { get; set; }
        public DbSet<LynkContainer> Containers { get; set; }
        public DbSet<Tag> Tags { get; set; }

        public DbSet<PendingPurge> PendingPurge { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LynkDocument>(e =>
            {
                e.HasKey(d => d.Id);
                e.HasQueryFilter(x => x.IsDeleted == false);
                e.Property(x => x.Location).IsRequired();
                e.Property(x => x.Type).IsRequired();
                e.Property(x => x.Extension).IsRequired();
            });

            modelBuilder.Entity<LynkContainer>(e =>
            {
                e.HasKey(f => f.Id);
                e.HasMany(f => f.Documents)
                    .WithOne(d => d.Container)
                    .HasForeignKey(d => d.ContainerId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(f => f.Parent);
                e.HasQueryFilter(x => x.IsDeleted == false);
            });

            modelBuilder.Entity<Tag>(e =>
            {
                e.HasKey(t => t.Id);
            });

            modelBuilder.Entity<PendingPurge>(e =>
            {
                e.HasKey(r => r.ReferenceId);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
