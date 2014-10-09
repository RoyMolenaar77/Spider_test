using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Products 
{
  public class ProductBarcodeView : BaseModel<ProductBarcodeView>
  {
    public String Barcode { get; set; }
          
    public Int32 ConnectorID { get; set; }
          
    public Int32 ProductID { get; set; }


    public override System.Linq.Expressions.Expression<Func<ProductBarcodeView, bool>> GetFilter()
    {
      return null;
    }
  }
}