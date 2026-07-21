using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
// Table detection is not available in the referenced PdfPig package in this project.
// The code below falls back to plain text extraction only.

namespace RagPipeline.Services;

public record ExtractedBlock(int PageNumber, string Content, bool IsTable);

public class PdfExtractionService
{
    /// <summary>
    /// Extracts text and tables page by page. Tables are rendered as markdown
    /// so the LLM can reason over rows/columns during answer generation.
    /// </summary>
    public (int PageCount, List<ExtractedBlock> Blocks) Extract(string pdfPath)
    {
        var blocks = new List<ExtractedBlock>();

        using var document = PdfDocument.Open(pdfPath);

        foreach (Page page in document.GetPages())
        {
            // Plain text extraction for the page.
            var pageText = page.Text;
            if (!string.IsNullOrWhiteSpace(pageText))
            {
                blocks.Add(new ExtractedBlock(page.Number, pageText, IsTable: false));
            }
        }

        return (document.NumberOfPages, blocks);
    }

    // Table rendering removed because the project's PdfPig package does not include table detection.
    // If you add a package that provides UglyToad.PdfPig.Tables, you can restore structured table extraction here.
}
