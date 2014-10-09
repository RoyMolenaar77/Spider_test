using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Orders
{
  public class OrderItemFullfilmentInformation : BaseModel<OrderItemFullfilmentInformation>
  {
    public Int32 OrderFullfilmentInformationID { get; set; }

    public Int32 OrderResponseLineID { get; set; }

    public String Sequence { get; set; }

    public string Type { get; set; }

    public string Header { get; set; }

    public string Unit { get; set; }

    public string SupportType { get; set; }

    public string SupportID { get; set; }

    public virtual OrderResponseLine OrderResponseLine { get; set; }

    public string Code { get; set; }

    public string Label { get; set; }

    public string Value { get; set; }

    public override System.Linq.Expressions.Expression<Func<OrderItemFullfilmentInformation, bool>> GetFilter()
    {
      return null;
    }
  }
}
