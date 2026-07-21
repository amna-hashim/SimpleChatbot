using RagPipeline.Models;

namespace RagPipeline.Interfaces;

public interface IDocumentChunksRepository
{
    Task AddWithEmbeddingAsync(DocumentChunk chunk, float[] embedding, CancellationToken ct);
    Task<List<DocumentChunk>> GetBySourceDocumentIdAsync(int sourceDocumentId, CancellationToken ct);
}
