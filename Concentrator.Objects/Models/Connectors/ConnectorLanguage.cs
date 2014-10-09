using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Localization;

namespace Concentrator.Objects.Models.Connectors
{
  public class ConnectorLanguage : BaseModel<ConnectorLanguage>
  {
    public Int32 ConnectorLanguageID { get; set; }
          
    public Int32 ConnectorID { get; set; }
          
    public Int32 LanguageID { get; set; }
          
    public String Country { get; set; }
          
    public virtual Connector Connector { get;set;}
            
    public virtual Language Language { get;set;}


    public override System.Linq.Expressions.Expression<Func<ConnectorLanguage, bool>> GetFilter()
    {
      return null;
    }
  }
}