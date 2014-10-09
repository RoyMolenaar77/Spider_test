using System;

namespace Concentrator.Objects.Models
{
  public partial class ConnectorProductAttribute
  {
    public virtual Int32? ConnectorProductAttributeID
    {
      get;
      set;
    }

    public virtual Int32 ConnectorID
    {
      get;
      set;
    }

    public virtual Int32 ProductAttributeID
    {
      get;
      set;
    }

    public virtual String ProductAttributeType
    {
      get;
      set;
    }

    public virtual String DefaultValue
    {
      get;
      set;
    }

    public virtual Boolean IsFilter
    {
      get;
      set;
    }
  }
}
