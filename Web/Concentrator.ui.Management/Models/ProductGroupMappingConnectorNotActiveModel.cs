using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Concentrator.ui.Management.Models
{
  public class ProductGroupMappingConnectorNotActiveModel
  {
    public int ConnectorID { get; set; }
   
    public bool IsActiveForGroup { get; set; }

  }
}