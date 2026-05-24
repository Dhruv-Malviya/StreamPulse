using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace catalog_service.Models
{
    [Table("video_assets")]
    public class VideoAsset
    {

        [Key]
        [Column("video_asset_id")]
        public int  VideoAssetId  {get; set; }

        [Column("video_id")]
        public int VideoId { get; set; }

        [Column("video_quality")]
        public int VideoQuality { get; set; }

        [Column("manifest_url")]
        public required string ManifestUrl { get; set; }

        [Column("video_asset_size")]
        public long VideoAssetSize { get; set; }

        [Column("video_asset_bitrate")]
        public int VideoAssetBitrate { get; set; }

        [Column("video_asset_status")]
        public VideoAssetStatus VideoAssetStatus { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now; 

    }

}

