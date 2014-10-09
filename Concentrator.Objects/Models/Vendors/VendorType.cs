using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Vendors
{
  [Flags]
  public enum VendorType
  {
    Stock = 1,
    Assortment = 2,
    Content = 4,
    JdeRetailStock = 8,
    AggregatorRetailStock = 16,
    Barcodes = 32,
    Concentrator = 64,
    HasFinancialProcess = 128,
    HasPfaCommunication = 256,
    SupportsPFATransferOrders = 512,
    UsedForProductEnrichment = 1024,
    ShowsStockInManagementApplication = 2048
  }
}