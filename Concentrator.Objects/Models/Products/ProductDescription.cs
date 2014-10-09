using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Products
{
  public class ProductDescription : AuditObjectBase<ProductDescription>
  {

      public ProductDescription()
      { 
      }

    public Int32 ProductID { get; set; }

    public Int32 LanguageID { get; set; }

    public Int32 VendorID { get; set; }

    public String ShortContentDescription { get; set; }

    public String LongContentDescription { get; set; }

    public String ShortSummaryDescription { get; set; }

    public String LongSummaryDescription { get; set; }

    public String PDFUrl { get; set; }

    public System.Nullable<Int32> PDFSize { get; set; }

    public String Url { get; set; }

    public String WarrantyInfo { get; set; }

    public String ModelName { get; set; }

    public String ProductName { get; set; }

    public String Quality { get; set; }

    public virtual Language Language { get; set; }

    public virtual Product Product { get; set; }

    public virtual Vendor Vendor { get; set; }

    /// <summary>
    /// Returns a productname/short content description/long content description
    /// </summary>
    public string Description
    {
      get
      {

        return !string.IsNullOrEmpty(ProductName) ?
        ProductName : ShortContentDescription;
      }
    }

    public virtual User User { get; set; }
    public virtual User User1 { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductDescription, bool>> GetFilter()
    {
      return (pd => Client.User.VendorIDs.Contains(pd.VendorID));
    }
  }
}