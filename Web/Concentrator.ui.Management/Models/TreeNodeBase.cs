using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Concentrator.ui.Management.Models
{
  public class TreeNodeBase
  {
    public string text { get; set; }
    public bool leaf { get; set; }
    public List<TreeNode> children;
  }
}