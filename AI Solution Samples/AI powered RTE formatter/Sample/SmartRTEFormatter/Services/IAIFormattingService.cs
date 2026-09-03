using SmartRTEFormatter.Models;

namespace SmartRTEFormatter.Services;

public interface IAIFormattingService
{
    Task<FormattingResult> FormatAsync(string content, CancellationToken cancellationToken = default);
}