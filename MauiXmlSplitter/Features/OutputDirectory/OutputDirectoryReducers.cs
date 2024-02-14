using Fluxor;

namespace MauiXmlSplitter;

public static class OutputDirectoryReducers
{
    [ReducerMethod]
    public static OutputDirectoryState ReduceOutputDirectoryCreatedAction(OutputDirectoryState state,
        OutputDirectoryCreatedAction action)
    {
        // Update state with the created directory
        return new OutputDirectoryState(action.OutputDirectory);
    }

    [ReducerMethod]
    public static OutputDirectoryState ReduceOutputDirectoryErrorAction(OutputDirectoryState state,
        OutputDirectoryErrorAction action)
    {
        // Update state with the error message
        return new OutputDirectoryState(state.OutputDirectory, action.Exception);
    }
}