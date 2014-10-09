using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Config;

namespace Spider.MiddleWare.TestImageConsole
{
  class Program
  {
    static void Main(string[] args)
    {
      XmlConfigurator.Configure();

      ImageService.ImageUtility.Start();
      Console.ReadLine();
      ImageService.ImageUtility.Stop();
    }
  }
}
