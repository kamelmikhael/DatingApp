using Microsoft.EntityFrameworkCore;
using DatingApp.API.Entities;

namespace DatingApp.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserLike>()
                .HasKey(x => new {x.SourceUserId, x.LikedUserId});

            modelBuilder.Entity<UserLike>()
                .HasOne(x => x.SourceUser)
                .WithMany(x => x.LikedUsers)
                .HasForeignKey(x => x.SourceUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserLike>()
                .HasOne(x => x.LikedUser)
                .WithMany(x => x.LikedByUsers)
                .HasForeignKey(x => x.LikedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(x => x.Recipient)
                .WithMany(x => x.MessagesReceived)
                .HasForeignKey(x => x.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Message>()
                .HasOne(x => x.Sender)
                .WithMany(x => x.MessagesSend)
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}