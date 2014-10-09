using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Concentrator.ui.Management.Models
{
    public class AttributeModel 
    {
      public int GroupIndex { get; set; }
      public string GroupName { get; set; }
      public string AttributeName { get; set; }
      public string AttributeValue { get; set; }
      public int? AttributeIndex { get; set; }
    }
}
