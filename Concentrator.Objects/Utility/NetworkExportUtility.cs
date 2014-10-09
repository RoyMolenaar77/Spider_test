using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using AuditLog4Net.Adapter;

namespace Concentrator.Objects
{
  public class NetworkExportUtility
  {
    private IAuditLogAdapter _logger;

    public NetworkExportUtility() : this(null) { }

    public NetworkExportUtility(IAuditLogAdapter logger)
    {
      _logger = logger;
    }

    public string ConnectorNetworkPath(string drive, string localDrive)
    {
      return ConnectorNetworkPath(drive, localDrive, null, null);
    }

    public string ConnectorNetworkPath(string drive, string localDrive, string userName, string password)
    {
      NetworkDrive oNetDrive = new NetworkDrive();
      try
      {

        oNetDrive.LocalDrive = localDrive;
        oNetDrive.ShareName = drive;



        if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
        {
          oNetDrive.MapDrive(userName, password);
        }
        else
          oNetDrive.MapDrive();

        drive = localDrive;
      }
      catch
      {
        //if (_logger != null)
        //{
        //  _logger.AuditError("Error mapping drive", err);
        //}
      }
      oNetDrive = null;
      return drive;
    }

    public void DisconnectNetworkPath(string drive)
    {
      NetworkDrive oNetDrive = new NetworkDrive();
      try
      {
        oNetDrive.LocalDrive = drive;
        oNetDrive.UnMapDrive();
      }
      catch
      {
        //if (_logger != null)
        //{
        //  _logger.AuditError("Error unmapping drive", err);
        //}
      }
      oNetDrive = null;
    }
  }
}
