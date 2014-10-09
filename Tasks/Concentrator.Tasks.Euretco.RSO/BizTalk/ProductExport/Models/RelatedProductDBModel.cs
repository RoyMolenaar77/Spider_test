using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concentrator.Tasks.Euretco.Rso.BizTalk.ProductExport.Models
{
  public class RelatedProductDBModel
  {
    public int ConfigurableProductID { set; get; }
    public int SimpleProductID { set; get; }
    public string Barcode { set; get; }
    public Boolean IsActive { set; get; }
    public Double Price { set; get; }
  }
}