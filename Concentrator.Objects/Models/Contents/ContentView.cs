using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Contents
{
  public class ContentView : BaseModel<ContentView>
  {
    public Int32 ConnectorID { get; set; }
          
    public Int32 ProductID { get; set; }
          
    public Int32 LanguageID { get; set; }
          
    public String VendorItemNumber { get; set; }
          
    public Int32 BrandID { get; set; }
          
    public String BrandName { get; set; }
          
    public String VendorBrandCode { get; set; }
          
    public String ShortDescription { get; set; }
          
    public String LongDescription { get; set; }
          
    public String LineType { get; set; }
          
    public String LedgerClass { get; set; }
          
    public String ProductDesk { get; set; }
          
    public Boolean? ExtendedCatalog { get; set; }
          
    public Int32 ProductContentID { get; set; }
          
    public Int32 VendorID { get; set; }
          
    public String LongContentDescription { get; set; }
          
    public String ShortContentDescription { get; set; }
          
    public DateTime? CutOffTime { get; set; }
          
    public Int32? DeliveryHours { get; set; }

    public String ProductName { get; set; }

    public String ModelName { get; set; }

    public String WarrantyInfo { get; set; }

    public override System.Linq.Expressions.Expression<Func<ContentView, bool>> GetFilter()
    {
      return (c => Client.User.VendorIDs.Contains(c.VendorID));
    }
  }
}