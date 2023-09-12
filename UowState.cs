using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Xml;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BaXmlSplitter.Tests")]

namespace BaXmlSplitter
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    internal record class UowState : IUowState
    {
        public string? XPath { get; set; }
        public string? TagName { get; set; }
        public string? Key { get; set; }
        public string? Resource { get; set; }
        public string? Title { get; set; }
        public string? Level { get; set; }
        public string? StateName { get; set; }
        public int? StateValue { get; set; }
        public string? Remark { get; set; }
        public UowState(int? value = null, string? name = null, string? remark = null, string? xpath = null, string? tag = null, string? key = null, string? resource = null, string? title = null, string? level = null)
        {
            StateValue = value;
            StateName = name;
            Remark = remark;
            XPath = xpath;
            TagName = tag;
            Key = key;
            Resource = resource;
            Title = title;
            Level = level;
        }

        public override string ToString()
        {
            return String.Join(", ", new ArrayList() { (StateValue == null ? null : $"StateValue={StateValue}"), (string.IsNullOrEmpty(StateName) ? null : $"StateName='{StateName}'"), (string.IsNullOrEmpty(Remark) ? null : $"Remark='{Remark}'"), (string.IsNullOrEmpty(XPath) ? null : $"XPath='{XPath}'"), (string.IsNullOrEmpty(TagName) ? null : $"TagName='{TagName}'"), (string.IsNullOrEmpty(Key) ? null : $"Key='{Key}'"), (string.IsNullOrEmpty(Resource) ? null : $"Resource='{Resource}'"), (string.IsNullOrEmpty(Title) ? null : $"Title='{Title}'"), (string.IsNullOrEmpty(Level) ? null : $"Level='{Level}'") }.Cast<string>().Where(field => !string.IsNullOrEmpty(field)).ToArray());
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }

        public bool Equals(IUowState? other)
        {
            return other != null && StateValue == other.StateValue;
        }

        public bool Equals(IUowState? x, IUowState? y)
        {
            return x != null && y != null && x.StateValue == y.StateValue;
        }

        public int GetHashCode(IUowState obj)
        {
            return obj.StateValue.GetHashCode();
        }

    }
    internal interface IUowState : IEquatable<IUowState>, IEqualityComparer<IUowState>
    {
        int? StateValue { get; set; }
        string? StateName { get; set; }
        string? Remark { get; set; }
        string? XPath { get; set; }
        string? TagName { get; set; }
        string? Key { get; set; }
        string? Resource { get; set; }
        string? Title { get; set; }
        string? Level { get; set; }
    }
}
