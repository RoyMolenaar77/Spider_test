using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Excel;
using System.Configuration;
using Concentrator.Objects.Web;
using System.IO;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Microsoft.Practices.ServiceLocation;
using Concentrator.Objects.Models.Products;
using System.Data;
using Concentrator.Objects.Utility;
using System.Net.Mail;

namespace Concentrator.Plugins.VendorPrices
{
  class ExcelProcessor : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Process excel files"; }
    }

    protected override void Process()
    {
      using (var work = GetUnitOfWork())
      {
        var filepath = ConfigurationManager.AppSettings["ExcelPath"];
        var backupFilePath = ConfigurationManager.AppSettings["ExcelBackupPath"];
        var emailAddress=ConfigurationManager.AppSettings["ConcentratorMail"];
        if ((!String.IsNullOrEmpty(filepath)) && (!String.IsNullOrEmpty(backupFilePath)))
        {
          List<string> excelFiles = scanDirectory(filepath).Select(x=>Path.GetFileName(x)).ToList();
          foreach (var excelFile in excelFiles)
          {
            try
            {
              makeBackup(filepath, backupFilePath, excelFile);
              importExcelToDb(Path.Combine(filepath, excelFile));
              mailSuccess(emailAddress, excelFile);
              removeOriginalFile(filepath, excelFile);
            }
            catch (Exception e)
            {
              mailFailure(emailAddress, excelFile, e.Message, e.StackTrace);
            }
          }
        }
      }
    }

    private void removeOriginalFile(string filepath, string excelFile)
    {
      System.IO.File.Delete(System.IO.Path.Combine(filepath, excelFile));
    }

    private void mailFailure(string emailAddress, string excelFile, string message, string stackTrace)
    {
      MailMessage m = new MailMessage();
      var body = String.Format("File {0} processing failed on {1}\nMessage:{2}\nStacktrace:{3}", excelFile, DateTime.Now,message,stackTrace);
      m.Subject = String.Format("File {0} failed to process", excelFile);
      m.To.Add(new MailAddress(emailAddress));
      m.Body = body;
      EmailDaemon d = new EmailDaemon(log);
      d.SendMail(emailAddress, m);
    }

    private void mailSuccess(string emailAddress, string excelFile)
    {
      MailMessage m = new MailMessage();
      m.Subject = String.Format("File {0} processed successfully", excelFile);
      m.Body=String.Format("File {0} processed successfully on {1}", excelFile, DateTime.Now);
      m.To.Add(new MailAddress(emailAddress));
      EmailDaemon d = new EmailDaemon(log);
      d.SendMail(emailAddress, m);
    }

    private void makeBackup(string originalFilePath,string backupFilePath, string excelFile)
    {
      var dateExtensions = createDateExtension();
      var newFileName = makeFileName(excelFile, dateExtensions);
      File.Copy(Path.Combine(originalFilePath, excelFile), Path.Combine(backupFilePath, newFileName),true);
    }

    private List<string> scanDirectory(string filepath)
    {
      return Directory.GetFiles(filepath).Where(x => x.EndsWith(".xlsx")).ToList();
    }

    private string makeFileName(string filename, string fileDateExtension)
    {
      string[] elements = filename.Split(new char[] { '.' });
      string extension = elements[elements.Length - 1];
      List<string> pathElements = new List<string>();
      for (var i = 0; i < elements.Length - 1; i++)
      {
        pathElements.Add(elements[i]);
      }
      pathElements.Add(fileDateExtension);
      pathElements.Add(extension);
      return String.Join(".", pathElements);
    }

    private string createDateExtension()
    {
      var d = DateTime.Now;
      var monthString = d.Month.ToString();
      if (d.Month < 10)
      {
        monthString = String.Format("0{0}", monthString);
      }
      var dayString = d.Day.ToString();
      if (d.Day < 10)
      {
        dayString = String.Format("0{0}", dayString);
      }
      var result = String.Format("{0}{1}{2}", d.Year.ToString(), monthString, dayString);
      return result;
    }
    private void importExcelToDb(string fullName)
    {
      using (var unit = GetUnitOfWork())
      {
        //var currentConnectorID = Client.User.ConnectorID;
        //var currentConnectorID = 1;
        if (System.IO.File.Exists(fullName))
        {
          FileStream excelStream = System.IO.File.Open(fullName, FileMode.Open, FileAccess.Read);
          IExcelDataReader excelReader = null;
          excelReader = ExcelReaderFactory.CreateOpenXmlReader(excelStream);

          var ds = excelReader.AsDataSet();
          var temp = (from p in ds.Tables[0].AsEnumerable().Skip(2) select p).FirstOrDefault();
          var content = (from p in ds.Tables[0].AsEnumerable().Skip(2)
                         let numberOfFields = p.ItemArray.Count()
                         select new
                         {
                           connectorID = Convert.ToInt32(p.Field<object>(numberOfFields - 1)),
                           vendorID = Convert.ToInt32(p.Field<object>(numberOfFields - 2)),
                           productID = Convert.ToInt32(p.Field<object>(numberOfFields - 3)),
                           Label = Convert.ToString(p.Field<object>(numberOfFields - 4)),
                           Price = Convert.ToDecimal(p.Field<object>(numberOfFields - 5)),
                           toDate = DateTime.FromOADate(Convert.ToDouble(p.Field<object>(numberOfFields - 6))),
                           fromDate = DateTime.FromOADate(Convert.ToDouble(p.Field<object>(numberOfFields-7)))
                         }).ToList();
          //insert the content into the db
          foreach (var c in content)
          {
            var rows = unit.Scope.Repository<ContentPrice>().GetAll().Where(x => (x.ProductID == c.productID) && (x.ConnectorID == c.connectorID) && (x.VendorID == c.vendorID)).ToList();
            if (rows != null)
            {
              if (rows.Count != 0)
              {
                foreach (var r in rows)
                {
                  if (!String.IsNullOrEmpty(c.Label))
                  {
                    r.ContentPriceLabel = c.Label;
                    r.FromDate = c.fromDate;
                    r.ToDate = c.toDate;
                    r.FixedPrice = c.Price;
                  }
                }
              }
              else
              {
                //create a new row
                var product = unit.Scope.Repository<Product>().GetAll().Where(x => x.ProductID == c.productID).SingleOrDefault();
                if (product != null)
                {
                  // find the brandid
                  var brandID = product.BrandID;
                  var ContentPriceRuleIndex = 4;
                  var PriceRuleType = 1;
                  var Margin = "+";
                  var VendorID = Convert.ToInt32(c.vendorID);

                  ContentPrice cp = new ContentPrice() { VendorID = VendorID, ConnectorID = c.connectorID, BrandID = brandID, ProductID = product.ProductID, Margin = Margin, CreatedBy = Client.User.UserID, CreationTime = DateTime.Now, ContentPriceRuleIndex = ContentPriceRuleIndex, PriceRuleType = PriceRuleType, FromDate = c.fromDate, ToDate = c.toDate, FixedPrice = c.Price, ContentPriceLabel = c.Label };
                  unit.Scope.Repository<ContentPrice>().Add(cp);

        //          //unit.Save();
                }

              }
               
            }

          }
          unit.Save();
        }
      }
    }
  }
    
}
