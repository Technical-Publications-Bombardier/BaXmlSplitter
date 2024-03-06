using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace TechPubsDatabase.Data;

/// <summary>
/// Represents entities in the <c>ANCHOR</c> tables.
/// </summary>
public class Anchor
{
    /// <summary>
    /// Gets or sets the anchor reference.
    /// </summary>
    /// <value>
    /// The anchor reference.
    /// </value>
    [Column(TypeName = "NUMBER(12)")]
    [Required] public long AnchorRef { get; set; }

    /// <summary>
    /// Gets or sets the manual object reference.
    /// </summary>
    /// <value>
    /// The manual object reference.
    /// </value>
    [Column(TypeName = "NUMBER(14)")]
    [Required] public long ManualObjectRef { get; set; }

    /// <summary>
    /// Gets or sets the sequence number.
    /// </summary>
    /// <value>
    /// The sequence number.
    /// </value>
    [Column(TypeName = "NUMBER(8)")]
    [Required] public int SeqNo { get; set; }

    /// <summary>
    /// Gets or sets the object path.
    /// </summary>
    /// <value>
    /// The object path.
    /// </value>
    [Column(TypeName = "VARCHAR2(1000)")]
    [MaxLength(1000)]
    [Required] public string ObjectPath { get; set; }

    /// <summary>
    /// Gets or sets the object reference.
    /// </summary>
    /// <value>
    /// The object reference.
    /// </value>
    [Column(TypeName = "NUMBER(14)")]
    [Required] public long ObjectRef { get; set; }

    /// <summary>
    /// Gets or sets the document reference.
    /// </summary>
    /// <value>
    /// The document reference.
    /// </value>
    [Column(TypeName = "NUMBER(6)")]
    public int? DocumentRef { get; set; }

    /// <summary>
    /// Gets or sets the key.
    /// </summary>
    /// <value>
    /// The key.
    /// </value>
    [Column(TypeName = "VARCHAR2(32)")]
    [MaxLength(32)]
    [Required] public string Key { get; set; }

    /// <summary>
    /// Gets or sets the CHG desc.
    /// </summary>
    /// <value>
    /// The CHG desc.
    /// </value>
    [Column(TypeName = "long")]
    [MaxLength(0)]
    public string? ChgDesc { get; set; }

    /// <summary>
    /// Gets or sets the CHG.
    /// </summary>
    /// <value>
    /// The CHG.
    /// </value>
    [Column(TypeName = "char(1)")]
    [Required] public char Chg { get; set; }

    /// <summary>
    /// Gets or sets the rev date.
    /// </summary>
    /// <value>
    /// The rev date.
    /// </value>
    [Column(TypeName = "VARCHAR2(255)")]
    [MaxLength(255)]
    [Required] public string RevDate { get; set; }
}