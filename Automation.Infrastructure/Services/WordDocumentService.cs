using Automation.Core.Entities;
using System.IO;

namespace Automation.Infrastructure.Services;

/// <summary>
/// سرویس پردازش فایل‌های Word برای قالب‌های چاپ
/// </summary>
public interface IWordDocumentService
{
    /// <summary>
    /// جایگزینی بوکمارک‌های Word با داده‌های فرم
    /// </summary>
    Task<string> ReplaceBookmarksAsync(string templatePath, Dictionary<string, object> formData, ICollection<FormField> fields);
}

public class WordDocumentService : IWordDocumentService
{
    public async Task<string> ReplaceBookmarksAsync(string templatePath, Dictionary<string, object> formData, ICollection<FormField> fields)
    {
        // Generate output file path
        var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temp");
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        var outputPath = Path.Combine(outputDir, $"output_{Guid.NewGuid()}.docx");

        // TODO: Install DocumentFormat.OpenXml NuGet package
        // Install-Package DocumentFormat.OpenXml
        // 
        // Implementation example:
        // using DocumentFormat.OpenXml.Packaging;
        // using DocumentFormat.OpenXml.Wordprocessing;
        // 
        // 1. Copy template to output location
        System.IO.File.Copy(templatePath, outputPath, true);
        
        // 2. Open the Word document
        // using (WordprocessingDocument doc = WordprocessingDocument.Open(outputPath, true))
        // {
        //     MainDocumentPart mainPart = doc.MainDocumentPart;
        //     if (mainPart == null) return outputPath;
        //     
        //     // 3. Find all bookmarks
        //     var bookmarks = mainPart.Document.Descendants<BookmarkStart>();
        //     
        //     foreach (var bookmark in bookmarks)
        //     {
        //         var bookmarkName = bookmark.Name;
        //         
        //         // 4. Match bookmark name with form field (check DatabaseColumnName or Name)
        //         var field = fields.FirstOrDefault(f => 
        //             f.DatabaseColumnName == bookmarkName || 
        //             f.Name == bookmarkName);
        //         
        //         if (field != null && formData.ContainsKey(field.DatabaseColumnName ?? field.Name))
        //         {
        //             var value = formData[field.DatabaseColumnName ?? field.Name]?.ToString() ?? "";
        //             
        //             // 5. Replace bookmark content
        //             var bookmarkEnd = mainPart.Document.Descendants<BookmarkEnd>()
        //                 .FirstOrDefault(b => b.Id == bookmark.Id);
        //             
        //             if (bookmarkEnd != null)
        //             {
        //                 // Remove existing content between bookmark start and end
        //                 var runs = bookmark.ElementsAfter()
        //                     .TakeWhile(e => e != bookmarkEnd)
        //                     .OfType<Run>()
        //                     .ToList();
        //                 
        //                 foreach (var run in runs)
        //                 {
        //                     run.Remove();
        //                 }
        //                 
        //                 // Insert new text
        //                 var newRun = new Run(new Text(value));
        //                 bookmarkEnd.InsertAfterSelf(newRun);
        //             }
        //         }
        //     }
        //     
        //     mainPart.Document.Save();
        // }

        await Task.CompletedTask;

        // For now, just return the copied file path
        // In production, implement the OpenXML logic above
        return outputPath;
    }
}




