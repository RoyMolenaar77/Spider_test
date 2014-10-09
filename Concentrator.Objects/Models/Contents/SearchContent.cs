using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Contents
{
  public class SearchContent : BaseModel<SearchContent>
  {
    public int SearchContentID { get; set; }

    public Int32 ProductID { get; set; }

    public string VendorItemNumber { get; set; }

    public string BrandName { get; set; }

    public string Barcode { get; set; }

    public string ProductName { get; set; }

    public string ModelName { get; set; }

    public string CustomItemNumber { get; set; }

    public string ShortDescription { get; set; }

    public string LongDescription { get; set; }

    public string ImagePath { get; set; }

    public string ShortContentDescription { get; set; }

    public string SearchText { get; set; }


    public override System.Linq.Expressions.Expression<Func<SearchContent, bool>> GetFilter()
    {
      return null;
    }
  }
}
