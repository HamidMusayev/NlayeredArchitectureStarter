using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Http;

namespace CORE.Helpers;

/// <summary>
///     Static helpers for file-related work outside the <c>IBlobStorage</c> abstraction —
///     disk I/O, image validation, and PDF sanitization (strips embedded JavaScript via iText).
/// </summary>
public static class FileHelper
{
    private static readonly HashSet<string> ImageExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

    private static readonly HashSet<string> ImageMimeTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp"
        };

    public static async Task WriteFile(IFormFile file, string name, string path)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        await using var fileStream = new FileStream(Path.Combine(path, name), FileMode.Create);
        await file.CopyToAsync(fileStream);
    }

    public static async Task<string?> ReadFileAsByte64(string name, string path)
    {
        var filePath = Path.Combine(path, name);
        return File.Exists(filePath) ? Convert.ToBase64String(await File.ReadAllBytesAsync(filePath)) : null;
    }

    public static bool DeleteFile(string filePath)
    {
        if (!File.Exists(filePath)) return false;

        File.Delete(filePath);

        return true;
    }

    public static async Task<IFormFile?> ReadFileAsIFormFile(string name, string path)
    {
        var filePath = Path.Combine(path, name);
        if (!File.Exists(filePath)) return null; // or throw an exception based on your use case

        var fileInfo = new FileInfo(filePath);
        var memoryStream = new MemoryStream();

        await using (var stream = fileInfo.OpenRead())
        {
            await stream.CopyToAsync(memoryStream);
        }

        memoryStream.Position = 0;

        return new FormFile(memoryStream, 0, memoryStream.Length, null, fileInfo.Name);
    }

    public static bool IsValidPdf(IFormFile file)
    {
        if (Path.GetExtension(file.FileName).ToLowerInvariant() != ".pdf")
            return false;

        return file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsValidImage(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName);
        return ImageExtensions.Contains(ext) && ImageMimeTypes.Contains(file.ContentType);
    }

    /*public async Task<bool> ScanForVirusesAsync(string filePath)
    {
        var clamAvClient = new ClamClient("localhost", 3310); // ClamAV server
        using var fileStream = new FileStream(filePath, FileMode.Open);
        var scanResult = await clamAvClient.SendAndScanFileAsync(fileStream);

        return scanResult.Result == ClamScanResults.Clean;
    }*/

    public static async Task<IFormFile> RemoveJavaScriptFromPdfAsync(IFormFile file)
    {
        using var inputStream = new MemoryStream();
        await file.CopyToAsync(inputStream);
        inputStream.Position = 0;

        byte[] sanitizedPdf;

        using (var outputStream = new MemoryStream())
        using (var reader = new PdfReader(inputStream))
        await using (var writer = new PdfWriter(outputStream))
        using (var pdf = new PdfDocument(reader, writer))
        {
            var catalogDict = pdf.GetCatalog().GetPdfObject();

            // Catalog JS
            catalogDict.Remove(PdfName.OpenAction);
            catalogDict.Remove(PdfName.AA);

            var names = catalogDict.GetAsDictionary(PdfName.Names);
            names?.Remove(PdfName.JavaScript);

            var pageCount = pdf.GetNumberOfPages();
            for (var i = 1; i <= pageCount; i++)
            {
                var page = pdf.GetPage(i);
                var pageDict = page.GetPdfObject();
                pageDict.Remove(PdfName.AA);

                foreach (var annot in page.GetAnnotations())
                {
                    var annotDict = annot.GetPdfObject();
                    annotDict.Remove(PdfName.A);
                    annotDict.Remove(PdfName.AA);
                }
            }

            var acroForm = catalogDict.GetAsDictionary(PdfName.AcroForm);
            acroForm?.Remove(PdfName.AA);
            acroForm?.Remove(PdfName.XFA);

            pdf.Close();
            sanitizedPdf = outputStream.ToArray();
        }

        var finalStream = new MemoryStream(sanitizedPdf);

        return new FormFile(finalStream, 0, finalStream.Length, file.Name, file.FileName)
        {
            Headers = file.Headers,
            ContentType = file.ContentType
        };
    }
}