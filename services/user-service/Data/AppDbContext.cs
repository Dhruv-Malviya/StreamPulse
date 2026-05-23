using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using user_service.Models;

namespace user_service.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Follow> Follows { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //for users
            //configure datatypes
            modelBuilder.Entity<User>().Property(u => u.UserId).HasColumnType("int"); 
            modelBuilder.Entity<User>().Property(u => u.UserName).HasColumnType("varchar(100)");    
            modelBuilder.Entity<User>().Property(u => u.Email).HasColumnType("varchar(100)");
            modelBuilder.Entity<User>().Property(u => u.PasswordHash).HasColumnType("varchar(100)");
            modelBuilder.Entity<User>().Property(u => u.ProfileUrl).HasColumnType("varchar(2000)");
            modelBuilder.Entity<User>().Property(u => u.AccountStatus).HasColumnType("smallint");
            modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasColumnType("timestamptz");
            modelBuilder.Entity<User>().Property(u => u.ModifiedAt).HasColumnType("timestamptz");

            // Configure required properties
            modelBuilder.Entity<User>().Property(u => u.UserName).IsRequired();
            modelBuilder.Entity<User>().Property(u => u.Email).IsRequired();
            modelBuilder.Entity<User>().Property(u => u.PasswordHash).IsRequired();

            //configure unique property
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            //configur default values
            modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<User>().Property(u => u.ModifiedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<User>().Property(u => u.AccountStatus).HasDefaultValueSql("1");

            //for follows
            //configure datatypes
            modelBuilder.Entity<Follow>().Property(f => f.FollowerId).HasColumnType("int"); 
            modelBuilder.Entity<Follow>().Property(f => f.FolloweeId).HasColumnType("int"); 
            modelBuilder.Entity<Follow>().Property(f => f.CreatedAt).HasColumnType("timestamptz"); 

            //configure composite primary key
            modelBuilder.Entity<Follow>().HasKey(f => new { f.FollowerId, f.FolloweeId });

            //configure indexes
            modelBuilder.Entity<Follow>().HasIndex(f => f.FolloweeId);

            //configure default values
            modelBuilder.Entity<Follow>().Property(f => f.CreatedAt).HasDefaultValueSql("NOW()");

            //configure relationships
            modelBuilder.Entity<Follow>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Follow>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(f => f.FolloweeId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}