using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using catalog_service.Models;

namespace catalog_service.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Channel> Channels { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<VideoAsset> VideoAssets { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //for channels
            //configure datatypes
            modelBuilder.Entity<Channel>().Property(c => c.ChannelId).HasColumnType("int");
            modelBuilder.Entity<Channel>().Property(c => c.UserId).HasColumnType("int");
            modelBuilder.Entity<Channel>().Property(c => c.ChannelName).HasColumnType("varchar(100)");
            modelBuilder.Entity<Channel>().Property(c => c.ChannelDescription).HasColumnType("varchar(2000)");
            modelBuilder.Entity<Channel>().Property(c => c.ChannelProfileUrl).HasColumnType("varchar(2000)");
            modelBuilder.Entity<Channel>().Property(c => c.ChannelStatus).HasColumnType("smallint");
            modelBuilder.Entity<Channel>().Property(c => c.CreatedAt).HasColumnType("timestamptz");
            modelBuilder.Entity<Channel>().Property(c => c.ModifiedAt).HasColumnType("timestamptz");

            // Configure required properties
            modelBuilder.Entity<Channel>().Property(c => c.ChannelName).IsRequired();
            modelBuilder.Entity<Channel>().Property(c => c.ChannelStatus).IsRequired();
            
            //configure default values
            modelBuilder.Entity<Channel>().Property(c => c.ChannelStatus).HasDefaultValueSql("1");
            modelBuilder.Entity<Channel>().Property(c => c.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Channel>().Property(c => c.ModifiedAt).HasDefaultValueSql("NOW()");

            //configure unique constraints
            modelBuilder.Entity<Channel>().HasIndex(c => c.UserId).IsUnique();
            modelBuilder.Entity<Channel>().HasIndex(c => c.ChannelName).IsUnique();

            //configure contraints
            modelBuilder.Entity<Channel>(c => {c.ToTable(t => t.HasCheckConstraint("CK_Channel_ChannelStatus", "channel_status >= 0 AND channel_status <= 3"));});

            //for videos
            //configure datatypes
            modelBuilder.Entity<Video>().Property(v => v.VideoId).HasColumnType("int");
            modelBuilder.Entity<Video>().Property(v => v.ChannelId).HasColumnType("int");
            modelBuilder.Entity<Video>().Property(v => v.VideoTitle).HasColumnType("varchar(100)");
            modelBuilder.Entity<Video>().Property(v => v.VideoDescription).HasColumnType("varchar(2000)");
            modelBuilder.Entity<Video>().Property(v => v.VideoThumbnailUrl).HasColumnType("varchar(2000)");
            modelBuilder.Entity<Video>().Property(v => v.VideoStatus).HasColumnType("smallint");
            modelBuilder.Entity<Video>().Property(v => v.VideoSize).HasColumnType("bigint");
            modelBuilder.Entity<Video>().Property(v => v.VideoDurationSeconds).HasColumnType("int");
            modelBuilder.Entity<Video>().Property(v => v.CreatedAt).HasColumnType("timestamptz");
            modelBuilder.Entity<Video>().Property(v => v.ModifiedAt).HasColumnType("timestamptz");

            // Configure required properties
            modelBuilder.Entity<Video>().Property(v => v.VideoTitle).IsRequired();
            modelBuilder.Entity<Video>().Property(v => v.VideoStatus).IsRequired();

            //configure default values
            modelBuilder.Entity<Video>().Property(v => v.VideoStatus).HasDefaultValueSql("0");
            modelBuilder.Entity<Video>().Property(v => v.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Video>().Property(v => v.ModifiedAt).HasDefaultValueSql("NOW()");

            //configure index
            modelBuilder.Entity<Video>().HasIndex(v => v.ChannelId);

            //configure constraints
            modelBuilder.Entity<Video>(c => {c.ToTable(t => t.HasCheckConstraint("CK_Video_VideoStatus", "video_status >= 0 AND video_status <= 4"));});

            //configure foreign key relationships
            modelBuilder.Entity<Video>()
                .HasOne<Channel>()
                .WithMany()
                .HasForeignKey(v => v.ChannelId)
                .OnDelete(DeleteBehavior.Cascade);

            //for video assets
            //configure datatypes
            modelBuilder.Entity<VideoAsset>().Property(va => va.VideoAssetId).HasColumnType("int");
            modelBuilder.Entity<VideoAsset>().Property(va => va.VideoId).HasColumnType("int");
            modelBuilder.Entity<VideoAsset>().Property(va => va.VideoQuality).HasColumnType("int");
            modelBuilder.Entity<VideoAsset>().Property(va => va.ManifestUrl).HasColumnType("varchar(2000)");
            modelBuilder.Entity<VideoAsset>().Property(va => va.VideoAssetSize).HasColumnType("bigint");
            modelBuilder.Entity<VideoAsset>().Property(va => va.VideoAssetBitrate).HasColumnType("int");
            modelBuilder.Entity<VideoAsset>().Property(va => va.VideoAssetStatus).HasColumnType("smallint");
            modelBuilder.Entity<VideoAsset>().Property(va => va.CreatedAt).HasColumnType("timestamptz");

            //configure required properties
            modelBuilder.Entity<VideoAsset>().Property(va => va.ManifestUrl).IsRequired();

            //configure default values
            modelBuilder.Entity<VideoAsset>().Property(va => va.VideoAssetStatus).HasDefaultValueSql("0");
            modelBuilder.Entity<VideoAsset>().Property(va => va.CreatedAt).HasDefaultValueSql("NOW()");

            //configure index
            modelBuilder.Entity<VideoAsset>().HasIndex(va => va.VideoId);

            //configure constraints
            modelBuilder.Entity<VideoAsset>(c => {c.ToTable(t => t.HasCheckConstraint("CK_VideoAsset_VideoAssetStatus", "video_asset_status >= 0 AND video_asset_status <= 3"));});

            //configure foreign key relationships
            modelBuilder.Entity<VideoAsset>()
                .HasOne<Video>()
                .WithMany()
                .HasForeignKey(va => va.VideoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}