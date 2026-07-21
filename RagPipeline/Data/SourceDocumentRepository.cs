using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RagPipeline.Interfaces;
using RagPipeline.Models;

namespace RagPipeline.Data;

public class SourceDocumentRepository : ISourceDocumentRepository
{
    private readonly RagDbContext _db;
    public SourceDocumentRepository(RagDbContext db) => _db = db;

    public async Task AddAsync(SourceDocument document, CancellationToken ct)
    {
        var conn = (SqlConnection)_db.Database.GetDbConnection();

        if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);

        await using var cmd = new SqlCommand(@"
            INSERT INTO [dbo].[SourceDocuments]
                   ([FileName]
                   ,[Title]
                   ,[IngestedAtUtc]
                   ,[PageCount])
            OUTPUT INSERTED.Id
            VALUES
                   (@FileName
                   ,@Title
                   ,@IngestedAtUtc
                   ,@PageCount);",
            conn);

        cmd.Parameters.AddWithValue("@FileName", document.FileName);
        cmd.Parameters.AddWithValue("@Title", (object?)document.Title ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IngestedAtUtc", document.IngestedAtUtc);
        cmd.Parameters.AddWithValue("@PageCount", document.PageCount);

        document.Id = (int)await cmd.ExecuteScalarAsync(ct);
    }

    public Task<SourceDocument?> GetByIdAsync(int id, CancellationToken ct) =>
        _db.SourceDocuments.FirstOrDefaultAsync(d => d.Id == id, ct);

    public Task<SourceDocument?> GetWithChunksAsync(int id, CancellationToken ct) =>
        _db.SourceDocuments.Include(d => d.Chunks).FirstOrDefaultAsync(d => d.Id == id, ct);
}
