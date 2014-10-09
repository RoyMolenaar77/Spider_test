using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Connectors
{
  public class ConnectorPaymentProvider : BaseModel<ConnectorPaymentProvider>
  {
    public Int32 ConnectorID { get; set; }
          
    public Int32 PaymentProviderID { get; set; }
          
    public String PaymentTermsCode { get; set; }
          
    public String PaymentInstrument { get; set; }
          
    public Int32 Portfolio { get; set; }
          
    public String UserName { get; set; }
          
    public String Password { get; set; }
          
    public virtual Connector Connector { get;set;}
            
    public virtual PaymentProvider PaymentProvider { get;set;}


    public override System.Linq.Expressions.Expression<Func<ConnectorPaymentProvider, bool>> GetFilter()
    {
      return null;
    }
  }
}