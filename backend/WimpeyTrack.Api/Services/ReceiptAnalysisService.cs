using Azure;
using Azure.AI.DocumentIntelligence;
using WimpeyTrack.Api.Dtos;
using WimpeyTrack.Api.Dtos.Receipt;
using WimpeyTrack.Api.Dtos.Shared;
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

    public async Task<ReceiptData?> AnalyseReceiptAsync(BinaryData receiptData)
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
                return null;
            }

            // Create list 
            var receipt = new ReceiptData();
            var document = result.Documents.First();
            
            // Get the bounding box
            receipt.BoundingBox = GetOverallBoundingBox(document);
            
            if (document.Fields.TryGetValue("MerchantName", out var merchantNameField))
            {
                if (merchantNameField.FieldType == DocumentFieldType.String)
                {
                    var merchantName = merchantNameField.ValueString;
                    var storeName = merchantName.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0];
                    receipt.StoreName = storeName;
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

                    // Set the default quantity to 1
                    receiptItem.Quantity = 1;

                    if (itemFields.TryGetValue("Quantity", out var quantityField))
                    {
                        receiptItem.Quantity = quantityField.ValueDouble;
                    }

                    receipt.ReceiptItems.Add(receiptItem);
                }
            }

            _logger.LogInformation("Successfully analysed receipt");
            return receipt;
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
    
    private BoundingBox? GetOverallBoundingBox(AnalyzedDocument document)
    {
        int? minX = null, minY = null, maxX = null, maxY = null;

        foreach (var field in document.Fields.Values)
        {
            if (field.BoundingRegions == null) continue;

            foreach (var region in field.BoundingRegions)
            {
                if (region.Polygon == null || region.Polygon.Count != 8) continue;

                for (int i = 0; i < 8; i += 2)
                {
                    int x = (int)Math.Round(region.Polygon[i]);
                    int y = (int)Math.Round(region.Polygon[i + 1]);

                    minX = minX.HasValue ? Math.Min(minX.Value, x) : x;
                    minY = minY.HasValue ? Math.Min(minY.Value, y) : y;
                    maxX = maxX.HasValue ? Math.Max(maxX.Value, x) : x;
                    maxY = maxY.HasValue ? Math.Max(maxY.Value, y) : y;
                }
            }
        }

        if (!minX.HasValue || !minY.HasValue || !maxX.HasValue || !maxY.HasValue)
            return null;

        return new BoundingBox
        {
            X = minX.Value,
            Y = minY.Value,
            Width = maxX.Value - minX.Value,
            Height = maxY.Value - minY.Value
        };
    }
}