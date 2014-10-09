using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.ConcentratorRepos.Models
{
  public class LookGroup
  {
    public List<int> Products { get; set; }

    public string TargetGroup { get; set; }

    public string InputCode { get; set; }

    public string Season { get; set; }

    public string BackendLabel { get { return string.Format("{0} - {1} - {2}", InputCode, Season, TargetGroup); } }
  } 
}
