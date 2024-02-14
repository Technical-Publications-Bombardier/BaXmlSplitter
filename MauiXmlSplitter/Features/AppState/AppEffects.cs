using System.Text.Json;
using Fluxor;
using MauiXmlSplitter.Models;
using Microsoft.Extensions.Logging;
using IDispatcher = Fluxor.IDispatcher;
using CsdbProgram = MauiXmlSplitter.Models.CsdbContext.CsdbProgram;

namespace MauiXmlSplitter.Features.AppState;

public class DeserializationEffect
{
    private readonly ILogger<DeserializationEffect> logger;
    private readonly JsonSerializerOptions options;

    public DeserializationEffect(ILogger<DeserializationEffect> logger)
    {
        this.logger = logger;
        options = new JsonSerializerOptions();
        options.Converters.Add(new CsdbProgramConverter());
        options.Converters.Add(new UowStateConverter());
    }

    [EffectMethod]
    public async Task HandleStartDeserializationAction(StartDeserializationAction _, IDispatcher dispatcher)
    {
        // Deserialize JSON resources
        var checkoutItems = await DeserializeCheckoutItems();
        logger.LogInformation("Deserialized CheckoutItems with {NumItems} items", checkoutItems.Count);
        var docnbrManualFromProgram = await DeserializeDocnbrManualFromProgram();
        logger.LogInformation("Deserialized DocnbrManualFromProgram with {NumItems} items",
            docnbrManualFromProgram.Count);
        var programPerDocnbr = await DeserializeProgramPerDocnbr();
        logger.LogInformation("Deserialized ProgramPerDocnbr with {NumItems} items", programPerDocnbr.Count);
        var statesPerProgram = await DeserializeStatesPerProgram();
        logger.LogInformation("Deserialized StatesPerProgram with {NumItems} items", statesPerProgram.Count);
        var lookupEntities = await DeserializeLookupEntities();
        logger.LogInformation("Deserialized LookupEntities with {NumItems} items", lookupEntities.Count);

        // Dispatch actions to update state
        dispatcher.Dispatch(new CompleteDeserializationAction(checkoutItems, docnbrManualFromProgram, programPerDocnbr,
            statesPerProgram, lookupEntities));
    }

    private async Task<Dictionary<CsdbProgram, Dictionary<string, string[]>>> DeserializeCheckoutItems()
    {
        await using var checkoutItemsStream =
            await FileSystem.OpenAppPackageFileAsync("CheckoutItems.json").ConfigureAwait(false);
        await FileSystem.OpenAppPackageFileAsync("CheckoutItems.json").ConfigureAwait(false);
        return await JsonSerializer
            .DeserializeAsync<Dictionary<CsdbProgram, Dictionary<string, string[]>>>(checkoutItemsStream, options)
            .ConfigureAwait(false) ?? throw new InvalidOperationException();
    }

    private async Task<Dictionary<CsdbProgram, Dictionary<string, string>>> DeserializeDocnbrManualFromProgram()
    {
        await using var docnbrManualFromProgramStream =
            await FileSystem.OpenAppPackageFileAsync("DocnbrManualFromProgram.json").ConfigureAwait(false);
        return await JsonSerializer
            .DeserializeAsync<Dictionary<CsdbProgram, Dictionary<string, string>>>(docnbrManualFromProgramStream,
                options)
            .ConfigureAwait(false) ?? throw new InvalidOperationException();
    }

    private async Task<Dictionary<string, CsdbProgram[]>> DeserializeProgramPerDocnbr()
    {
        await using var programPerDocnbrStream =
            await FileSystem.OpenAppPackageFileAsync("ProgramPerDocnbr.json").ConfigureAwait(false);
        return await JsonSerializer
            .DeserializeAsync<Dictionary<string, CsdbProgram[]>>(programPerDocnbrStream, options)
            .ConfigureAwait(false) ?? throw new InvalidOperationException();
    }

    private async Task<Dictionary<CsdbProgram, Dictionary<int, UowState>>> DeserializeStatesPerProgram()
    {
        await using var statesPerProgramStream =
            await FileSystem.OpenAppPackageFileAsync("StatesPerProgram.json").ConfigureAwait(false);
        return await JsonSerializer
            .DeserializeAsync<Dictionary<CsdbProgram, Dictionary<int, UowState>>>(statesPerProgramStream, options)
            .ConfigureAwait(false) ?? throw new InvalidOperationException();
    }

    private async Task<Dictionary<int, string>> DeserializeLookupEntities()
    {
        await using var lookupEntitiesStream =
            await FileSystem.OpenAppPackageFileAsync("LookupEntities.json").ConfigureAwait(false);
        return await JsonSerializer
            .DeserializeAsync<Dictionary<int, string>>(lookupEntitiesStream, options)
            .ConfigureAwait(false) ?? throw new InvalidOperationException();
    }
}