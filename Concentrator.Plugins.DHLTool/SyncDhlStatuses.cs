using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Plugins.DhlTool.Helpers;

namespace Concentrator.Plugins.DhlTool
{
  public class SyncDhlStatuses : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Sync DHL Statuses to Magento"; }
    }

    protected override void Process()
    {
      foreach (var connector in base.Connectors.Where(c => ((ConnectorType)c.ConnectorType).Has(ConnectorType.DhlTool)))
      {
        using (var helper = new MySqlHelper(connector.Connection))
        {
          List<DhlStatus> statuses = GetStatusesFromWms();

          foreach (var status in statuses)
          {
            if (status.success)
              helper.UpdateStatusSuccessInMagento(status);
            else
              helper.UpdateStatusFailureInMagento(status);
          }
        }
      }
    }

    private List<DhlStatus> GetStatusesFromWms()
    {
      var config = GetConfiguration();
      var dhlInFolder = config.AppSettings.Settings["DhlInFolder"].Value;
      var dhlInProcessedFolder = config.AppSettings.Settings["dhlInProcessedFolder"].Value;

      List<DhlStatus> dhlStatuses = new List<DhlStatus>();

      foreach (var file in Directory.GetFiles(dhlInFolder, "*.xml"))
      {
        try
        {
          var xml = XDocument.Load(file);

          dhlStatuses = (from status in xml.Root.Elements("shipment")
                         select new DhlStatus
                         {
                           ggb = status.Element("ggb").Value,
                           success = bool.Parse(status.Element("success").Value),
                           shipmentNumber = status.Element("shipmentNumber").Value
                         }).ToList();
        }
        catch (Exception e)
        {
          File.Move(file, file + ".err");
          var logFile = file + ".log";

          if (!File.Exists(logFile))
            File.Create(logFile);

          using (StreamWriter sw = new StreamWriter(logFile))
          {
            sw.Write(e.Message);
          }
        }

        File.Move(file, dhlInProcessedFolder + "\\" + Path.GetFileName(file));
      }

      return dhlStatuses;
    }
  }
}
