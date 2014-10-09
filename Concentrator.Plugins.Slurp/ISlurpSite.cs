using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Slurp
{
  public interface ISlurpSite
  {
    SlurpResult Process(string manufacturerId);
  }
}
