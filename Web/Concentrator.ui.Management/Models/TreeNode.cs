using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Concentrator.ui.Management.Models
{
  public class TreeNode : TreeNodeBase
  {
    public int ProductGroupMappingID { get; set; }

    public int ConnectorID { get; set; }
  }
}