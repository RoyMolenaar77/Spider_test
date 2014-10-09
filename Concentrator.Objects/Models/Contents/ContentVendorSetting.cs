using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Contents
{
  public class ContentVendorSetting : AuditObjectBase<ContentVendorSetting>
  {
    public Int32 ContentVendorSettingID { get; set; }

    public Int32 ConnectorID { get; set; }

    public Int32 VendorID { get; set; }

    public Int32? ProductGroupID { get; set; }

    public Int32? ProductID { get; set; }

    public Int32? BrandID { get; set; }

    public Int32 ContentVendorIndex { get; set; }

    public virtual Brand Brand { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual Products.Product Product { get; set; }

    public virtual ProductGroup ProductGroup { get; set; }

    public virtual Vendor Vendor { get; set; }


    public override System.Linq.Expressions.Expression<Func<ContentVendorSetting, bool>> GetFilter()
    {
      return contentVendorSetting => Client.User.VendorIDs.Contains(contentVendorSetting.VendorID);
    }

    public override String ToString()
    {
      var stringBuilder = new StringBuilder();

      stringBuilder.AppendFormat("ContentVendorSetting {0}: Index: {1}, {2}, {3}"
        , ContentVendorSettingID
        , ContentVendorIndex
        , Connector
        , Vendor);

      if (ProductGroup != null)
      {
        stringBuilder.AppendFormat(", {0}", ProductGroup);
      }

      if (Brand != null)
      {
        stringBuilder.AppendFormat(", {0}", Brand);
      }

      if (Product != null)
      {
        stringBuilder.AppendFormat(", {0}", Product);
      }

      return stringBuilder.ToString();
    }
  }
}