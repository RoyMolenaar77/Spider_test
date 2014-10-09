using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Contents
{
  public class ContentAttribute : BaseModel<ContentAttribute>
  {
    public Int32 ContentAttributeID { get; set; }

    public Int32 AttributeID { get; set; }
          
    public Boolean IsSearchable { get; set; }
          
    public Boolean IsVisible { get; set; }
          
    public String Sign { get; set; }
          
    public Int32 GroupID { get; set; }
          
    public Boolean NeedsUpdate { get; set; }
          
    public String AttributeCode { get; set; }
          
    public Int32 GroupIndex { get; set; }
          
    public Int32? OrderIndex { get; set; }
          
    public String GroupName { get; set; }
          
    public Int32? ProductID { get; set; }
          
    public String AttributeValue { get; set; }
          
    public Int32 LanguageID { get; set; }
          
    public Int32? ConnectorID { get; set; }
          
    public String AttributeName { get; set; }
          
    public DateTime? LastUpdate { get; set; }
          
    public Int32? VendorID { get; set; }

    public String AttributePath { get; set; }

    public Int32? AttributeValueID { get; set; }

    public String AttributeOriginalValue { get; set; }

    public bool IsConfigurable { get; set; }

		public int? ConfigurablePosition { get; set; }

    public override System.Linq.Expressions.Expression<Func<ContentAttribute, bool>> GetFilter()
    {
      if (VendorID.HasValue)
        return (content => Client.User.VendorIDs.Contains((int)content.VendorID));
      else
        return null;
    }
  }
}