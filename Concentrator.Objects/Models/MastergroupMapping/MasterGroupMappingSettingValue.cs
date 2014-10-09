using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.MastergroupMapping
{
  public class MasterGroupMappingSettingValue : AuditObjectBase<MasterGroupMappingSettingValue>
  {

    public int MasterGroupMappingID { get; set; }

    public int MasterGroupMappingSettingID { get; set; }

    public string Value { get; set; }

    public int? LanguageID { get; set; }

    public virtual MasterGroupMapping MasterGroupMapping { get; set; }

    public virtual MasterGroupMappingSetting MasterGroupMappingSetting { get; set; }

    public virtual Language Language { get; set; }

    public override System.Linq.Expressions.Expression<Func<MasterGroupMappingSettingValue, bool>> GetFilter()
    {
      return null;
    }
  }
}
