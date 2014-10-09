using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Concentrator.Tasks.Euretco.RSO.Navision.Exporters;

namespace Concentrator.Tasks.Euretco.RSO.Navision
{
  internal class Program
  {
    static void Main(string[] args)
    {
      TaskBase.Execute<ProductExporter>();
    }
  }
}
