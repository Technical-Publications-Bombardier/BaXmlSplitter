using Fluxor;
using CsdbProgram = MauiXmlSplitter.Models.CsdbContext.CsdbProgram;

namespace MauiXmlSplitter;

[FeatureState]
public class CsdbProgramState
{
    public CsdbProgram Program { get; init; }

    public CsdbProgramState() { }

    public CsdbProgramState(CsdbProgram program){
        this.Program = program;
    }
}