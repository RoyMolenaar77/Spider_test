using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.ZipUtil;
using System.IO;

namespace Concentrator.Plugins.CentraalBoekhuis
{
  public class MediaImportFull : MediaImportBase
  {
    private const string _name = "Centraal Boekhuis Media Full";

    protected override Configuration Config
    {
      get { return GetConfiguration(); }
    }

    protected override int VendorID
    {
      get { return int.Parse(Config.AppSettings.Settings["VendorID"].Value); }
    }

    protected override int DefaultVendorID
    {
      get { return int.Parse(Config.AppSettings.Settings["DefaultVendorID"].Value); }
    }

    public override string Name
    {
      get { return _name; }
    }

    public void Process()
    {
      var config = GetConfiguration().AppSettings.Settings;

      var subFolderPath = Path.Combine(config["CentraalBoekhuisCoverPath"].Value, config["CentraalBoekhuisInitialPath"].Value);

      FtpManager ftp = new FtpManager(config["CentraalBoekhuisFtpUrl"].Value, subFolderPath,
                     config["CentraalBoekhuisUser"].Value, config["CentraalBoekhuisPassword"].Value, false, true, log);

      using (var unit = GetUnitOfWork())
      {
        string drive = GetConfiguration().AppSettings.Settings["FTPImageDirectory"].Value;

        bool networkDrive = false;
        bool.TryParse(GetConfiguration().AppSettings.Settings["IsNetworkDrive"].Value, out networkDrive);

        if (networkDrive)
        {
          NetworkDrive oNetDrive = new NetworkDrive();
          try
          {
            oNetDrive.LocalDrive = "Z:";
            oNetDrive.ShareName = drive;
            oNetDrive.MapDrive();
            drive = "Z:";
            drive = Path.Combine(drive, "Concentrator");
          }
          catch (Exception err)
          {
            log.Error("Invalid network drive", err);
          }
          oNetDrive = null;
        }

        foreach (var zipFile in ftp)
        {
          try
          {
            using (var zipProc = new ZipProcessor(zipFile.Data))
            {
              foreach (var file in zipProc)
              {
                ProcessMediaFile(file, unit, drive);
              }
              unit.Save();
            }
          }
          catch (Exception ex)
          {
            log.AuditError(string.Format("Error import Centraal boekhuis media file {0}", zipFile.FileName), ex);
          }
        }

        if (networkDrive)
        {
          NetworkDrive oNetDrive = new NetworkDrive();
          try
          {
            oNetDrive.LocalDrive = drive;
            oNetDrive.UnMapDrive();
          }
          catch (Exception err)
          {
            log.Error("Error unmap drive" + err.InnerException);
          }
          oNetDrive = null;
        }
      }
      log.AuditComplete("Finished full Centraal Boekhuis image import", "Centraal Boekhuis Image Import");
    }

    protected override void SyncProducts()
    {
      Process();
    }

   
  }
}
