using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Entities
{
    /**
     * It's actually not a really good example as it is known that different DB engines
     * may have different setups when it comes to column properties but as for this quick
     * example with minimal column with just long, string, bool, and DateTime; I am sure that
     * EF should be able to translate most of it into the respective DB engine of their choices.
     */

    [Table("user")]
    public class User
    {
        [Key, Column("id")] public long Id { get; set; }

        [Required, MaxLength(255), Column("username")]
        public string Username { get; set; }

        [Required, MaxLength(255), Column("password_hash")]
        public string PasswordHash { get; set; }

        [Column("is_active")] public bool IsActive { get; set; } = true;
        [Column("is_deleted")] public bool IsDeleted { get; set; } = false;

        [Column("created_at")] public DateTime CreatedAt { get; set; }
        [Column("creator_id")] public long CreatorId { get; set; }

        [Column("updated_at")] public DateTime? UpdatedAt { get; set; } = null;
        [Column("updater_id")] public long? UpdaterId { get; set; } = null;
    }
}
