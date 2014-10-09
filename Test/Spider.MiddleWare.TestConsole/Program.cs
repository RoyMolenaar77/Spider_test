using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Service;
using log4net.Config;
using System.Configuration;
using Quartz;
using Quartz.Impl;
using Quartz.Job;
using Concentrator.Objects.Web;
using Concentrator.Objects;
using System.Xml.Linq;
using System.Xml;

namespace Concentrator.Middleware.Consolehost
{
  class Program
  {
    static void Main(string[] args)
    {
       
      // start log4net
      XmlConfigurator.Configure();

      SpiderPrincipal.Login("SYSTEM", "SYS");

      ServiceLayer.Start();
      Console.WriteLine("Press any key");
      Console.ReadLine();
      ServiceLayer.Stop();
    }
  }
}
