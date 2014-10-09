using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Forms
{
  [Flags]
  public enum FormNotificationType
  {
    Price = 1,
    Product = 2,
    Publication = 4,
    Other = 8
  }
}
