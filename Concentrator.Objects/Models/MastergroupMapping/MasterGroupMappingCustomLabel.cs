using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.MastergroupMapping
{
  public class MasterGroupMappingCustomLabel : BaseModel<MasterGroupMappingCustomLabel>
  {
    public int MasterGroupMappingCustomLabelID { get; set; }

    public int MasterGroupMappingID { get; set; }

    public int LanguageID { get; set; }

    public int? ConnectorID { get; set; }

    public string CustomLabel { get; set; }

    public virtual MasterGroupMapping MasterGroupMapping { get; set; }

    public virtual Language Language { get; set; }

    public virtual Connector Connector { get; set; }

    public override System.Linq.Expressions.Expression<Func<MasterGroupMappingCustomLabel, bool>> GetFilter()
    {
      return null;
    }
  }
}
