using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Concentrator.Objects.Models.EDI.Order;

namespace Concentrator.ui.Management.Models
{
  public class TopTenProductViewModel
  {
    public String CustomItemNumber { get; set; }
    public String ProductName { get; set; }
    public int Count { get; set; }
    public int ProductID { get; set; }
  }
}
