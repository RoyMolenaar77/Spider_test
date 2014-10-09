using System;

namespace Concentrator.Objects.Models
{
  public partial class ConnectorProductAttributeSettingTemplate
  {
    public virtual Int32 ConnectorSystemID
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
