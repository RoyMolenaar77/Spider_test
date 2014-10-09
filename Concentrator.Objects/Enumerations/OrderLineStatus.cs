
namespace Concentrator
{
  public enum OrderLineStatus
  {
    New = 0,
    InProcess = 1,
    WaitingForPurchaseConfirmation = 2,
    PushProduct = 3,
    ReadyToOrder = 10,
    WaitingForAcknowledgement = 90,
    WaitingForShipmentNotification = 91,
    WaitingForInvoiceNotification = 92,    
    ProcessedAdministrativeVendorChangeNotification = 93,
    ProcessedAdministrativeVendorInvoiceNotification = 94,
    ProcessedInvoicePaymentProvider = 95,
    ProcessedPurchaseConfirmation = 96,
    ProcessedExportNotification = 97,
    
    ProcessedReturnNotification = 98,
    ProcessedUnassignedNotification = 80,
    ProcessedReturnExportNotification = 100,
    ProcessedCancelExportNotification = 110,
    
    Cancelled = 99,
    Processed = 999,
    ProcessedKasmut = 120,
        
    ReceivedTransfer = 130,
    ProcessedReceivedTransfer = 140,

    StockReturnRequestConfirmation = 150,
    ProcessedStockReturnRequestConfirmation = 160
  }
}
