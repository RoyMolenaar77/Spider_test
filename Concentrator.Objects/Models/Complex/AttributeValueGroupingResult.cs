using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;

namespace Concentrator.Objects.Models.Complex
{
  public class AttributeValueGroupingResult : ComplexObject
  {
    public int AttributeID { get; set; }

    public string Value { get; set; }

    public string Name { get; set; }

    public int? AttributeValueGroupID { get; set; }
  }
}
