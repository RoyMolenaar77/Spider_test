using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Config;
using Concentrator.Objects.Service;
using Concentrator.Objects.Web;

namespace Concentrator.MiddleWare.ExportTestConsole
{
  class Program
  {
    static void Main(string[] args)
    {
      // start log4net
      XmlConfigurator.Configure();

      ServiceLayer.Start();
      Console.WriteLine("Press any key");
      Console.ReadLine();
      ServiceLayer.Stop();
    }
  }
}
