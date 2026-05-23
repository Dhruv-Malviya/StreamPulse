using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace user_service.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public int  UserId  {get; set; }

        [Column("username")]
        public required string UserName    {get; set; }

        [Column("email")]
        public required string Email    {get; set; }

        [Column("password_hash")]
        public required string PasswordHash {get; set; }

        [Column("profile_url")]
        public string? ProfileUrl  {get; set; }

        [Column("account_status")]
        public AccountStatus  AccountStatus    {get; set; }

        [Column("created_at")]
        public DateTime CreatedAt   {get; set; }
        
        [Column("modified_at")]
        public DateTime ModifiedAt { get; set; }
    }

}

