using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.EDI.Response
{
  public class ReturnError
  {
    public string ErrorCode { get; set; }
    public bool SkipOrder { get; set; }
    public string ErrorMessage { get; set; }
  }
}
