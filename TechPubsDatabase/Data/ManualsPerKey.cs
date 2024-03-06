using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPubsDatabase.Data
{
    /// <summary>
    /// Enables finding the manuals that use a certain graphic.
    /// </summary>
    public class ManualsPerKey
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Column(TypeName = "VARCHAR2(1999)")]
        [MaxLength(1999)]
        public string? Name { get; set; }
        /// <summary>
        /// Gets or sets the manual.
        /// </summary>
        /// <value>
        /// The manual.
        /// </value>
        [Column(TypeName = "VARCHAR2(1999)")]
        [MaxLength(1999)]
        public string? Manual { get; set; }
        /// <summary>
        /// Gets or sets the document number.
        /// </summary>
        /// <value>
        /// The document number.
        /// </value>
        [Column(TypeName = "VARCHAR2(1999)")]
        [MaxLength(1999)]
        public string? DocNbr { get; set; }
        /// <summary>
        /// Gets or sets the customer.
        /// </summary>
        /// <value>
        /// The customer.
        /// </value>
        [Column(TypeName = "VARCHAR2(1999)")]
        [MaxLength(1999)]
        public string? Cus { get; set; }
        /// <summary>
        /// Gets or sets the TSN.
        /// </summary>
        /// <value>
        /// The TSN.
        /// </value>
        [Column(TypeName = "VARCHAR2(1999)")]
        [MaxLength(1999)]
        public string? Tsn { get; set; }
        /// <summary>
        /// Gets or sets the revision date.
        /// </summary>
        /// <value>
        /// The revision date.
        /// </value>
        [Column(TypeName = "VARCHAR2(1999)")]
        [MaxLength(1999)]
        public DateTime? RevDate { get; set; }
        /// <summary>
        /// Gets or sets the valid time.
        /// </summary>
        /// <value>
        /// The valid time.
        /// </value>
        [Column(TypeName = "DATE")]
        public DateTime ValidTime { get; set; }
        /// <summary>
        /// Gets or sets the object reference.
        /// </summary>
        /// <value>
        /// The object reference.
        /// </value>
        [Column(TypeName = "NUMBER(14)")]
        [MaxLength(14)]
        public long ObjectRef { get; set; }
        /// <summary>
        /// Gets or sets the object identifier.
        /// </summary>
        /// <value>
        /// The object identifier.
        /// </value>
        [Column(TypeName = "NUMBER(12)")]
        [MaxLength(12)]
        public long ObjectId { get; set; }
        /// <summary>
        /// Gets or sets the parent object identifier.
        /// </summary>
        /// <value>
        /// The parent object identifier.
        /// </value>
        [Column(TypeName = "NUMBER(12)")]
        [MaxLength(12)]
        public long? ParentObjectId { get; set; }
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        [Column(TypeName = "VARCHAR2(1999)")]
        [MaxLength(1999)]
        public string? Key { get; set; }
    }
}
