using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.MastergroupMapping
{
  public class MasterGroupMappingLabel : BaseModel<MasterGroupMappingLabel>
  {
    public Int32 MasterGroupMappingLabelID { get; set; }

    public Int32 MasterGroupMappingID { get; set; }

    public String Label { get; set; }

    public Boolean? SearchEngine { get; set; }

    public Int32 LanguageID { get; set; }

    public virtual MasterGroupMapping MasterGroupMapping { get; set; }

    public override System.Linq.Expressions.Expression<Func<MasterGroupMappingLabel, bool>> GetFilter()
    {
      return null;
    }
  }
}
