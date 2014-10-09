using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Concentrator.Web.Services.Models.Attributes
{
  public class ProductAttributeValueGroupResult
  {
    public int ProductAttributeValueGroupID { get; set; }

    public int AttributeID { get; set; }

    public int? ProductAttributeValueGroupScore { get; set; }

    public string ProductAttributeValueGroupName { get; set; }

    public string ProductAttributeValueGroupImage { get; set; }
  }
}