using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace ImageService
{
  public partial class Service1 : ServiceBase
  {
    public Service1()
    {
      InitializeComponent();
    }

    protected override void OnStart(string[] args)
    {
      ImageService.ImageUtility.Start();
    }

    protected override void OnStop()
    {
      ImageService.ImageUtility.Stop();
    }
  }
}
