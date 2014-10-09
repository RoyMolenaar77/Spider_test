using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Management
{
  public class ManagementLabel : BaseModel<ManagementLabel>
  {
    public int ManagementLabelID
    {
      get;
      set;
    }

    public string Field
    {
      get;
      set;
    }

    public string Label
    {
      get;
      set;
    }

    public string Grid
    {
      get;
      set;
    }

    public int UserID
    {
      get;
      set;
    }

    public override System.Linq.Expressions.Expression<Func<ManagementLabel, bool>> GetFilter()
    {
      return null;
    }
  }
}
