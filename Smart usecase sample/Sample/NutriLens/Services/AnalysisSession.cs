using NutriLens.Models;

namespace NutriLens.Services;

public sealed class AnalysisSession
{
    public FileResult? SelectedImage { get; private set; }

    public ProductAnalysis? Analysis { get; private set; }

    public Exception? Error { get; private set; }

    public bool IsLoading { get; private set; }

    public void SetSelectedImage(FileResult image)
    {
        SelectedImage = image;
        Analysis = null;
        Error = null;
    }

    public async Task AnalyzeAsync(
        FileResult image,
        IAzureOpenAIAnalysisService analysisService,
        CancellationToken cancellationToken = default)
    {
        SelectedImage = image;
        Analysis = null;
        Error = null;
        IsLoading = true;

        try
        {
            Analysis =
                await analysisService.AnalyzeAsync(
                    image,
                    cancellationToken);
        }
        catch (Exception exception)
        {
            Error = exception;
            throw;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void Clear()
    {
        SelectedImage = null;
        Analysis = null;
        Error = null;
        IsLoading = false;
    }
}