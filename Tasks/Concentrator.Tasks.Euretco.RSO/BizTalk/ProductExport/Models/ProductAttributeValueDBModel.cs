using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concentrator.Tasks.Euretco.Rso.BizTalk.ProductExport.Models
{
  public class ProductAttributeValueDBModel
  {
    public int ProductID { set; get; }
    public bool IsConfigurable { set; get; }
    public string AttributeCode { set; get; }
    public string Value { set; get; }
    public string Name { set; get; }
  }
}