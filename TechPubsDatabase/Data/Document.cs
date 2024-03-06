using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPubsDatabase.Data
{
    /// <summary>
    /// Represents entities in the <c>DOCUMENT</c> tables.
    /// </summary>
    public class Document
    {
        /// <summary>
        /// Gets or sets the document reference.
        /// </summary>
        /// <value>
        /// The document reference.
        /// </value>
        [Required]
        [Column(TypeName = "NUMBER(6)")]
        [MaxLength(6)]
        public int DocumentRef { get; set; }

        /// <summary>
        /// Gets or sets the object path.
        /// </summary>
        /// <value>
        /// The object path.
        /// </value>
        [Required]
        [Column(TypeName = "VARCHAR2(1000)")]
        [MaxLength(1000)]
        public string? ObjectPath { get; set; }

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
    }
}
