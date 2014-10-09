using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concentrator.Tasks.Euretco.Rso.BizTalk.ProductExport.Models
{
  public class ConfiguredProductDBModel
  {
    public int MasterGroupMappingID { set; get; }
    public int ProductID { set; get; }
    public bool IsConfigurable { set; get; }
  }
}