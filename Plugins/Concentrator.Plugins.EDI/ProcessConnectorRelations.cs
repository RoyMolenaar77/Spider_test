using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Enumerations;
using System.Configuration;
using Concentrator.Objects.EDI.Ftp;
using System.IO;
using Concentrator.Objects.Ftp;

namespace Concentrator.Plugins.EDI
{
  public class ProcessConnectorRelations : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Process Connector Relations"; }
    }

    protected override void Process()
    {
      using(var unit = GetUnitOfWork())
      {
        var connectorRelations = unit.Scope.Repository<ConnectorRelation>().GetAll().ToList();

        connectorRelations.ForEach(x =>
        {
          string userPath = ConfigurationManager.AppSettings["ConcentratorFtpSource"].ToString();
        
          if (x.UseFtp.HasValue && x.UseFtp.Value)
          {
            if (x.FtpType.HasValue && x.FtpType.Value != (int)FtpTypeEnum.Customer)
            {
              x.FtpAddress = ConfigurationManager.AppSettings["ConcentratorFtpAddress"];
              x.FtpPort = int.Parse(ConfigurationManager.AppSettings["ConcentratorFtpPort"]);

              if (!ConcentratorFtp.HomeDirCreated(x.ConnectorRelationID.ToString()))
                ConcentratorFtp.AddUserToConfig(x.ConnectorRelationID.ToString(), x.FtpPass);              
            }
            else
            {
              x.ConnectorRelationExports.ForEach((export, idx) =>
              {
                try
                {
                  bool ftpPassive = false;
                  if (x.FtpConnectionType.HasValue)
                  {
                    var ftpType = (FtpConnectionTypeEnum)x.FtpConnectionType.Value;

                    if (ftpType == FtpConnectionTypeEnum.Passive)
                      ftpPassive = true;
                  }

                  FtpManager manager = new FtpManager(x.FtpAddress,
                                             export.DestinationPath,
                                             x.FtpUserName,
                                            x.FtpPass, x.FtpSSL.HasValue ? x.FtpSSL.Value : false, ftpPassive, log);

                  var transferPath = Path.Combine(userPath, export.SourcePath);

                  string archive = Path.Combine(transferPath, "Archive");
                  if (!Directory.Exists(archive))
                    Directory.CreateDirectory(archive);
                  string error = Path.Combine(transferPath, "Error");
                  if (!Directory.Exists(error))
                    Directory.CreateDirectory(error);

                  Directory.GetFiles(transferPath).ForEach((file, index) =>
                  {
                    FileInfo inf = new FileInfo(file);
                    try
                    {
                      using (var stream = new FileStream(file, FileMode.Open))
                      {
                        manager.Upload(stream, inf.Name);
                      }
                      File.Move(file, Path.Combine(archive, inf.Name));
                    }
                    catch (Exception ex)
                    {
                      log.Error(string.Format("Transfer file {0} failed", inf.Name), ex);
                      File.Move(file, Path.Combine(error, inf.Name));
                    }
                  });
                }
                catch (Exception ex)
                {
                  log.AuditError("File transfer for " + export.ConnectorRelationID + " failed", ex);
                }
              });
            }
          }
        });
      }
    }
  }
}
