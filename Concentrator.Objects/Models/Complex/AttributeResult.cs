using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;

namespace Concentrator.Objects.Models.Complex
{
  public class AttributeResult : ComplexObject
  {

    public Int32 AttributeID { get; set; }

    public Boolean IsSearchable { get; set; }

    public Boolean IsVisible { get; set; }

    public String Sign { get; set; }

    public Int32 GroupID { get; set; }

    public Boolean NeedsUpdate { get; set; }

    public String AttributeCode { get; set; }

    public Int32 GroupIndex { get; set; }

    public Int32? OrderIndex { get; set; }

    public String GroupName { get; set; }

    public Int32? ProductID { get; set; }

    public String AttributeValue { get; set; }

    public Int32 LanguageID { get; set; }

    public Int32? ConnectorID { get; set; }

    public String AttributeName { get; set; }

    public DateTime? LastUpdate { get; set; }

    public Int32? VendorID { get; set; }
  }
}
