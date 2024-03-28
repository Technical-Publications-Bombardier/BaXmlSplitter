using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPubsDatabase.Data;

public class State
{
    /// <summary>
    ///     Gets or sets the StateValue.
    /// </summary>
    /// <value>
    ///     The StateValue.
    /// </value>
    [Column(TypeName = "NUMBER(4)")]
    [Required]
    public short StateValue { get; set; }

    /// <summary>
    ///     Gets or sets the StateName.
    /// </summary>
    /// <value>
    ///     The StateName.
    /// </value>
    [Column(TypeName = "VARCHAR2(12)")]
    [MaxLength(12)]
    [Required]
    public required string StateName { get; set; }

    /// <summary>
    ///     Gets or sets the Remark.
    /// </summary>
    /// <value>
    ///     The Remark.
    /// </value>
    [Column(TypeName = "VARCHAR2(1999)")]
    [MaxLength(1999)]
    public string? Remark { get; set; }
}