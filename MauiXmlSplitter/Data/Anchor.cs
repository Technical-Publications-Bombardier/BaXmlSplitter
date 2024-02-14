using System.ComponentModel.DataAnnotations;

namespace BaXmlSplitter;

public class Anchor
{
    [Required] public int AnchorRef { get; set; }

    [Required] public int ManualObjectRef { get; set; }

    [Required] public int SeqNo { get; set; }

    [Required] public string ObjectPath { get; set; }

    [Required] public int ObjectRef { get; set; }

    public int? DocumentRef { get; set; }

    [Required] public string Key { get; set; }

    public string ChgDesc { get; set; }

    [Required] public string Chg { get; set; }

    [Required] public string RevDate { get; set; }
}