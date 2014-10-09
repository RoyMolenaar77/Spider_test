using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Concentrator.ui.Management.Models
{
  public class MediaAndSpecificationsModel
  {
    public int ProductsWithoutMedia { get; set; }
    public int ProductsWithoutMediaAndSpecs { get; set; }
    public int ProductsWithoutSpecs { get; set; }
    public int ProductWithoutMediaUrlAndVideo { get; set; }
    public int ProductsWithoutImage { get; set; }

    public int TotalProducts { get; set; }
  }
}