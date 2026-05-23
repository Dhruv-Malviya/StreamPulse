using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace user_service.Models
{
    [Table("follows")]
    public class Follow
    {
        [Column("follower_id")]
        public int FollowerId { get; set; }

        [Column("followee_id")]
        public int FolloweeId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}