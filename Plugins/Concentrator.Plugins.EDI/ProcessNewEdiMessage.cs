using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Concentrator.Objects.EDI;
using System.Configuration;
using System.IO;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.EDI.Enumerations;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Utility;

namespace Concentrator.Plugins.EDI
{
  public class ProcessNewEdiMessage : ConcentratorEDIPlugin
  {
    public override string Name
    {
      get { return "EDI process messages"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          if (Config.AppSettings.Settings["EdiOrderDir"] != null)
          {

            var importpath = Config.AppSettings.Settings["EdiOrderDir"].Value;

            foreach (var file in Directory.GetFiles(importpath))
            {
              FileInfo inf = new FileInfo(file);
              log.InfoFormat("Found file with extension", inf.Extension);
              if (inf.Extension == ".xls" || inf.Extension == ".xlsx")
              {
                log.Info("Found excel order");
                ExcelReader excel = new ExcelReader(importpath, log, unit);
                excel.ProcessFile(file, "OrderDir");
              }
              else
              {
                using (StreamReader reader = new StreamReader(file))
                {
                  EdiOrderListener ediListener = new EdiOrderListener()
                  {
                    CustomerName = "new",
                    CustomerIP = "Unknown",
                    CustomerHostName = "Directory order",
                    RequestDocument = reader.ReadToEnd(),
                    Processed = false,
                    ReceivedDate = DateTime.Now,
                    ConnectorID = int.Parse(Config.AppSettings.Settings["DefaultConnectorID"].Value)
                  };
                  unit.Scope.Repository<EdiOrderListener>().Add(ediListener);

                  unit.Save();
                }
              }
              var archivePath = Path.Combine(importpath, "Processed");

              if (!Directory.Exists(archivePath))
                Directory.CreateDirectory(archivePath);

              FileInfo fInf = new FileInfo(file);

              if (File.Exists(Path.Combine(archivePath, fInf.Name)))
                File.Delete(Path.Combine(archivePath, fInf.Name));

              File.Move(file, Path.Combine(archivePath, fInf.Name));
            }
          }
        }
        catch (Exception ex)
        {
          log.AuditError("Failed to load files from directory");

        }

        var orders = (from o in unit.Scope.Repository<EdiOrderListener>().GetAll()
                      where o.Processed == false
                      && o.ErrorMessage == null
                      select o).ToList();

        foreach (var order in orders)
        {
          try
          {
            string type = ediProcessor.DocumentType(order.RequestDocument);

            if (!string.IsNullOrEmpty(type))
            {
              var messageType = ediOrderTypes.FirstOrDefault(x => x.Name == type);

              if (messageType != null)
              {
                order.Processed = true;

                var ediOrders = ediProcessor.ProcessOrder(type, order.RequestDocument, order.ConnectorID, order.EdiRequestID, unit);
                foreach (var ediOrder in ediOrders)
                {
                  var o = unit.Scope.Repository<EdiOrder>().GetSingle(x => x.EdiOrderID == ediOrder.EdiOrderID);
                  o.Status = (int)EdiOrderStatus.Validate;
                }
              }
              else
              {
                string error = string.Format("Messagetype {0} does not exists in Database", type);
                log.AuditError(error, "EDI New Messages");
                order.ErrorMessage = error;
              }
            }
          }
          catch (Exception ex)
          {
            log.AuditError(string.Format("Failed to process order {0}", order.EdiRequestID), ex, "Process New EDI orders");
            order.ErrorMessage = ex.Message;
          }
          finally
          {
            unit.Save();
          }
        }
      }
    }

    public override Configuration Config
    {
      get { return GetConfiguration(); }
    }
  }
}
