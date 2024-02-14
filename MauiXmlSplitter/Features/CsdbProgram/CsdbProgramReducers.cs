using Fluxor;

namespace MauiXmlSplitter;

public static class CsdbProgramReducers
{
    [ReducerMethod]
    public static CsdbProgramState ReduceSelectCsdbProgramAction(CsdbProgramState state, SelectCsdbProgramAction action)
    {
        return new CsdbProgramState(action.Program);
    }
}