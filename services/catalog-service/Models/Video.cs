using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace catalog_service.Models
{
    [Table("videos")]
    public class Video
    {
        [Key]
        [Column("video_id")]
        public int VideoId { get; set; }

        [Column("channel_id")]
        public int ChannelId { get; set; }

        [Column("video_title")]
        public required string VideoTitle { get; set; }

        [Column("video_description")]
        public string? VideoDescription { get; set; }

        [Column("video_thumbnail_url")]
        public string? VideoThumbnailUrl { get; set; }

        [Column("video_status")]
        public VideoStatus VideoStatus { get; set; }

        [Column("video_size")]
        public long VideoSize { get; set; }

        [Column("video_duration_seconds")]
        public int VideoDurationSeconds { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        [Column("modified_at")]
        public DateTime ModifiedAt { get; set; }
    }
}