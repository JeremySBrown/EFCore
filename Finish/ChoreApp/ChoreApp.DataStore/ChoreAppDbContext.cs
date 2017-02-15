using ChoreApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace ChoreApp.DataStore
{
    public class ChoreAppDbContext : DbContext
    {
        private readonly IConfigurationRoot _configurationRoot;

        public ChoreAppDbContext(DbContextOptions<ChoreAppDbContext> options, IConfigurationRoot configurationRoot)
        {
            _configurationRoot = configurationRoot;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Chore> Chores { get; set; }
        public DbSet<CompletedChore> CompletedChores { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(_configurationRoot["Data:ConnectionString"]);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(p => p.Chores)
                .WithOne(p => p.Child)
                .HasForeignKey(k => k.ChildId);

            modelBuilder.Entity<Chore>()
                .Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(500);

            modelBuilder.Entity<CompletedChore>()
                .HasOne(p => p.Child)
                .WithMany()
                .HasForeignKey(k => k.ChildId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CompletedChore>()
                .HasOne(p => p.Chore)
                .WithMany()
                .HasForeignKey(k => k.ChoreId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
