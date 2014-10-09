namespace Concentrator.Plugins.Axapta.Enum
{
  public enum SaveTo
  {
    StockDirectory,
    CorrectionStockDirectory,
    CorruptStockDirectory,
    CorruptCorrectionStockDirectory,

    PurchaseOrderDirectory,
    ReceivedPurchaseOrderConfirmationDirectory,
    CorruptPurchaseOrderDirectory,
    
    PickTicketDirectory,
    PickingPickTicketShipmentConfirmationDirectory,
    TransferPickTicketShipmentConfirmationDirectory,
    CorruptPickTicketDirectory,
    CorruptNotificationDirectory,

    CustomerInformationDirectory,
    CorruptCustomerInformationDirectory
  }
}
