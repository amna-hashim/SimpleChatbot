using RagPipeline.Models;

namespace RagPipeline.Interfaces;

public interface ISourceDocumentRepository
{
    Task<SourceDocument?> GetByIdAsync(int id, CancellationToken ct);
    Task<SourceDocument?> GetWithChunksAsync(int id, CancellationToken ct);

    Task AddAsync(SourceDocument document, CancellationToken ct);
}
