using System;
using System.Collections.Generic;
using System.Diagnostics;

using Concentrator.Objects.Models.Connectors;
using Concentrator.Tasks.Stores;

namespace Concentrator.Tasks.Euretco.Rso.BizTalk.ProductExport
{
  public class ProductExporterSettingsStore : ConnectorSettingStoreBase
  {
    [ConnectorSetting("RSO Biztalk export folder")]
    public string LocalExportFolder { get; set; }

    private string _server;
    [ConnectorSetting("RSO Export Server")]
    public String Server {
      get
      {
#if DEBUG
        _server = _server.Replace("Staging", "Test");
#endif
        return _server;
      }
      set { _server = value; }
    }

    [ConnectorSetting("RSO Export Username")]
    public String UserName { get; set; }

    [ConnectorSetting("RSO Export Password")]
    public String Password { get; set; }

    [ConnectorSetting("RSO Connector Archive")]
    public String RemoteExportSubFolder { get; set; }

    public ProductExporterSettingsStore(Connector connector, TraceSource traceSource = null)
      : base(connector, traceSource)
    {
    }
  }
}