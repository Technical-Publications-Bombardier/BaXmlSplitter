using MauiXmlSplitter.Models;
using CsdbProgram = MauiXmlSplitter.Models.CsdbContext.CsdbProgram;

namespace MauiXmlSplitter.Features.AppState;

public class StartDeserializationAction
{
}

public class CompleteDeserializationAction(
    Dictionary<CsdbProgram, Dictionary<string, string[]>> checkoutItems,
    Dictionary<CsdbProgram, Dictionary<string, string>> docnbrManualFromProgram,
    Dictionary<string, CsdbProgram[]> programPerDocnbr,
    Dictionary<CsdbProgram, Dictionary<int, UowState>> statesPerProgram,
    Dictionary<int, string> lookupEntities)
{
    public Dictionary<int, string> LookupEntities { get; } = lookupEntities;
    public Dictionary<CsdbProgram, Dictionary<string, string[]>> CheckoutItems { get; } = checkoutItems;

    public Dictionary<CsdbProgram, Dictionary<string, string>> DocnbrManualFromProgram { get; } =
        docnbrManualFromProgram;

    public Dictionary<string, CsdbProgram[]> ProgramPerDocnbr { get; } = programPerDocnbr;
    public Dictionary<CsdbProgram, Dictionary<int, UowState>> StatesPerProgram { get; } = statesPerProgram;
}