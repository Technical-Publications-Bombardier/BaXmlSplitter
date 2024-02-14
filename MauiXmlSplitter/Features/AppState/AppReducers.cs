using Fluxor;

namespace MauiXmlSplitter.Features.AppState;

public static class AppReducers
{
    [ReducerMethod]
    public static AppState ReduceCompleteDeserializationAction(AppState state, CompleteDeserializationAction action)
    {
        return new AppState(state.UowSpecifier, state.OutputDirectory, state.CsdbProgramSelection,
            state.Settings, action.LookupEntities);
    }
}