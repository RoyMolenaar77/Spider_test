using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Concentrator.Objects.Service;
using log4net.Config;
using Concentrator.Objects.Web;

namespace Concentrator.ConcentratorVendorImport
{
  public partial class ServiceConcentrator : ServiceBase
  {
    public ServiceConcentrator()
    {
      InitializeComponent();
    }

    protected override void OnStart(string[] args)
    {
      XmlConfigurator.Configure();

      SpiderPrincipal.Login("SYSTEM", "SYS");

      ServiceLayer.Start();
    }

    protected override void OnStop()
    {
      ServiceLayer.Stop();
    }
  }
}
