using Azure;
using Azure.AI.DocumentIntelligence;
using DocumentFormat.OpenXml.Spreadsheet;
using WimpeyTrack.Api.Dtos;
using WimpeyTrack.Api.Enums;

namespace WimpeyTrack.Api.Services;

public class ReceiptAnalysisService : IReceiptAnalysisService
{
    private readonly DocumentIntelligenceClient  _documentClient;
    private readonly ILogger<ReceiptAnalysisService> _logger;

    public ReceiptAnalysisService(IConfiguration configuration, ILogger<ReceiptAnalysisService> logger)
    {
        // Validate that we have config values
        var endpoint = configuration["AzureDocumentIntelligence:Endpoint"] 
                       ?? throw new InvalidOperationException("Azure Document Intelligence endpoint is not configured");
        var apiKey = configuration["AzureDocumentIntelligence:ApiKey"] 
                     ?? throw new InvalidOperationException("Azure Document Intelligence API key is not configured");
        
        // Create a document client 
        _documentClient = new DocumentIntelligenceClient(
            new Uri(endpoint),
            new AzureKeyCredential(apiKey));
        
        // Initiate a logger
        _logger = logger;
    }

    public async Task<List<ReceiptData>> AnalyseReceiptAsync(BinaryData receiptData)
    {
        try
        {
            // Make the request
            var operation =
                await _documentClient.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-receipt", receiptData);
            var result = operation.Value;

            // If no documents, return empty
            if (result.Documents.Count == 0)
            {
                _logger.LogWarning("No receipts found for {Receipt}", receiptData);
                return [];
            }

            // Create list 
            var receipts = new List<ReceiptData>();

            foreach (var document in result.Documents)
            {
                var receipt = new ReceiptData();

                if (document.Fields.TryGetValue("MerchantName", out var merchantNameField))
                {
                    if (merchantNameField.FieldType == DocumentFieldType.String)
                    {
                        receipt.StoreName = merchantNameField.ValueString;
                    }
                }

                if (document.Fields.TryGetValue("TransactionDate", out DocumentField transactionDateField))
                {
                    if (transactionDateField.FieldType == DocumentFieldType.Date)
                    {
                        if (transactionDateField.ValueDate != null)
                            receipt.TransactionDate = DateOnly.FromDateTime(transactionDateField.ValueDate.Value.DateTime);
                    }
                }
                
                if (document.Fields.TryGetValue("ReceiptType", out DocumentField receiptTypeField))
                {
                    if (receiptTypeField.FieldType == DocumentFieldType.String)
                    {
                        receipt.ReceiptCategory = receiptTypeField.ValueString == "Fuel&Energy.Gas" ? ReceiptCategory.Fuel : ReceiptCategory.Supplies;
                    }
                }

                if (document.Fields.TryGetValue("Items", out var itemsField))
                {
                    foreach (var itemField in itemsField.ValueList)
                    {
                        var receiptItem = new ReceiptItem();
                        var itemFields = itemField.ValueDictionary;

                        if (itemFields.TryGetValue("Description", out var itemDescriptionField))
                        {
                            receiptItem.Description = itemDescriptionField.ValueString;
                        }

                        if (itemFields.TryGetValue("TotalPrice", out var totalPriceField))
                        {
                            receiptItem.Price = totalPriceField.ValueCurrency.Amount;
                        }

                        if (itemFields.TryGetValue("Quantity", out var quantityField))
                        {
                            receiptItem.Quantity = quantityField.ValueDouble;
                        }

                        receipt.ReceiptItems.Add(receiptItem);
                    }
                }

                receipts.Add(receipt);
            }

            _logger.LogInformation("Successfully analysed {Count} receipt", receipts.Count);
            return receipts;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure Document Intelligence request failed");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analysing receipt");
            throw;
        }
    }
}