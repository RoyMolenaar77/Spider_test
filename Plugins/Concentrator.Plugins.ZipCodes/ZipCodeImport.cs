using System;
using Concentrator.Objects.ConcentratorService;
using System.IO;
using System.Linq;
using Concentrator.Objects;
using Concentrator.Objects.Sftp;
using Concentrator.Objects.ZipUtil;
using System.Transactions;
using System.Configuration;
using Concentrator.Objects.Models.Orders;

namespace Concentrator.Plugins.ZipCodes
{
  public class ZipCodeImport : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "ZipCode import"; }
    }

    protected override void Process()
    {
      log.Info("ZipCode Import started");

      var config = GetConfiguration();

      string session = config.AppSettings.Settings["WinscpSession"].Value;
      string logPath = config.AppSettings.Settings["WinscpLogPath"].Value;
      string applicationPath = config.AppSettings.Settings["WinscpAppPath"].Value;
      string downloadLocation = config.AppSettings.Settings["WinscpDownloadPath"].Value;

      using (var unit = GetUnitOfWork())
      {

        using (WinSCPDownloader downloader = new WinSCPDownloader(session, logPath, applicationPath, downloadLocation, "*.zip"))
        {
          log.Debug("Connected to Cendris.");
          foreach (var remoteFile in downloader)
          {
            log.DebugFormat("Try process file {0}", remoteFile.FileName);
            using (ZipProcessor processor = new ZipProcessor(remoteFile.Data))
            {
              foreach (var zippedFile in processor)
              {
                using (StreamReader sr = new StreamReader(zippedFile.Data))
                {
                  int linecount = 0;
                  int logline = 0;

                  string line;
                  while ((line = sr.ReadLine()) != null)
                  {
                    linecount++;
                    if (linecount <= 1 || line.StartsWith("**"))
                      continue;

                    #region Parse PostCode
                    ZipCode newzip = new ZipCode()
                    {
                      PCWIJK = line.Substring(17, 4).Trim(),
                      PCLETTER = line.Substring(21, 2).Trim(),
                      PCREEKSID = line.Substring(23, 1).Trim(),
                      PCREEKSVAN = string.IsNullOrEmpty(line.Substring(24, 5).Trim()) ? 0 : int.Parse(line.Substring(24, 5).Trim()),
                      PCREEKSTOT = string.IsNullOrEmpty(line.Substring(29, 5).Trim()) ? 0 : int.Parse(line.Substring(29, 5).Trim()),
                      PCCITYTPG = line.Substring(34, 18).Trim(),
                      PCCITYNEN = line.Substring(52, 24).Trim(),
                      PCSTRTPG = line.Substring(76, 17).Trim(),
                      PCSTRNEN = line.Substring(93, 24).Trim(),
                      PCSTROFF = line.Substring(117, 43).Trim(),
                      PCCITYEXT = line.Substring(160, 4).Trim(),
                      PCSTREXT = line.Substring(164, 5).Trim(),
                      PCGEMEENTEID = string.IsNullOrEmpty(line.Substring(169, 4).Trim()) ? 0 : int.Parse(line.Substring(169, 4).Trim()),
                      PCGEMEENTENAAM = line.Substring(173, 24).Trim(),
                      PCPROVINCIE = line.Substring(197, 1).Trim(),
                      PCCEBUCO = string.IsNullOrEmpty(line.Substring(198, 3).Trim()) ? 0 : int.Parse(line.Substring(198, 3).Trim())
                    };

                    var existingVar = (from c in unit.Scope.Repository<ZipCode>().GetAll()
                                       where
                                         c.PCWIJK == newzip.PCWIJK
                                      && c.PCLETTER == newzip.PCLETTER
                                      && c.PCREEKSID == newzip.PCREEKSID
                                      && c.PCREEKSVAN == newzip.PCREEKSVAN
                                      && c.PCREEKSTOT == newzip.PCREEKSTOT
                                      && c.PCCITYTPG == newzip.PCCITYTPG
                                      && c.PCCITYNEN == newzip.PCCITYNEN
                                      && c.PCSTRTPG == newzip.PCSTRTPG
                                      && c.PCSTRNEN == newzip.PCSTRNEN
                                      && c.PCSTROFF == newzip.PCSTROFF
                                      && c.PCCITYEXT == newzip.PCCITYEXT
                                      && c.PCSTREXT == newzip.PCSTREXT
                                      && c.PCGEMEENTEID == newzip.PCGEMEENTEID
                                      && c.PCGEMEENTENAAM == newzip.PCGEMEENTENAAM
                                      && c.PCPROVINCIE == newzip.PCPROVINCIE
                                      && c.PCCEBUCO == newzip.PCCEBUCO
                                       select c).FirstOrDefault();

                    if (existingVar == null)
                    {
                      existingVar = new ZipCode()
                                      {
                                        PCWIJK = newzip.PCWIJK,
                                        PCLETTER = newzip.PCLETTER,
                                        PCREEKSID = newzip.PCREEKSID,
                                        PCREEKSVAN = newzip.PCREEKSVAN,
                                        PCREEKSTOT = newzip.PCREEKSTOT,
                                        PCCITYTPG = newzip.PCCITYTPG,
                                        PCCITYNEN = newzip.PCCITYNEN,
                                        PCSTRTPG = newzip.PCSTRTPG,
                                        PCSTRNEN = newzip.PCSTRNEN,
                                        PCSTROFF = newzip.PCSTROFF,
                                        PCCITYEXT = newzip.PCCITYEXT,
                                        PCSTREXT = newzip.PCSTREXT,
                                        PCGEMEENTEID = newzip.PCGEMEENTEID,
                                        PCGEMEENTENAAM = newzip.PCGEMEENTENAAM,
                                        PCPROVINCIE = newzip.PCPROVINCIE,
                                        PCCEBUCO = newzip.PCCEBUCO
                                      };
                      unit.Scope.Repository<ZipCode>().Add(existingVar);
                    }

                    #endregion
                  }
                }
                try
                {
                  using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromMinutes(3)))
                  {
                    unit.Save();
                    scope.Complete();
                  }

                  //mark file as completed
                  downloader.RenameRemoteFile(remoteFile.FileName, remoteFile.FileName + ".comp");
                  log.Info("Post code import process completed succesfully for file: " + remoteFile.FileName);
                }
                catch (Exception e)
                {
                  log.Error("Import of zip codes for file: " + remoteFile.FileName + " failed", e);

                  //mark as error
                  downloader.RenameRemoteFile(remoteFile.FileName, remoteFile.FileName + ".err");
                }
              }
            }
          }
        }

      }
      //delete all local files
      DirectoryInfo info = new DirectoryInfo(downloadLocation);
      FileInfo[] infos = info.GetFiles("*.zip");
      foreach (var i in infos)
      {
        File.Delete(Path.Combine(downloadLocation, i.Name));
      }

      log.AuditSuccess("Post Code import finished", "Post Code Import");
    }
  }
}
