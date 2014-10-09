using Concentrator.Objects.Models.Base;
using System;

namespace Concentrator.Objects.Models.MastergroupMapping
{
  public class MasterGroupMappingSettingTemplate : AuditObjectBase<MasterGroupMappingSettingTemplate>
  {
    public int MasterGroupMappingSettingTemplateID { get; set; }

    public int MasterGroupMappingSettingID { get; set; }

    public int? LanguageID { get; set; }

    public string DefaultValue { get; set; }

    public virtual MasterGroupMappingSetting MasterGroupMappingSetting { get; set; }

    public override System.Linq.Expressions.Expression<Func<MasterGroupMappingSettingTemplate, bool>> GetFilter()
    {
      return null;
    }
  }
}