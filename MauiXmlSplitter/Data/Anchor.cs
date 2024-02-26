using System.ComponentModel.DataAnnotations;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MauiXmlSplitter.Data;

/// <summary>
/// 
/// </summary>
public class Anchor
{
    /// <summary>
    /// Gets or sets the anchor reference.
    /// </summary>
    /// <value>
    /// The anchor reference.
    /// </value>
    [Required] public int AnchorRef { get; set; }

    /// <summary>
    /// Gets or sets the manual object reference.
    /// </summary>
    /// <value>
    /// The manual object reference.
    /// </value>
    [Required] public int ManualObjectRef { get; set; }

    /// <summary>
    /// Gets or sets the seq no.
    /// </summary>
    /// <value>
    /// The seq no.
    /// </value>
    [Required] public int SeqNo { get; set; }

    /// <summary>
    /// Gets or sets the object path.
    /// </summary>
    /// <value>
    /// The object path.
    /// </value>
    [Required] public string ObjectPath { get; set; }

    /// <summary>
    /// Gets or sets the object reference.
    /// </summary>
    /// <value>
    /// The object reference.
    /// </value>
    [Required] public int ObjectRef { get; set; }

    /// <summary>
    /// Gets or sets the document reference.
    /// </summary>
    /// <value>
    /// The document reference.
    /// </value>
    public int? DocumentRef { get; set; }

    /// <summary>
    /// Gets or sets the key.
    /// </summary>
    /// <value>
    /// The key.
    /// </value>
    [Required] public string Key { get; set; }

    /// <summary>
    /// Gets or sets the CHG desc.
    /// </summary>
    /// <value>
    /// The CHG desc.
    /// </value>
    public string ChgDesc { get; set; }

    /// <summary>
    /// Gets or sets the CHG.
    /// </summary>
    /// <value>
    /// The CHG.
    /// </value>
    [Required] public string Chg { get; set; }

    /// <summary>
    /// Gets or sets the rev date.
    /// </summary>
    /// <value>
    /// The rev date.
    /// </value>
    [Required] public string RevDate { get; set; }
}