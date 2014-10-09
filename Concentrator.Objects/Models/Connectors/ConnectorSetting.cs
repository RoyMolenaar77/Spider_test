using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;
                  
namespace Concentrator.Objects.Models 
{
  public class ConnectorSetting : BaseModel<ConnectorSetting>
  {
    public Int32 ConnectorID { get; set; }
          
    public String SettingKey { get; set; }
          
    public String Value { get; set; }
          
    public virtual Connector Connector { get;set;}


    public override System.Linq.Expressions.Expression<Func<ConnectorSetting, bool>> GetFilter()
    {
      return null;
    }
  }
}