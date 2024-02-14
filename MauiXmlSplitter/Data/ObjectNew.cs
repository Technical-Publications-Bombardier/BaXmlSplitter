using System.ComponentModel.DataAnnotations;

namespace BaXmlSplitter
{
    public class ObjectNew
    {
        [Required]
        public int ObjectRef { get; set; }

        [Required]
        public int ObjectId { get; set; }

        [Required]
        public DateTime CreateTime { get; set; }

        public int? SourceObjectId { get; set; }

        public DateTime? SourceCreateTime { get; set; }

        public int? SourceUpdateCount { get; set; }

        public int? ParentObjectId { get; set; }

        public int? ObjectSeqNo { get; set; }

        public int? ApprovalPathRef { get; set; }

        public int? FiniteStateMachineRef { get; set; }

        public int? CurrentState { get; set; }

        [Required]
        public int UserRef { get; set; }

        [Required]
        public DateTime ModifyTime { get; set; }

        [Required]
        public DateTime ValidTime { get; set; }

        [Required]
        public DateTime ObsoleteTime { get; set; }

        [Required]
        public DateTime OfflineTime { get; set; }

        public int? LinkType { get; set; }

        public DateTime? LinkTime { get; set; }

        public string LinkContext { get; set; }

        public int? LinkObject { get; set; }

        public string ObjectName { get; set; }

        public string ObjectKey { get; set; }

        public string RevisionSymbol { get; set; }

        [Required]
        public int ObjectType { get; set; }

        public int? DataType { get; set; }

        public int? Length { get; set; }

        public int? Checksum { get; set; }

        public int? Flags { get; set; }
    }
}
