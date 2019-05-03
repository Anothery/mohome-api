using Microsoft.EntityFrameworkCore;
using Models;


namespace DBRepository
{
    public class MohomeContext : DbContext
    {
        public DbSet<Profile> Profile { get; set; }
        public DbSet<Role> Role { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // UNCOMMENT AND FILL (!)
            //optionsBuilder.UseMySQL("server=localhost;database=db;user=user;password=password");
            

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Profile>(entity =>
            {
                entity.HasOne(e => e.Role);
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Password).IsRequired();
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Email).IsRequired();   
                entity.Property(e => e.RoleId).HasDefaultValue(1);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId);
                entity.Property(e => e.Name).IsRequired();
            });
        }
    }
}
