using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Concentrator.Tasks;

namespace Concentrator.Tasks.PGZ.ProductHarmonization
{
  internal class Program
  {
    static void Main(string[] args)
    {
      TaskBase.Execute<ProductHarmonizer>();
    }
  }
}