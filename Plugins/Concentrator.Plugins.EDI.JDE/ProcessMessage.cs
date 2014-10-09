using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.EDI;
using System.Xml.Linq;

namespace Concentrator.Plugins.EDI.JDE
{
  public class ProcessMessage : IEdiOrder
  {
    #region IEdiOrder Members

    public string DocumentType(string requestDocument)
    {
      XDocument xdoc = XDocument.Parse(requestDocument);

      if (xdoc.Root.Name.LocalName == "ProductRequest")
      {
        return "ProductXML";
      }

      if (xdoc.Root.Name.LocalName == "PurchaseRequest")
      {
        return "PurchaseXML";
      }

      if (xdoc.Root.Name.LocalName == "DirectShipmentRequest")
      {
        return "DirectShipmentXML";
      }

      if (xdoc.Root.Name.LocalName.Contains("TradeplaceMessage"))
      {
        return "XML";
      }

      if (xdoc.Root.Name.LocalName == "PurchaseConfirmation")
      {
        return "PurchaseConfirmationXML";
      }

      if (xdoc.Root.Name.LocalName == "InvoiceMessage")
      {
        return "InvoiceXML";
      }

      if (xdoc.Root.Name.LocalName == "ChangeOrderRequest")
      {
        return "OrderChangeXML";
      }

      if (xdoc.Root.Name.LocalName == "ChangePurchangeOrderRequest")
      {
        return "PurchaseOrderChangeXML";
      }

      if (xdoc.Root.Name.LocalName.Contains("OrderRequest"))
      {
        return "XML";
      }

      return "UNKNOWN Type";
    }

    public bool OrderCheck(System.Xml.Linq.XDocument xdoc)
    {
      throw new NotImplementedException();
    }

    public int ProcessOrder(string type, XDocument xdoc)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
