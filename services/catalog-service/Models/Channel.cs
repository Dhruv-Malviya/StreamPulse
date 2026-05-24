using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace catalog_service.Models
{
    [Table("channels")]
    public class Channel
    {
        [Key]
        [Column("channel_id")]
        public int  ChannelId  {get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("channel_name")]
        public required string ChannelName { get; set; }

        [Column("channel_description")]
        public string? ChannelDescription {get; set; }

        [Column("channel_profile_url")]
        public string? ChannelProfileUrl  {get; set; }

        [Column("channel_status")]
        public ChannelStatus ChannelStatus  {get; set; }

        [Column("created_at")]
        public DateTime CreatedAt   {get; set; }
        
        [Column("modified_at")]
        public DateTime ModifiedAt { get; set; }
    }
}