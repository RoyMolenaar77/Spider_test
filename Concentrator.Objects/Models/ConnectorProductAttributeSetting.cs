using System;

namespace Concentrator.Objects.Models
{
  public partial class ConnectorProductAttributeSetting
  {
    public virtual Int32 ConnectorProductAttributeID
    {
      get;
      set;
    }

    public virtual String Code
    {
      get;
      set;
    }

    public virtual String Type
    {
      get;
      set;
    }

    public virtual String Value
    {
      get;
      set;
    }
  }
}
