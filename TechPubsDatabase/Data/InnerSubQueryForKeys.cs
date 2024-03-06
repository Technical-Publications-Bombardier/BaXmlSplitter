using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPubsDatabase.Data
{
    /// <summary>
    /// Allows listing entities with <c>KEY</c> and <c>PARENTOBJECTID</c> for later finding their parents.
    /// </summary>
    public class InnerSubQueryForKeys
    {
        /// <summary>
        /// Gets or sets the parent object identifier.
        /// </summary>
        /// <value>
        /// The parent object identifier.
        /// </value>
        [Required]
        [Column(TypeName = "NUMBER(12)")]
        [MaxLength(12)]
        public long ParentObjectId { get; set; }
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        [Required]
        [Column(TypeName = "VARCHAR2(32)")]
        [MaxLength(32)]
        public string? Key { get; set; }
    }
}
