using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Config;

namespace Spider.MiddleWare.ShopImport
{
  class Program
  {
    static void Main(string[] args)
    {
      XmlConfigurator.Configure();

      Spider.ShopImport.Importer.Start();
      Console.ReadLine();
      Spider.ShopImport.Importer.Stop();
    }
  }
}
