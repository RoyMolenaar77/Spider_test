using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Localization;
using System;

namespace Concentrator.Objects.Models.MastergroupMapping
{
  public class MasterGroupMappingLanguage : AuditObjectBase<MasterGroupMappingLanguage>, ILedgerObject
  {
    public Int32 MasterGroupMappingID { get; set; }
    public Int32 LanguageID { get; set; }
    public string Name { get; set; }

    public virtual MasterGroupMapping MasterGroupMapping { get; set; }
    public virtual Language Language { get; set; }

    public override System.Linq.Expressions.Expression<Func<MasterGroupMappingLanguage, bool>> GetFilter()
    {
      return null;
    }

    public string ReturnPrimaryKeyHash()
    {
      return String.Format("{0}-{1}", MasterGroupMappingID, LanguageID);
    }
  }
}
