using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Brands
{
  public class Brand : BaseModel<Brand>
  {
    public Int32 BrandID { get; set; }

    public String Name { get; set; }    

    public virtual ICollection<BrandVendor> BrandVendors { get; set; }

    public virtual ICollection<ConnectorPublication> ConnectorPublications { get; set; }

    public virtual ICollection<ContentPrice> ContentPrices { get; set; }

    public virtual ICollection<ConnectorPublicationRule> ConnectorPublicationRules { get; set; }

    public virtual ICollection<ContentProduct> ContentProducts { get; set; }

    public virtual ICollection<ContentVendorSetting> ContentVendorSettings { get; set; }

    public virtual ICollection<Products.Product> Products { get; set; }

    public virtual ICollection<BrandMedia> BrandMedias { get; set; }

    public virtual ICollection<Brand> ChildBrands { get; set; }

    public virtual ICollection<MissingContent> MissingContents { get; set; }

    public virtual ICollection<VendorPriceRule> VendorPriceRules { get; set; }
   
    public virtual Brand ParentBrand { get; set; }

    public int? ParentBrandID { get; set; }



    public override System.Linq.Expressions.Expression<Func<Brand, bool>> GetFilter()
    {
      return (brand => Client.User.VendorIDs.Any(l => brand.BrandVendors.Select(c => c.VendorID).Contains(l)));
    }

    public override String ToString()
    {
      return String.Format("Brand {0}: '{1}'", BrandID, Name);
    }
  }
}