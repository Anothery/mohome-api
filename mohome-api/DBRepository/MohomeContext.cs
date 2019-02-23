using Microsoft.EntityFrameworkCore;
using Models;


namespace DBRepository
{
    public class MohomeContext : DbContext
    {
        public DbSet<Profile> Profile { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("server=localhost;database=db;user=user;password=password");
           

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Profile>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Password).IsRequired();
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Email).IsRequired();   
            });
        }
    }
}
