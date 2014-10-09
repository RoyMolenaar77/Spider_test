using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concentrator.Tasks.Euretco.Rso.BizTalk.ProductExport.Models
{
  public class ProductDBModel
  {
    public int ProductID { set; get; }
    public string VendorItemNumber { set; get; }
    public bool IsConfigurable { set; get; }
    public string BrandName { get; set; }
    public string ProductName { get; set; }
    public string LongContentDescription { get; set; }
    public string ShortContentDescription { get; set; }
    public string LongSummaryDescription { get; set; }
    public string ShortSummaryDescription { get; set; }
    public Boolean IsActive { set; get; }
    public Double Price { set; get; }
    public Double SpecialPrice { set; get; }
  }
}