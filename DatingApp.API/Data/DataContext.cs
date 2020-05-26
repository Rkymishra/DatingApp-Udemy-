using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data {
    public class DataContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, 
                                    UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>> 
    {
        public DataContext (DbContextOptions<DataContext> options) : base (options) { }
        public DbSet<Value> Values { get; set; }
        // public DbSet<User> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }
        protected override void OnModelCreating (ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRole>(userRole=>{
                userRole.HasKey(ur=>new{ur.UserId, ur.RoleId});
                userRole.HasOne(x=>x.Role).WithMany(x=>x.UserRoles).HasForeignKey(x=>x.RoleId).IsRequired();
                userRole.HasOne(x=>x.User).WithMany(x=>x.UserRoles).HasForeignKey(x=>x.UserId).IsRequired();
            });

            modelBuilder.Entity<Like> ().HasKey (a => new { a.LikerId, a.LikeeId });
            modelBuilder.Entity<Like> ().HasOne (u => u.Likee).WithMany (u => u.Likers).HasForeignKey(u=>u.LikeeId).OnDelete (DeleteBehavior.Restrict);
            modelBuilder.Entity<Like> ().HasOne (u => u.Liker).WithMany (u => u.Likees).HasForeignKey(u=>u.LikerId).OnDelete (DeleteBehavior.Restrict);

            modelBuilder.Entity<Message> ().HasOne (a => a.Sender).WithMany(m=>m.MessagesSent).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Message> ().HasOne (a => a.Recipient).WithMany(m=>m.MessagesReceived).OnDelete(DeleteBehavior.Restrict);
        }
    }
}