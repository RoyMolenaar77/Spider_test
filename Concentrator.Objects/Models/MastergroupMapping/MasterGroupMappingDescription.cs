using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Scan;
using Concentrator.Objects.Models.Slurp;
using Concentrator.Objects.Models.Magento;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.MastergroupMapping;

namespace Concentrator.Objects.Models.MastergroupMapping
{
  public class MasterGroupMappingDescription : BaseModel<MasterGroupMappingDescription>
  {
    public override bool Equals(object obj)
    {
      if (obj is MasterGroupMappingDescription)
        return (this.MasterGroupMappingID == ((MasterGroupMappingDescription)obj).MasterGroupMappingID && this.LanguageID == ((MasterGroupMappingDescription)obj).LanguageID);
      else return false;
    }

    public override int GetHashCode()
    {
      return MasterGroupMappingID;
    }

    public Int32 MasterGroupMappingID { get; set; }

    public Int32 LanguageID { get; set; }

    public String Description { get; set; }

    public virtual MasterGroupMapping MasterGroupMapping { get; set; }

    public virtual Language Language { get; set; }

    public override System.Linq.Expressions.Expression<Func<MasterGroupMappingDescription, bool>> GetFilter()
    {
      return null;
    }
  }
}