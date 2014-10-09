using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Contents
{
  public class SeoTexts : BaseModel<SeoTexts>
  {
    public int SeoTextID { get; set; }

    public Int32 LanguageID { get; set; }

    public Int32? ConnectorID { get; set; }

    public Int32? ProductGroupMappingID { get; set; }

    public Int32 MasterGroupMappingID { get; set; }

    public String Description { get; set; }

    public int DescriptionType { get; set; }

    public virtual Language Language { get; set; }

    public override System.Linq.Expressions.Expression<Func<SeoTexts, bool>> GetFilter()
    {
      return null;
    }
  }


  public enum SeoDescriptionTypes
  {
    Description = 1,
    Meta_title = 2,
    Meta_description = 3,
    Description2 = 4,
    Description3 = 5
  }


}