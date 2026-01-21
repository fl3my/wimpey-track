namespace WimpeyTrack.Api.Services;

public interface IPdfConverterService
{
    Task<byte[]> ConvertXlsxToPdfAsync(Stream xlsxStream, string? pageRange = null);
}