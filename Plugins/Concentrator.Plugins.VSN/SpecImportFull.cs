using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.ZipUtil;

namespace Concentrator.Plugins.VSN
{
  public class SpecImportFull : SpecImportBase
  {
    private const string _name = "VSN Specs Import Plugin (Full)";

    public override string Name
    {
      get { return _name; }
    }

    protected override void Process()
    {

      var config = GetConfiguration().AppSettings.Settings;

      var ftp = new FtpManager(config["VSNFtpUrl"].Value, "pub3/",
        config["VSNUser"].Value, config["VSNPassword"].Value, false, true, log);

      using (var unit = GetUnitOfWork())
      {
        using (var specFile = ftp.OpenFile("XMLExportProperties.zip"))
        {
          using (var zipProc = new ZipProcessor(specFile.Data))
          {
            foreach (var file in zipProc)
            {
              using (file)
              {
                using (DataSet ds = new DataSet())
                {
                  ds.ReadXml(file.Data);

                  ProcessSpecTable(ds.Tables[0], unit);
                }
              }
            }
          }
        }
      }
      log.AuditComplete("Finished full VSN specs import", "VSN Specs Import");
    }
  }
}
