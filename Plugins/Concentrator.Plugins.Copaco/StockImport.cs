using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Ftp;
using System.Data;
using System.Transactions;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Vendors;
using System.IO;

namespace Concentrator.Plugins.Copaco
{
  class StockImport : ConcentratorPlugin
  {
    int? VendorID = null;
    public override string Name
    {
      get { return "Copaco Stock Import"; }
    }

    protected override void Process()
    {
      try
      {
        var config = GetConfiguration();

        VendorID = Int32.Parse(config.AppSettings.Settings["VendorID"].Value);

        DataTable stockFile = new DataTable();

        //FtpManager productDownloader = new FtpManager(
        //  config.AppSettings.Settings["CopacoProductFtpUrl"].Value,
        //  config.AppSettings.Settings["CopacoProductPath"].Value,
        //  config.AppSettings.Settings["CopacoUserName"].Value,
        //  config.AppSettings.Settings["CopacoPassword"].Value,
        // true, true, log);//new FtpDownloader("test/");

        FtpManager contentDownloader = new FtpManager(
         config.AppSettings.Settings["CopacoFtpUrl"].Value,
         config.AppSettings.Settings["CopacoContentPath"].Value,
         config.AppSettings.Settings["CopacoContentUserName"].Value,
         config.AppSettings.Settings["CopacoContentPassword"].Value,
        false, true, log);//new FtpDownloader("test/");

        var file = contentDownloader.OpenFile("COPACO_ATP.CSV");
        log.InfoFormat("Processing file: {0}", file.FileName);
        using (file)
        {
          try
          {
            stockFile = CreateDataTable(file.Data, false, ',');
          }
          catch (Exception ex)
          {
            log.AuditError(String.Format("Failed to load xml for file: {0}", file.FileName), ex);

          }
        }


        try
        {
          using (var unit = GetUnitOfWork())
          {
            if (stockFile != null)
            {
              ProcessFile(unit, stockFile);
            }
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromMinutes(2)))
            {
              unit.Save();
              ts.Complete();
            }
          }
        }
        catch (Exception ex)
        {
          log.AuditError("Error import stock for products", ex);
        }


      }
      catch (Exception ex)
      {
        log.AuditError("Error downloading/processing file");
      }
    }

    private DataTable CreateDataTable(Stream File, bool includeHeader, char delimiter)
    {
      DataTable table = new DataTable();
      using (System.IO.StreamReader csv_file = new StreamReader(File))
      {
        int rows = 0;
        while (csv_file.Peek() >= 0)
        {
          string line = csv_file.ReadLine().Replace(@"\""", "");
          string[] vals = line.Split(delimiter);
          for (int i = 0; i < vals.Length; i++)
          {
            if (table.Columns.Count < vals.Length)
            {
              table.Columns.Add(new DataColumn());
            }
            vals[i] = vals[i].Trim('"');
          }
          if (rows != 0 && !includeHeader)
          {
            table.Rows.Add(vals);
          }
          rows++;
        }
      }
      return table;
    }

    private void ProcessFile(IUnitOfWork unit, DataTable stockFile)
    {

      var repoAssortment = unit.Scope.Repository<VendorAssortment>();
      var repoStock = unit.Scope.Repository<VendorStock>();

      var stockData = (from data in stockFile.AsEnumerable()
                       select new
                       {
                         CustomItemNumber = data[0].ToString(),
                         Stock = data[2].ToString()
                       }).ToList();

      foreach (var productStock in stockData)
      {
        //get productID in vendorStockAssortment
        var productID = repoAssortment.GetSingle(vs => vs.CustomItemNumber == productStock.CustomItemNumber && vs.VendorID == VendorID).Try<VendorAssortment, int?>(c => c.ProductID, null);

        //if found
        if (productID.HasValue && productID.Value > 0)
        {
          var vendorStock = repoStock.GetSingle(st => st.ProductID == productID);

          try
          {
            if (vendorStock == null)
            {
              vendorStock = new VendorStock
              {
                VendorID = VendorID.Value,
                ProductID = productID.Value,
                VendorStockTypeID = 1
              };
              repoStock.Add(vendorStock);
            }
            vendorStock.QuantityOnHand = Int32.Parse(productStock.Stock);
            vendorStock.ConcentratorStatusID = -1;


          }
          catch (Exception ex)
          {
            log.AuditError(string.Format("Error update stock for product {0}, error {1}", productID, ex.InnerException));
          }
        }
      }
    }

  }
}
