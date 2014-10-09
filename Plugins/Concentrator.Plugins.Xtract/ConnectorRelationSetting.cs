using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Enumerations;
using System.IO;
using System.Configuration;
using Concentrator.Objects.Ftp;
using System.Data.SqlClient;

namespace Concentrator.Plugins.Xtract
{
  public enum XtractCatalogTypes
  {
    Products,
    ProductAttributes,
    ProductImages,
    ProductContent,
    Prices,
    Stock,
    ProductGroups,
    Brands,
    Freegoods,
    Accruel,
    Insurance
  }

  public class ConnectorRelationSetting : ConcentratorPlugin
  {
    public Configuration _config { get; set; }

    public override string Name
    {
      get { return "Xtract Connector Plugin"; }
    }

    protected override void Process()
    {
      log.Info("Start Xtract ConnectorRelation process");

      _config = GetConfiguration();

      string jdeConnectionString = ConfigurationManager.ConnectionStrings["JDE"].ConnectionString;

      using (var unit = GetUnitOfWork())
      {
        var connectorRelations = (from r in unit.Scope.Repository<ConnectorRelation>().GetAll(x => x.UseFtp.HasValue && x.UseFtp.Value && x.ProviderType.HasValue && x.XtractType.HasValue)
                                  select r).ToList();

        connectorRelations.ForEach(x =>
        {
          try
          {
            string password = string.Empty;
            using (SqlConnection connection = new SqlConnection(jdeConnectionString))
            {
              connection.Open();

              string passwordQuery = @"SELECT     WPPH1
FROM         F0115
WHERE     (WPPHTP = 'PAS') AND (WPAN8 = {0})";
              using (SqlCommand command = new SqlCommand(string.Format(passwordQuery, x.CustomerID), connection))
              {
                command.CommandTimeout = 3600;
                password = (string)command.ExecuteScalar();
                password = password.Trim();
              }
            }

            if (!string.IsNullOrEmpty(password))
            {
              switch ((XractTypeEnum)x.XtractType.Value)
              {
                case XractTypeEnum.WebshopPriceList:
                  if (x.ProviderType.Value == (int)ProviderTypeEnum.ByCsv)
                  {
                    Enum.GetNames(typeof(XtractCatalogTypes)).ForEach((name, idx) =>
                    {
                      string url = string.Format(_config.AppSettings.Settings["PriceListCSV"].Value, "CustomerID=" + x.CustomerID, "&Password=" + password + "&catalogType=" + name);
                      PublishFile(x, url, string.Format("WebshopPriceList_CSV_{0}.csv", name));
                    });
                  }
                  else if (x.ProviderType.Value == (int)ProviderTypeEnum.ByExcel)
                  {
                    Enum.GetNames(typeof(XtractCatalogTypes)).ForEach((name, idx) =>
                    {
                      string url = string.Format(_config.AppSettings.Settings["PriceListExcel"].Value, "CustomerID=" + x.CustomerID, "&Password=" + password + "&catalogType=" + name);
                      PublishFile(x, url, string.Format("WebshopPriceList_Excel_{0}.xls", name));
                    });
                  }
                  else if (x.ProviderType.Value == (int)ProviderTypeEnum.ByXml)
                  {

                  }
                  break;
                case XractTypeEnum.FullPriceList:
                  if (x.ProviderType.Value == (int)ProviderTypeEnum.ByCsv)
                  {
                    string url = string.Format(_config.AppSettings.Settings["PriceListCSV"].Value, "CustomerID=" + x.CustomerID, "&Password=" + password);
                    PublishFile(x, url, "FullPriceList_CSV.csv");
                  }
                  else if (x.ProviderType.Value == (int)ProviderTypeEnum.ByExcel)
                  {
                    string url = string.Format(_config.AppSettings.Settings["PriceListExcel"].Value, "CustomerID=" + x.CustomerID, "&Password=" + password);
                    PublishFile(x, url, "FullPriceList_Excel.xls");
                  }
                  else if (x.ProviderType.Value == (int)ProviderTypeEnum.ByXml)
                  {

                  }
                  break;
                default:
                  break;
              }
            }
            else
              log.InfoFormat("Empty password for connectorrelation {0}", x.ConnectorRelationID);
          }
          catch (Exception ex)
          {
            log.Error("Error xtract connector for connectorrelation" + x.ConnectorRelationID ,ex);
          }
        });

      }
      log.Info("Finish Xtract ConnectorRelation process");
    }

    private void PublishFile(ConnectorRelation connector, string fileUrl, string fileName)
    {
      switch ((FtpTypeEnum)connector.FtpType.Value)
      {
        case FtpTypeEnum.Customer:
          var ftp = new FtpManager(connector.FtpAddress, string.Empty, connector.Username, connector.FtpPass,
              false, connector.FtpType.HasValue ? (FtpConnectionTypeEnum)connector.FtpType.Value == FtpConnectionTypeEnum.Passive : false, log);

          fileName = String.Format("{0}_{1}", DateTime.Now.ToString("yyyyMMddhhmmss"), fileName);

          using (var wc = new System.Net.WebClient())
          {
            using (var stream = new MemoryStream(wc.DownloadData(fileUrl)))
            {
              ftp.Upload(stream, fileName);
            }
          }
          break;
        case FtpTypeEnum.Xtract:
          var xtractPath = Path.Combine(_config.AppSettings.Settings["XtractFtpPath"].Value, connector.CustomerID);
          DownloadFile(fileUrl, xtractPath, fileName);
          break;
      }
    }

    private void DownloadFile(string url, string destination, string name)
    {
      try
      {
        //name = url.Substring(url.LastIndexOf('.'));
        var file = Path.Combine(destination, name);

        using (var wc = new System.Net.WebClient())
        {
          //wc.DownloadFile(url, file);
        }
      }
      catch (Exception excep)
      {
        log.Error("Error downloading file", excep);
      }
    }
  }
}
