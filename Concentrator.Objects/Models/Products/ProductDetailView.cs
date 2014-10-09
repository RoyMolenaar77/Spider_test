using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Objects.Models.Products
{
  public class ProductDetailView
  {
    public Int32 ProductID { get; set; }

    public string ShortContentDescription { get; set; }

    public string BrandName { get; set; }

    public string LongContentDescription { get; set; }

    public Int32 LanguageID { get; set; }

    public string ModelName { get; set; }

    public string LongDescription { get; set; }

  }
}