using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MagentoTest
{
  class Program
  {
    static void Main(string[] args)
    {
      MagentoImportService.MagentoImportService.Start();
      Console.ReadLine();
      MagentoImportService.MagentoImportService.Stop();
    }
  }
}
