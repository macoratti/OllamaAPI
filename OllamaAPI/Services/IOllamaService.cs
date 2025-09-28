using OllamaSharp.Models;

namespace OllamaAPI.Services;

public interface IOllamaService
{
    Task<IEnumerable<Model>> GetLocalModelsAsync(CancellationToken
                                                 cancellationToken = default);
}
