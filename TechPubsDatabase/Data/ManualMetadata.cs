using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPubsDatabase.Data;

public class ManualMetadata
{
    /// <summary>
    ///     Gets or sets the NAME.
    /// </summary>
    /// <value>
    ///     The NAME.
    /// </value>
    [Required]
    [Column(TypeName = "VARCHAR2(255)")]
    [MaxLength(255)]
    public string Name { get; set; }

    /// <summary>
    ///     Gets or sets the MANUAL.
    /// </summary>
    /// <value>
    ///     The MANUAL.
    /// </value>
    [Required]
    [Column(TypeName = "VARCHAR2(1999)")]
    [MaxLength(1999)]
    public string Manual { get; set; }

    /// <summary>
    ///     Gets or sets the DOCNBR.
    /// </summary>
    /// <value>
    ///     The DOCNBR.
    /// </value>
    [Required]
    [Column(TypeName = "VARCHAR2(1999)")]
    [MaxLength(1999)]
    public string Docnbr { get; set; }

    /// <summary>
    ///     Gets or sets the CUS.
    /// </summary>
    /// <value>
    ///     The CUS.
    /// </value>
    [Required]
    [Column(TypeName = "VARCHAR2(1999)")]
    [MaxLength(1999)]
    public string Cus { get; set; }

    /// <summary>
    ///     Gets or sets the TSN.
    /// </summary>
    /// <value>
    ///     The TSN.
    /// </value>
    [Required]
    [Column(TypeName = "VARCHAR2(1999)")]
    [MaxLength(1999)]
    public string Tsn { get; set; }

    /// <summary>
    ///     Gets or sets the STATE.
    /// </summary>
    /// <value>
    ///     The STATE.
    /// </value>
    [Required]
    [Column(TypeName = "VARCHAR2(40)")]
    [MaxLength(40)]
    public string State { get; set; }

    /// <summary>
    ///     Gets or sets the revision date.
    /// </summary>
    /// <value>
    ///     The revision date.
    /// </value>
    [Required]
    [Column(TypeName = "VARCHAR2(1999)")]
    public DateOnly RevDate { get; set; }

    /// <summary>
    ///     Gets or sets the valid time.
    /// </summary>
    /// <value>
    ///     The valid time.
    /// </value>
    [Required]
    [Column(TypeName = "VARCHAR2(1999)")]
    public DateTimeOffset ValidTime { get; set; }

    /// <summary>
    ///     Gets or sets the ObjectRef.
    /// </summary>
    /// <value>
    ///     The ObjectRef.
    /// </value>
    [Required]
    [Column(TypeName = "NUMBER(14)")]
    [MaxLength(14)]
    public long ObjectRef { get; set; }

    /// <summary>
    ///     Gets or sets the OBJECTID.
    /// </summary>
    /// <value>
    ///     The OBJECTID.
    /// </value>
    [Required]
    [Column(TypeName = "NUMBER(12)")]
    [MaxLength(12)]
    public long ObjectId { get; set; }

    /// <summary>
    ///     Gets or sets the PARENTOBJECTID.
    /// </summary>
    /// <value>
    ///     The PARENTOBJECTID.
    /// </value>
    [Required]
    [Column(TypeName = "NUMBER(12)")]
    [MaxLength(12)]
    public long ParentObjectId { get; set; }

    /// <summary>
    ///     Gets or sets the parent's ObjectRef.
    /// </summary>
    /// <value>
    ///     The the parent's ObjectRef.
    /// </value>
    [Required]
    [Column(TypeName = "NUMBER(14)")]
    [MaxLength(14)]
    public long ObjP { get; set; }

}