using Concentrator.Objects.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.MastergroupMapping
{
  public class MasterGroupMappingSettingOption : AuditObjectBase<MasterGroupMappingSettingOption>
  {
    public int MasterGroupMappingSettingID { get; set; }

    public int OptionID { get; set; }

    public string Value { get; set; }

    public virtual MasterGroupMappingSetting MasterGroupMappingSetting { get; set; }

    public override System.Linq.Expressions.Expression<Func<MasterGroupMappingSettingOption, bool>> GetFilter()
    {
      return null;
    }
  }
}
