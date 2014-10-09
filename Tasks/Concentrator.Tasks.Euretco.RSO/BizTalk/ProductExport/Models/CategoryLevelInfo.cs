using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concentrator.Tasks.Euretco.Rso.BizTalk.ProductExport.Models
{
  public class CategoryLevelInfo
  {
    public int MasterGroupMappingID { set; get; }
    public int ParentMasterGroupMappingID { set; get; }
    public string LevelID { set; get; }
    public string LevelName { set; get; }
  }
}