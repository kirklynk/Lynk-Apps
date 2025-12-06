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
        public DbSet<LynkFolder> Folders { get; set; }
        public DbSet<Tag> Tags { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LynkDocument>()
                .HasKey(d => d.Id);

            modelBuilder.Entity<LynkFolder>(e =>
            {
                e.HasKey(f => f.Id);
                e.HasMany(f => f.Documents)
                    .WithOne(d => d.Folder)
                    .HasForeignKey(d => d.FolderId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(f => f.Parent);
            });

            modelBuilder.Entity<Tag>(e =>
            {
                e.HasKey(t => t.Id);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
