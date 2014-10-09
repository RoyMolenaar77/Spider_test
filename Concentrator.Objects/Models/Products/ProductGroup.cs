using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.MastergroupMapping;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Products
{
  public class ProductGroup : BaseModel<ProductGroup>
  {
    public Int32 ProductGroupID { get; set; }

    public Int32 Score { get; set; }

    public Boolean? Searchable { get; set; }

    public String ImagePath { get; set; }

    public virtual ICollection<ConnectorPublication> ConnectorPublications { get; set; }

    public virtual ICollection<ContentPrice> ContentPrices { get; set; }

    public virtual ICollection<ContentProduct> ContentProducts { get; set; }

    public virtual ICollection<ContentVendorSetting> ContentVendorSettings { get; set; }

    public virtual ICollection<ProductGroupConnectorVendor> ProductGroupConnectorVendors { get; set; }

    public virtual ICollection<ProductGroupContentVendor> ProductGroupContentVendors { get; set; }

    public virtual ICollection<ProductGroupLanguage> ProductGroupLanguages { get; set; }

    public virtual ICollection<ProductGroupMapping> ProductGroupMappings { get; set; }

    public virtual ICollection<ProductGroupPublish> ProductGroupPublishes { get; set; }

    public virtual ICollection<ProductGroupSelector> ProductGroupSelectors { get; set; }

    public virtual ICollection<ProductGroupVendor> ProductGroupVendors { get; set; }

    public virtual ICollection<MissingContent> MissingContents { get; set; }

    public virtual ICollection<VendorPriceRule> VendorPriceRules { get; set; }

    public virtual ICollection<VendorPrice> VendorPrices { get; set; }

    public virtual ICollection<ConnectorPublicationRule> ConnectorPublicationRules { get; set; }

    public virtual ICollection<MasterGroupMapping> MasterGroupMappings { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductGroup, bool>> GetFilter()
    {
      return null;
    }

    public override String ToString()
    {
      return String.Format("Product Group {0}: '{1}'", ProductGroupID, ProductGroupLanguages
        .Where(pgl => pgl.LanguageID == Client.User.LanguageID)
        .Select(pgl => pgl.Name)
        .FirstOrDefault() ?? String.Empty);
    }
  }
}