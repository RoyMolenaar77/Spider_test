using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Products;

namespace Concentrator.Objects.Models.Vendors
{
  public class SapProductGroupNumber
  {
    public String SAPNumber { get; set; }

    public String GTin { get; set; }

    public String Description { get; set; }

    public Int32 ProductGroupNumber { get; set; }
  }
}