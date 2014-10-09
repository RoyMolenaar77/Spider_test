using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;
                  
namespace Concentrator.Objects.Models.Attributes 
{
  public class AttributeMatchStore : BaseModel<AttributeMatchStore>
  {
    public Int32 AttributeStoreID { get; set; }
          
    public Int32 AttributeID { get; set; }
          
    public Int32 ConnectorID { get; set; }
          
    public String StoreName { get; set; }
          
    public virtual Connector Connector { get;set;}
            
    public virtual ProductAttributeMetaData ProductAttributeMetaData { get;set;}


    public override System.Linq.Expressions.Expression<Func<AttributeMatchStore, bool>> GetFilter()
    {
      return null;
    }
  }
}