using Concentrator.Objects.Models.Base;
using System.Collections.Generic;
using System;

namespace Concentrator.Objects.Models.MastergroupMapping
{
  public class MasterGroupMappingSetting : AuditObjectBase<MasterGroupMappingSetting>
  {
    public int MasterGroupMappingSettingID { get; set; }

    public string Group { get; set; }

    public string Type { get; set; }

    public string Name { get; set; }

    public virtual ICollection<MasterGroupMappingSettingOption> MasterGroupMappingSettingOptions { get; set; }

    public virtual ICollection<MasterGroupMappingSettingTemplate> MasterGroupMappingSettingTemplates { get; set; }

    public virtual ICollection<MasterGroupMappingSettingValue> MasterGroupMappingSettingValues { get; set; }
    
    public override System.Linq.Expressions.Expression<Func<MasterGroupMappingSetting, bool>> GetFilter()
    {
      return null;
    }
  }
}
