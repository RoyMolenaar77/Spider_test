﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.Ftp;

namespace Concentrator.Plugins.VSN
{
  public class SpecImportPartial : SpecImportBase
  {
    private const string _name = "VSN Specs Import Plugin (Partial)";

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
        using (var specFile = ftp.OpenFile("7ExportProperties.xml"))
        {
          using (DataSet ds = new DataSet())
          {
            ds.ReadXml(specFile.Data);

            ProcessSpecTable(ds.Tables[0], unit);
          }
        }
      }
      log.AuditComplete("Finished partial VSN specs import", "VSN Specs Import");
    }
  }
}
