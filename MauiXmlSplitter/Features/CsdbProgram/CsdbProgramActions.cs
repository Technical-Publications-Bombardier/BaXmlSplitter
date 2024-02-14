using CsdbProgram = MauiXmlSplitter.Models.CsdbContext.CsdbProgram;

namespace MauiXmlSplitter;

public class SelectCsdbProgramAction(CsdbProgram program)
{
    public CsdbProgram Program { get; } = program;
}