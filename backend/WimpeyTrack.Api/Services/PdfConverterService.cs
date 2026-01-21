using System.Diagnostics;

namespace WimpeyTrack.Api.Services;

public class PdfConverterService : IPdfConverterService
{
    private readonly string _tempPath;
    
    public PdfConverterService(IWebHostEnvironment env)
    {
        _tempPath = Path.Combine(env.ContentRootPath, "temp");

        if (!Directory.Exists(_tempPath))
        {
            Directory.CreateDirectory(_tempPath);
        }
    }
    
    public async Task<byte[]> ConvertXlsxToPdfAsync(Stream xlsxStream, string? pageRange = null)
    {
        var fileId = Guid.NewGuid().ToString();
        var inputFile = Path.Combine(_tempPath, $"{fileId}.xlsx");
        var outputFile = Path.Combine(_tempPath, $"{fileId}.pdf");
        
        try
        {
            // Save the xlsx to file
            await using (var fileStream = File.Create(inputFile))
            {
                await xlsxStream.CopyToAsync(fileStream);
            }

            // Build the page range JSON if provided
            string exportOptions = string.Empty;
            if (!string.IsNullOrWhiteSpace(pageRange))
            {
                exportOptions =
                    $"{{\\\"PageRange\\\":{{\\\"type\\\":\\\"string\\\",\\\"value\\\":\\\"{pageRange}\\\"}}}}";
            }

            // Build the LibreOffice command
            var convertArgument = string.IsNullOrEmpty(exportOptions)
                ? "pdf:calc_pdf_Export"
                : $"pdf:calc_pdf_Export:{exportOptions}";

            var command =
                $"--headless --convert-to \"{convertArgument}\" --outdir \"{_tempPath}\" \"{inputFile}\"";
            
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "soffice",
                Arguments = command,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (process == null)
            {
                throw new Exception("Failed to start soffice");
            }
            
            await process.WaitForExitAsync();
            
            if (process.ExitCode != 0)
            {
                throw new Exception($"Conversion failed with exit code {process.ExitCode}.");
            }
 
            await Task.Delay(100);

            if (!File.Exists(outputFile))
            {
                throw new FileNotFoundException($"PDF not generated.");
            }

            return await File.ReadAllBytesAsync(outputFile);
        }
        finally
        {
            // Remove the files from the temp directory
            if (File.Exists(inputFile)) File.Delete(inputFile);
            if (File.Exists(outputFile)) File.Delete(outputFile);
        }
    }
}