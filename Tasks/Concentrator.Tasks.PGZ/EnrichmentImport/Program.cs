using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concentrator.Tasks.PGZ.EnrichmentImport
{
  internal class Program
  {
    static void Main(string[] args)
    {
      TaskBase.Execute<EnrichmentImporter>();
    }
  }
}