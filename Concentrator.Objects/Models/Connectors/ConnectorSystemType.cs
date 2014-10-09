using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Connectors
{
  [Flags]
  public enum ConnectorSystemType
  {
    LoadProductGroups = 1,
    DontActivateNewProducts = 2,
    ExportStock = 4,
    ExportAddionalItems = 8,
    AutoCleanUpAssortment = 16,
    AutoAnchorSubProductGroups = 32,
    ExportRetailStock = 64,
    ShopOrders = 128,
    ImportSingleBarcode = 256,
    CallShipmentLink = 512,
    TakeContentVendorProductName = 1024
  }
}
