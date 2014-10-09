using System;
using System.Configuration;
using Concentrator.Objects;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.ConcentratorService;
using System.IO;


namespace Concentrator.Plugins.Sapph
{
  public class ProcessInformationToTNT : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Sapph passthrough product information to TNT"; }
    }

    protected override void Process()
    {
      try
      {
        Vendor vendor;

        using (var unit = GetUnitOfWork())
        {
          vendor = unit.Scope.Repository<Vendor>().GetSingle(x => x.Name.Equals("Sapph"));
        }

        string axaptaFtpSetting = vendor.VendorSettings.GetValueByKey("AxaptaftpProductInformationSettingsURI", string.Empty);
        string axaptaMoveFtpSetting = vendor.VendorSettings.GetValueByKey("AxaptaMoveftpProductInformationSettingsURI", string.Empty);
        string TNTFtpSetting = vendor.VendorSettings.GetValueByKey("TNTftpProductInformationSettings", string.Empty);

        if (string.IsNullOrEmpty(axaptaFtpSetting) || string.IsNullOrEmpty(TNTFtpSetting) || string.IsNullOrEmpty(axaptaMoveFtpSetting))
        {
          log.AuditError("FTP information is missing! Please check the settings");

          throw new ConfigurationErrorsException();
        }

        var AxaptaFtpManager = new Concentrator.Objects.Ftp.FtpManager(axaptaFtpSetting, log, usePassive:true);
        var AxaptaMoveFtpManager = new Concentrator.Objects.Ftp.FtpManager(axaptaMoveFtpSetting, log, usePassive: true);
        var TNTftpManager = new Concentrator.Objects.Ftp.FtpManager(TNTFtpSetting, log, usePassive: true);


        foreach (var completeFileName in AxaptaFtpManager.GetFiles())
        {
          string fileName = Path.GetFileName(completeFileName);

          if (!fileName.Equals("Processed"))
          {
            try
            {
              log.InfoFormat("Processing file {0}", fileName);

              using (Stream stream = AxaptaFtpManager.Download(fileName))
              {
                TNTftpManager.Upload(stream, Path.GetFileName(fileName));
                AxaptaMoveFtpManager.Upload(stream, fileName);
                AxaptaFtpManager.Delete(fileName);

              }
            }
            catch (Exception e)
            {
              log.ErrorFormat("Could not process file {0}! Error: {1}", fileName, e.Message);
            }
          }
        }
      }
      catch (Exception e)
      {
        log.AuditError("Sapph passthrough product information to TNT Failed", e);
      }
    }
  }
}
