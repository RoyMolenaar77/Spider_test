using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Connectors
{
  public class ConnectorSystemAttribute :
    Attribute
  {
    //public int ConnectorSystemID;
    public string ConnectorSystem;
    //public ConnectorSystemAttribute(int ConnectorSystemID)
    //{
    //  this.ConnectorSystemID = ConnectorSystemID;
    //}

    public ConnectorSystemAttribute(string ConnectorSystem)
    {
      this.ConnectorSystem = ConnectorSystem;
    }
  }
}
