using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.EDI.Order
{
  public class EdiOrderListener
  {
    public Int32 EdiRequestID { get; set; }

    public string CustomerName { get; set; }

    public string CustomerIP { get; set; }

    public string CustomerHostName { get; set; }

    public string RequestDocument { get; set; }

    public DateTime ReceivedDate { get; set; }

    public bool Processed { get; set; }

    public string ResponseRemark { get; set; }

    public Int32 ResponseTime { get; set; }

    public string ErrorMessage { get; set; }
  }
}
