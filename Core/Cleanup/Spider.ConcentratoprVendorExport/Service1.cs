using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using log4net.Config;
using Spider.Objects.Web;
using Spider.Objects.Concentrator;

namespace Spider.ConcentratoprVendorExport
{
  public partial class Service1 : ServiceBase
  {
    public Service1()
    {
      InitializeComponent();
    }

    protected override void OnStart(string[] args)
    {
      XmlConfigurator.Configure();

      ServiceLayer.Start();
    }

    protected override void OnStop()
    {
      ServiceLayer.Stop();
    }
  }
}
