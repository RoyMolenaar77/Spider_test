using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using MySql.Data.MySqlClient;
using System.IO;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects;
using System.Net;
using Concentrator.Objects.Constant;

namespace Concentrator.Plugins.Magento
{
  [ConnectorSystem(Constants.Connectors.ConnectorSystems.Magento)]
  public abstract class MagentoBasePlugin : ConcentratorPlugin
  {


    protected void SyncMagentoDatabase()
    {
      log.Info("Starting database syncing");


      var config = GetConfiguration();
      //run query for each magento connector to update database to required version 
      foreach (var connector in base.Connectors)
      {
        if (string.IsNullOrEmpty(connector.Connection)) continue;

        //update structure of the database
        using (MySqlConnection connection = new MySqlConnection(connector.Connection))
        {
          using (MySqlCommand command = connection.CreateCommand())
          {

            var commandQuery = File.ReadAllText(config.AppSettings.Settings["MagentoDatabaseStructuralScripts"].Value);

            command.CommandText = commandQuery;
            log.DebugFormat("Try execute: {0}", command.CommandText);

            try
            {
              connection.Open();
              command.ExecuteScalar();
              log.InfoFormat("Executed sync scripts for database with connection {0}", connector.Connection);
            }
            catch (MySqlException ex)
            {
              log.AuditError("Syncing magento database failed for db with connection " + connector.Connection, ex, Name);
            }
          }
        }
      }
    }

    protected void RefreshMagentoIndex(Connector connector)
    {
      try
      {
        var url = connector.ConnectorSettings.GetValueByKey("MagentoIndexUrl", string.Empty);
        if (!string.IsNullOrEmpty(url))
        {
          log.InfoFormat("Start Call index refresh");
          WebRequest req = WebRequest.Create(url);
          using (WebResponse resp = req.GetResponse())
          {
            log.AuditInfo("Index Url called for " + connector.Name + " returned:" + (((HttpWebResponse)resp).StatusDescription));
          }
          log.InfoFormat("Finish Call index refresh");
        }
      }
      catch (Exception ex)
      {
        log.Error("Error call index refresh", ex);
      }
    }

    public abstract override string Name { get; }

    protected abstract override void Process();
  }


}
