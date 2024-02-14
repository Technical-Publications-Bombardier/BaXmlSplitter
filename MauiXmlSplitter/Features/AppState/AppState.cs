using Fluxor;

namespace MauiXmlSplitter.Features.AppState;

[FeatureState]
public class AppState
{
    public AppState()
    {
        LookupEntities = [];
        UowSpecifier = new UowSpecifierState();
        OutputDirectory = new OutputDirectoryState();
        CsdbProgramSelection = new CsdbProgramState();
        Settings = new SettingsState();

    }
    public AppState(UowSpecifierState uowSpecifier,
        OutputDirectoryState outputDirectory,
        CsdbProgramState csdbProgramSelection,
        SettingsState settings,
        Dictionary<int, string> lookupEntities)
    {
        LookupEntities = lookupEntities;
        UowSpecifier = uowSpecifier;
        OutputDirectory = outputDirectory;
        CsdbProgramSelection = csdbProgramSelection;
        Settings = settings;
    }

    public Dictionary<int, string> LookupEntities { get; }
    public UowSpecifierState UowSpecifier { get; }
    public OutputDirectoryState OutputDirectory { get; }
    public CsdbProgramState CsdbProgramSelection { get; }
    public SettingsState Settings { get; }
}