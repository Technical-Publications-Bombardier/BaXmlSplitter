using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPubsDatabase.Data
{
    /// <summary>
    /// Represents the <c>OBJECTATTRIBUTE</c> tables.
    /// </summary>
    public class ObjectAttribute
    {
        /// <summary>
        /// Gets or sets the object reference.
        /// </summary>
        /// <value>
        /// The object reference.
        /// </value>
        [Required]
        [Column(TypeName = "NUMBER(14)")]
        [MaxLength(14)]
        public long ObjectRef { get; set; }

        /// <summary>
        /// Gets or sets the name of the attribute.
        /// </summary>
        /// <value>
        /// The name of the attribute.
        /// </value>
        [Required]
        [Column(TypeName = "VARCHAR2(255)")]
        [MaxLength(255)]
        public string? AttributeName { get; set; }

        /// <summary>
        /// Gets or sets the attribute value.
        /// </summary>
        /// <value>
        /// The attribute value.
        /// </value>
        [Column(TypeName = "VARCHAR2(1999)")]
        [MaxLength(1999)]
        public string? AttributeValue { get; set; }
        // Navigation property
        /// <summary>
        /// Gets or sets the object new.
        /// </summary>
        /// <value>
        /// The object new.
        /// </value>
        public ObjectNew? ObjectNew { get; set; }
    }
}