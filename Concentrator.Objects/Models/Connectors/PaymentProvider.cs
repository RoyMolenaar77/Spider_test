using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
                  
namespace Concentrator.Objects.Models.Connectors 
{
  public class PaymentProvider : BaseModel<PaymentProvider>
  {
    public Int32 PaymentProviderID { get; set; }
          
    public String Name { get; set; }
          
    public String ProviderType { get; set; }
          
    public Boolean IsActive { get; set; }
          
    public virtual ICollection<ConnectorPaymentProvider> ConnectorPaymentProviders { get;set;}


    public override System.Linq.Expressions.Expression<Func<PaymentProvider, bool>> GetFilter()
    {
      return null;
    }
  }
}