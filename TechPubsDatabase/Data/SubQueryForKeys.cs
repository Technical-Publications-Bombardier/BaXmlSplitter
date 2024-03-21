using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPubsDatabase.Data
{
    /// <summary>
    /// Represents the query for getting parent objects using <c>CONNECT BY PRIOR O.PARENTOBJECTID = O.OBJECTID</c>.
    /// </summary>
    public class SubQueryForKeys
    {
        /// <summary>
        /// Gets or sets the object identifier.
        /// </summary>
        /// <value>
        /// The object identifier.
        /// </value>
        [Required]
        [Column(TypeName = "NUMBER(12)")]
        [MaxLength(12)]
        public long ObjectId { get; init; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        [Required]
        [Column(TypeName = "VARCHAR2(1999)")]
        [MaxLength(1999)]
        public string? Key { get; init; }
    }
}
