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
        public DbSet<Playlist> Playlist { get; set; }
        public DbSet<Music> Music { get; set; }
        public DbSet<RefreshToken> RefreshToken { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("server=localhost;database=mohome;user=root;password=secretpassword");
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
                      .HasForeignKey(e => e.AlbumId);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Token).IsRequired();
                entity.Property(e => e.CreationDate).IsRequired();
                entity.Property(e => e.ExpirationDate).IsRequired();

                entity.HasOne(e => e.User)
                      .WithMany(e => e.Tokens)
                      .HasForeignKey(e => e.UserId);

                entity.Property(e => e.CreationDate).HasDefaultValue(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            });

            modelBuilder.Entity<Music>(entity =>
            {
                entity.HasOne(e => e.Profile);
                entity.HasOne(e => e.Playlist)
                      .WithMany(e => e.Music)
                      .HasForeignKey(e => e.PlaylistId);
                entity.HasKey(e => e.MusicId);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Created).HasDefaultValue(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                entity.Property(e => e.PlaylistId).HasDefaultValue(null);
                entity.Property(e => e.MusicPath).IsRequired();
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Author).IsRequired();
            });

            modelBuilder.Entity<Playlist>(entity =>
            {
                entity.HasKey(e => e.PlaylistId);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Created).HasDefaultValue(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                entity.Property(e => e.CoverPhotoPath).HasDefaultValue(null);
                entity.Property(e => e.Name).IsRequired();

                entity.HasOne(e => e.Profile);
                entity.HasMany(e => e.Music)
                      .WithOne(e => e.Playlist)
                      .HasForeignKey(e => e.PlaylistId);
            });
        }

    }
}
