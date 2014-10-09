using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.ZipUtil;
using System.IO;

namespace Concentrator.Plugins.VSN
{
  public class ImageImportHistory : ImageImportBase
  {
    private const string _name = "VSN Image History";

    public override string Name
    {
      get { return _name; }
    }

    protected override void Process()
    {
      var config = GetConfiguration().AppSettings.Settings;

      var ftp = new FtpManager(config["VSNFtpUrl"].Value, "pub2/images/history/",
              config["VSNUser"].Value, config["VSNPassword"].Value, false, true, log);

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
            //oNetDrive.MapDrive("diract", "D1r@ct379");
            oNetDrive.MapDrive();
            drive = "Z:";
            drive = Path.Combine(drive, "Concentrator");
          }
          catch (Exception err)
          {
            log.AuditError("Invalid network drive", err);
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

                ProcessImageFile(file, unit, drive);

              }
              unit.Save();
            }
          }
          catch (Exception ex)
          {
            log.AuditError(string.Format("Error import VSN image file {0}", zipFile.FileName), ex);
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
            log.AuditError("Error unmap drive" + err.InnerException);
          }
          oNetDrive = null;
        }
      }
      log.AuditComplete("Finished history VSN image import", "VSN Image Import");
    }
  }
}
