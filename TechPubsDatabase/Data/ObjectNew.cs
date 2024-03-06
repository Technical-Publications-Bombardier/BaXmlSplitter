using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPubsDatabase.Data
{
    /// <summary>
    /// Represents entities in the <c>OBJECTNEW</c> tables.
    /// </summary>
    public class ObjectNew
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
        /// Gets or sets the object identifier.
        /// </summary>
        /// <value>
        /// The object identifier.
        /// </value>
        [Required]
        [Column(TypeName = "NUMBER(12)")]
        [MaxLength(12)]
        public long ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the creation time.
        /// </summary>
        /// <value>
        /// The creation time.
        /// </value>
        [Required]
        [Column(TypeName = "DATE")]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// Gets or sets the source object identifier.
        /// </summary>
        /// <value>
        /// The source object identifier.
        /// </value>
        [Column(TypeName = "NUMBER(12)")]
        [MaxLength(12)]
        public long? SourceObjectId { get; set; }

        /// <summary>
        /// Gets or sets the source create time.
        /// </summary>
        /// <value>
        /// The source create time.
        /// </value>
        [Column(TypeName = "DATE")]
        public DateTime? SourceCreateTime { get; set; }

        /// <summary>
        /// Gets or sets the source update count.
        /// </summary>
        /// <value>
        /// The source update count.
        /// </value>
        [Column(TypeName = "NUMBER(4)")]
        [MaxLength(4)]
        public short? SourceUpdateCount { get; set; }

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
        /// Gets or sets the object sequence number.
        /// </summary>
        /// <value>
        /// The object sequence number.
        /// </value>
        [Column(TypeName = "NUMBER(8)")]
        [MaxLength(8)]
        public int? ObjectSeqNo { get; set; }

        /// <summary>
        /// Gets or sets the approval path reference.
        /// </summary>
        /// <value>
        /// The approval path reference.
        /// </value>
        [Column(TypeName = "NUMBER(6)")]
        [MaxLength(6)]
        public int? ApprovalPathRef { get; set; }

        /// <summary>
        /// Gets or sets the finite state machine reference.
        /// </summary>
        /// <value>
        /// The finite state machine reference.
        /// </value>
        [Column(TypeName = "NUMBER(6)")]
        [MaxLength(6)]
        public int? FiniteStateMachineRef { get; set; }

        /// <summary>
        /// Gets or sets the state of the current.
        /// </summary>
        /// <value>
        /// The state of the current.
        /// </value>
        [Column(TypeName = "NUMBER(4)")]
        [MaxLength(4)]
        public short? CurrentState { get; set; }

        /// <summary>
        /// Gets or sets the user reference.
        /// </summary>
        /// <value>
        /// The user reference.
        /// </value>
        [Required]
        [Column(TypeName = "NUMBER(6)")]
        [MaxLength(6)]
        public int UserRef { get; set; }

        /// <summary>
        /// Gets or sets the modify time.
        /// </summary>
        /// <value>
        /// The modify time.
        /// </value>
        [Required]
        [Column(TypeName = "DATE")]
        public DateTime ModifyTime { get; set; }

        /// <summary>
        /// Gets or sets the valid time.
        /// </summary>
        /// <value>
        /// The valid time.
        /// </value>
        [Required]
        [Column(TypeName = "DATE")]
        public DateTime ValidTime { get; set; }

        /// <summary>
        /// Gets or sets the obsolete time.
        /// </summary>
        /// <value>
        /// The obsolete time.
        /// </value>
        [Required]
        [Column(TypeName = "DATE")]
        public DateTime ObsoleteTime { get; set; }

        /// <summary>
        /// Gets or sets the offline time.
        /// </summary>
        /// <value>
        /// The offline time.
        /// </value>
        [Required]
        [Column(TypeName = "DATE")]
        public DateTime OfflineTime { get; set; }

        /// <summary>
        /// Gets or sets the type of the link.
        /// </summary>
        /// <value>
        /// The type of the link.
        /// </value>
        [Column(TypeName = "NUMBER(1)")]
        [MaxLength(1)]
        public bool? LinkType { get; set; }

        /// <summary>
        /// Gets or sets the link time.
        /// </summary>
        /// <value>
        /// The link time.
        /// </value>
        [Column(TypeName = "DATE")]
        public DateTime? LinkTime { get; set; }

        /// <summary>
        /// Gets or sets the link context.
        /// </summary>
        /// <value>
        /// The link context.
        /// </value>
        [Column(TypeName = "VARCHAR2(1024)")]
        [MaxLength(1024)]
        public string? LinkContext { get; set; }

        /// <summary>
        /// Gets or sets the link object.
        /// </summary>
        /// <value>
        /// The link object.
        /// </value>
        [Column(TypeName = "NUMBER(14)")]
        [MaxLength(14)]
        public long? LinkObject { get; set; }

        /// <summary>
        /// Gets or sets the name of the object.
        /// </summary>
        /// <value>
        /// The name of the object.
        /// </value>
        [Column(TypeName = "VARCHAR2(255)")]
        [MaxLength(255)]
        public string? ObjectName { get; set; }

        /// <summary>
        /// Gets or sets the object key.
        /// </summary>
        /// <value>
        /// The object key.
        /// </value>
        [Column(TypeName = "VARCHAR2(255)")]
        [MaxLength(255)]
        public string? ObjectKey { get; set; }

        /// <summary>
        /// Gets or sets the revision symbol.
        /// </summary>
        /// <value>
        /// The revision symbol.
        /// </value>
        [Column(TypeName = "CHAR(1)")]
        [MaxLength(1)]
        public char? RevisionSymbol { get; set; }

        /// <summary>
        /// Gets or sets the type of the object.
        /// </summary>
        /// <value>
        /// The type of the object.
        /// </value>
        [Required]
        [Column(TypeName = "NUMBER(1)")]
        [MaxLength(1)]
        public byte ObjectType { get; set; }

        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
        [Column(TypeName = "NUMBER(3)")]
        [MaxLength(3)]
        public short? DataType { get; set; }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        [Column(TypeName = "NUMBER(10)")]
        [MaxLength(10)]
        public int? Length { get; set; }

        /// <summary>
        /// Gets or sets the checksum.
        /// </summary>
        /// <value>
        /// The checksum.
        /// </value>
        [Column(TypeName = "NUMBER(5)")]
        [MaxLength(5)]
        public int? Checksum { get; set; }

        /// <summary>
        /// Gets or sets the flags.
        /// </summary>
        /// <value>
        /// The flags.
        /// </value>
        [Column(TypeName = "NUMBER(4)")]
        [MaxLength(4)]
        public short? Flags { get; set; }
        // Navigation property
        /// <summary>
        /// Gets or sets the object attributes.
        /// </summary>
        /// <value>
        /// The object attributes.
        /// </value>
        public List<ObjectAttribute>? ObjectAttributes { get; set; }
    }
}
