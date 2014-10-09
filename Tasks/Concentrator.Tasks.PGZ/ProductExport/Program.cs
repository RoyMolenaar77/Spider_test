using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concentrator.Tasks.PGZ.ProductExport
{
  internal class Program
  {
    static void Main(string[] args)
    {
      TaskBase.Execute<JDEProductExporter>();

      TaskBase.Execute<WMSProductExporter>();
    }
  }
}