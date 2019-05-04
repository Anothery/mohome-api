using Microsoft.EntityFrameworkCore;
using Models;


namespace DBRepository
{
    public class MohomeContext : DbContext
    {
        public DbSet<Profile> Profile { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<PhotoAlbum> PhotoAlbum { get; set; }
        public DbSet<Photo> Photo { get; set; }


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

            modelBuilder.Entity<Photo>(entity =>
            {
                entity.HasOne(e => e.Profile);
                entity.HasOne(e => e.Album)
                      .WithMany(e => e.Photos)
                      .HasForeignKey(e => e.AlbumId);
                entity.HasKey(e => e.PhotoId);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Created).HasDefaultValue(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                entity.Property(e => e.Path).IsRequired();
            });
            modelBuilder.Entity<PhotoAlbum>(entity =>
            {    
                entity.HasKey(e => e.AlbumId);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Created).HasDefaultValue(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                entity.Property(e => e.CoverPhotoId).HasDefaultValue(null);
                entity.Property(e => e.Name).IsRequired();

                entity.HasOne(e => e.Profile);
                entity.HasOne(e => e.CoverPhoto);
                entity.HasMany(e => e.Photos)
                      .WithOne(e => e.Album)
                      .HasForeignKey(e => e.AlbumId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
