using Fluxor;
using Microsoft.Extensions.Logging;
using IDispatcher = Fluxor.IDispatcher;

namespace MauiXmlSplitter.Features.OutputDirectory;

public class OutputDirectoryEffects(ILogger<OutputDirectoryEffects> logger)
{
    [EffectMethod]
    public async Task HandleSelectOutputDirectoryAction(SelectOutputDirectoryAction action, IDispatcher dispatcher)
    {
        try
        {
            DirectoryInfo outputDirectory = default!;
            if (!Directory.Exists(action.OutputDirectory))
            {
                logger.LogTrace("Creating output directory: '{OutputDirectory}'", action.OutputDirectory);
                outputDirectory = Directory.CreateDirectory(action.OutputDirectory);
                logger.LogTrace("Created output directory: '{OutputDirectory}'", outputDirectory);
            }
            else
            {
                outputDirectory = new DirectoryInfo(action.OutputDirectory);
            }

            // Dispatch action to update state
            dispatcher.Dispatch(new OutputDirectoryCreatedAction(outputDirectory));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating a directory at '{OutputDirectory}'",
                action.OutputDirectory);
            // Dispatch action to indicate error
            dispatcher.Dispatch(new OutputDirectoryErrorAction(ex));
        }
    }
}