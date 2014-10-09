using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Ftp;
using System.Data;
using Concentrator.Objects.Write;
using System.IO;
using System.Configuration;

namespace Concentrator.Objects.Ordering.Dispatch
{
  class AmacomDispatcher : IDispatchable
  {
    public int DispatchOrders(Dictionary<Models.Orders.Order, List<Models.Orders.OrderLine>> orderLines, Models.Vendors.Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, DataAccess.UnitOfWork.IUnitOfWork uni)
    {
      var AmacomUrl = vendor.VendorSettings.GetValueByKey("AmacomUrl", string.Empty);
      var OrderPath = vendor.VendorSettings.GetValueByKey("OrderPath", string.Empty);
      var Username = vendor.VendorSettings.GetValueByKey("Username", string.Empty);
      var Password = vendor.VendorSettings.GetValueByKey("Password", string.Empty);
      var Organistion = vendor.VendorSettings.GetValueByKey("Organisation", string.Empty);
      var Fact = vendor.VendorSettings.GetValueByKey("FactDeb", string.Empty);

      FtpManager orderUploader = new FtpManager(
          AmacomUrl,
          OrderPath,
          Username,
          Password,
         false, true, log);

      //Generate CSV

      DataTable table = new DataTable();
      table.Columns.Add("vkorg");
      table.Columns.Add("bstkd_besteldatumt");
      table.Columns.Add("betaler_kunre");
      table.Columns.Add("vastklantnummer_kunnr");
      table.Columns.Add("bstnk");
      table.Columns.Add("posnr");
      table.Columns.Add("kunnr");
      table.Columns.Add("matnr");
      table.Columns.Add("kwmeng");
      table.Columns.Add("vrkme");
      table.Columns.Add("leeg");
      table.Columns.Add("kunwe");
      table.Columns.Add("name1");
      table.Columns.Add("stras");
      table.Columns.Add("pstlz");
      table.Columns.Add("ort01");
      table.Columns.Add("land1");
      table.Columns.Add("kunre");
      table.Columns.Add("email");
      table.Columns.Add("tel");
      
      foreach (var order in orderLines.Keys)
      {
        DataRow row = table.NewRow();
        row["vkorg"] = Organistion;
        row["bstkd_besteldatumt"] = DateTime.Now.ToString("ddMMyyyy");
        row["betaler_kunre"] = Fact;
        row["vastklantnummer_kunnr"] = "";
        table.Rows.Add(row);

        DataRow line = table.NewRow();
        line[0] = order.OrderID.ToString();
        foreach (var orderline in order.OrderLines)
        {
          var customItemNr = orderline.Product.VendorAssortments.FirstOrDefault().CustomItemNumber;

          if (customItemNr.Length < 18)
          {
            customItemNr = "";
            for (int i = orderline.Product.VendorAssortments.FirstOrDefault().CustomItemNumber.Length; i < 18; i++)
            {
              customItemNr += "0";
            }
            customItemNr += orderline.Product.VendorAssortments.FirstOrDefault().CustomItemNumber;
          }

          line[1] = orderline.OrderLineID.ToString();
          line[2] = "";
          line[3] = customItemNr;
          line[4] = orderline.GetDispatchQuantity().ToString();
          line[5] = "";
          line[6] = "";
          line[7] = "";
          line[8] = order.ShippedToCustomer.CustomerName;
          line[9] = order.ShippedToCustomer.CustomerAddressLine1;
          line[10] = order.ShippedToCustomer.PostCode;
          line[11] = order.ShippedToCustomer.City;
          line[12] = order.ShippedToCustomer.Country;
          line[14] = "";
          line[14] = order.ShippedToCustomer.CustomerEmail;
          line[15]= order.ShippedToCustomer.CustomerTelephone;

          table.Rows.Add(line);
          line = table.NewRow();
        }

        CsvWriter csvWriter = new CsvWriter(table, '\t');
        var csv = csvWriter.AsString();
        //edit new line

        csv = csv.Substring(0, 60) + Environment.NewLine + csv.Substring(61);

        System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

        var fileName = string.Format("BasOrder_{0}.csv", order.OrderID.ToString());
        byte[] file = encoding.GetBytes(csv);
        MemoryStream stream = new MemoryStream(file);
        //Place order on FTP 
        orderUploader.Upload(stream, fileName);

        LogOrder(csv, vendor.VendorID, fileName, log);

      }

      throw new NotImplementedException();
    }

    public void LogOrder(object orderInformation, int vendorID, string fileName, AuditLog4Net.Adapter.IAuditLogAdapter log)
    {
      try
      {
        var logPath = ConfigurationManager.AppSettings["ConcentratorOrderLog"];

        logPath = Path.Combine(logPath, DateTime.Now.ToString("yyyyMMdd"), vendorID.ToString());

        if (!Directory.Exists(logPath))
          Directory.CreateDirectory(logPath);

        using (StreamWriter writer = new StreamWriter(Path.Combine(logPath, fileName)))
        {
          writer.Write((string)orderInformation);
        }
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
