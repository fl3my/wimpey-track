using System.IO.Compression;
using WimpeyTrack.Api.Dtos.ReportGeneration;

namespace WimpeyTrack.Api.Domain;

/// <summary>
/// Builds a zip containing the files generated from report.
/// </summary>
public interface IReportZipBuilder
{
    byte[] BuildZip(IReadOnlyList<ReportFile> files);
}

public class ReportZipBuilder : IReportZipBuilder
{
    public byte[] BuildZip(IReadOnlyList<ReportFile> files)
    {
        // This memory stream will contain the zip file
        using var zipStream = new MemoryStream();
        
        // Create the zip archive in memory
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            foreach (var file in files)
            {
                var entry = archive.CreateEntry(
                    file.FileName,
                    CompressionLevel.Fastest);

                using var entryStream = entry.Open();
                entryStream.Write(file.Content);
            }
        }
        
        return zipStream.ToArray();
    }
}