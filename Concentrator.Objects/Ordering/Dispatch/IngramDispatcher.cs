using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mvc.Mailer;
using System.Web;
using System.Net;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Configuration;

namespace Concentrator.Objects.Ordering.Dispatch
{
  class IngramDispatcher : IDispatchable
  {

    public int DispatchOrders(Dictionary<Models.Orders.Order, List<Models.Orders.OrderLine>> orderLines, Models.Vendors.Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, DataAccess.UnitOfWork.IUnitOfWork uni)
    {
      try
      {
        if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["ConcentratorMailerUrl"]))
          throw new Exception("ConcentratorMailerUrl setting empty or not in Config");

        var mailerUrl = new Uri(new Uri(ConfigurationManager.AppSettings["ConcentratorMailerUrl"]), "SendIngramOrder");

        List<IngramMailProduct> productList = new List<IngramMailProduct>();

        foreach (var order in orderLines.Keys)
        {
          foreach (var orderline in orderLines[order])
          {
            IngramMailProduct product = new IngramMailProduct();

            product.CustomItemNumber = orderline.Product.VendorAssortments.FirstOrDefault().CustomItemNumber;
            product.ProductDescription = orderline.Product.VendorAssortments.FirstOrDefault().ShortDescription;
            product.Price = orderline.Price.ToString();
            product.NumberOfProducts = orderline.Quantity.ToString();

            productList.Add(product);
          }


          var data = new IngramMailData
          {
            CustomerName = order.ShippedToCustomer.CustomerName,
            Address = order.ShippedToCustomer.CustomerAddressLine1,
            Email = order.ShippedToCustomer.CustomerEmail,
            PhoneNumber = order.ShippedToCustomer.CustomerTelephone,
            ProductList = productList
          };

          StringBuilder requestString = new StringBuilder();
          XmlWriterSettings settings = new XmlWriterSettings();
          settings.Encoding = Encoding.UTF8;
          using (XmlWriter xw = XmlWriter.Create(requestString, settings))
          {
            XmlSerializer serializer = new XmlSerializer(typeof(IngramMailData));
            XmlSerializerNamespaces nm = new XmlSerializerNamespaces();
            nm.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            serializer.Serialize(xw, data);

            XmlDocument document = new XmlDocument();
            document.LoadXml(requestString.ToString());
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(mailerUrl);
            request.Method = "POST";

            byte[] byteData = UTF8Encoding.UTF8.GetBytes(document.OuterXml);

            using (Stream s = request.GetRequestStream())
            {
              s.Write(byteData, 0, byteData.Length);
            }

            var result = request.GetResponse();

            log.AuditSuccess("The order has been successfully mailed to the customer");

            LogOrder(document, vendor.VendorID, string.Format("{0}.xml", order.OrderID), log);
          }
        }
        return 1;
      }
      catch (Exception e)
      {
        log.Error(e.Message, e.InnerException);

        return 0;
      }

    }

    public void LogOrder(object orderInformation, int vendorID, string fileName, AuditLog4Net.Adapter.IAuditLogAdapter log)
    {
      try
      {
        var logPath = ConfigurationManager.AppSettings["ConcentratorOrderLog"];

        logPath = Path.Combine(logPath, DateTime.Now.ToString("yyyyMMdd"), vendorID.ToString());

        if (!Directory.Exists(logPath))
          Directory.CreateDirectory(logPath);

        ((XmlDocument)orderInformation).Save(Path.Combine(logPath, fileName));
      }
      catch (Exception ex)
      {
        log.AuditError("Failed to log order information for " + vendorID, ex);
      }
    }

    public void GetAvailableDispatchAdvices(Models.Vendors.Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, string logPath, DataAccess.UnitOfWork.IUnitOfWork unit)
    {
      throw new NotImplementedException();
    }

    public void CancelOrder(Models.Vendors.Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, string logPath)
    {
      throw new NotImplementedException();
    }
  }
}
