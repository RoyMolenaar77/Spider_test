using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Payment.Providers.AfterPayMerchant;
using System.ServiceModel;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Payment.Providers
{
  public class AfterPay : IPayment
  {
    #region IPayment Members

    public bool InvoiceOrder(OrderResponse invoiceResponse, ConnectorPaymentProvider connectorPaymentProvider, AuditLog4Net.Adapter.IAuditLogAdapter log)
    {
      Concentrator.Payment.Providers.AfterPayMerchant.MerchantClient client = new Concentrator.Payment.Providers.AfterPayMerchant.MerchantClient();
      try
      {
        //user credentials
        client.ClientCredentials.UserName.UserName = connectorPaymentProvider.UserName;
        client.ClientCredentials.UserName.Password = connectorPaymentProvider.Password;

        var request = new Concentrator.Payment.Providers.AfterPayMerchant.CreateInvoice();

        request.OrderNumber = invoiceResponse.OrderResponseLines.FirstOrDefault(x => x.OrderLineID.HasValue).OrderLine.Order.WebSiteOrderNumber;
        request.Portfolio = connectorPaymentProvider.Portfolio;

        request.InvoiceDate = invoiceResponse.InvoiceDate.Value;
        request.InvoiceNumber = invoiceResponse.InvoiceDocumentNumber;

        request.InvoiceLines = (from iLine in invoiceResponse.OrderResponseLines
                                let quantity = iLine.Invoiced < 0 ? 0 - iLine.Invoiced : iLine.Invoiced
                                where iLine.OrderLineID.HasValue
                                select new InvoiceLine
                                {
                                  LineNumber = int.Parse(iLine.OrderLine.CustomerOrderLineNr),//web line number;
                                  Quantity = quantity, //quantity to invoice;
                                  //UnitPriceInVat = iLine.OrderLine.Price.HasValue ? (decimal)iLine.OrderLine.Price.Value : 0 //unit price from invoice

                                    UnitPriceInVat = (iLine.VatAmount.HasValue ? iLine.Price + iLine.VatAmount.Value : iLine.Price * (decimal)1.19) / quantity //unit price from invoice
                                }).ToArray();

        var response = client.CreateInvoice(request);


        if (response.Success)
          return true;
        else
        {
          log.AuditError(string.Format("Error processing AfterPay invoice for order {0} error {1}", invoiceResponse.OrderResponseID, response.ErrorMessage), "AfterPay invoice Process");
          return false;
        }
      }
      catch (Exception ex)
      {
        log.Error("Failed Afterpay create invoice", ex);
        return false;
      }
      finally
      {
        client.Close();
      }

    }

    public bool CancelOrder(OrderResponse invoiceResponse, ConnectorPaymentProvider connectorPaymentProvider, AuditLog4Net.Adapter.IAuditLogAdapter log)
    {
      throw new NotImplementedException();
    }
    #endregion
  }
}
