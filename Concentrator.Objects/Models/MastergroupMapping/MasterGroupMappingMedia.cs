using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.MastergroupMapping
{
  public class MasterGroupMappingMedia : AuditObjectBase<MasterGroupMappingMedia>
  {
    public int MasterGroupMappingMediaID { get; set; }

    public int MasterGroupMappingID { get; set; }

    public int ImageTypeID { get; set; }

    public string ImagePath { get; set; }

    public int? ConnectorID { get; set; }

    public int? LanguageID { get; set; }

    public virtual MasterGroupMapping MasterGroupMapping { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual Language Language { get; set; }

    public override System.Linq.Expressions.Expression<Func<MasterGroupMappingMedia, bool>> GetFilter()
    {
      return null;
    }
  }
}
