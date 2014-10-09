using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects;
using System.Xml.Linq;
using Concentrator.Objects.Ftp;
using System.IO;

namespace Concentrator.Plugins.Dmis
{
  public class DmisUtilty
  {
    public static bool UploadFiles(Connector connector, MemoryStream inStream, string name, AuditLog4Net.Adapter.IAuditLogAdapter log)
    {
      string ftpAddress = connector.ConnectorSettings.GetValueByKey<string>("FtpAddress", null);
      string ftpUserName = connector.ConnectorSettings.GetValueByKey<string>("FtpUserName", null);
      string ftpPassword = connector.ConnectorSettings.GetValueByKey<string>("FtpPassword", null);
      string ftpPath = connector.ConnectorSettings.GetValueByKey<string>("FtpPath", null);

      if (!string.IsNullOrEmpty(ftpAddress)
&& !string.IsNullOrEmpty(ftpUserName)
&& !string.IsNullOrEmpty(ftpPassword)
&& !string.IsNullOrEmpty(ftpPath))
      {
        var ftp = new FtpManager(ftpAddress, ftpPath, ftpUserName, ftpPassword, false, false, log);
        ftp.Upload(inStream, name);
        return true;
      }
      else
      {
        log.AuditError(string.Format("No ftp settings availible for Dmis connector {0}", connector.Name));
        return false;
      }
    }

    internal static string GetCustomItemNumber(string customItemNumber, string ConcentratorProductID)
    {
      return customItemNumber;
    }
  }
}
