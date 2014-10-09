using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Globalization;

using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Management;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Vendors
{
  public class Vendor : BaseModel<Vendor>
  {
    public Int32 VendorID { get; set; }

    public Int32 VendorType { get; set; }

    public String Name { get; set; }

    public String Description { get; set; }

    public String BackendVendorCode { get; set; }

    public Int32? ParentVendorID { get; set; }

    public String OrderDispatcherType { get; set; }

    public Double? CDPrice { get; set; }

    public Double? DSPrice { get; set; }

    public String PurchaseOrderType { get; set; }

    public Boolean IsActive { get; set; }

    public DateTime? CutOffTime { get; set; }

    public Int32? DeliveryHours { get; set; }

    public virtual ICollection<AdditionalOrderProduct> AdditionalOrderProducts { get; set; }

    public virtual ICollection<BrandMedia> BrandMedias { get; set; }

    public virtual ICollection<Connector> Connectors { get; set; }

    public virtual ICollection<ConnectorPublication> ConnectorPublications { get; set; }

    public virtual ICollection<ConnectorRuleValue> ConnectorRuleValues { get; set; }

    public virtual ICollection<ContentPrice> ContentPrices { get; set; }

    public virtual ICollection<ContentProduct> ContentProducts { get; set; }

    public virtual ICollection<ContentVendorSetting> ContentVendorSettings { get; set; }

    public virtual ICollection<OrderLine> OrderLines { get; set; }

    public virtual ICollection<OrderResponse> OrderResponses { get; set; }

    public virtual ICollection<PreferredConnectorVendor> PreferredConnectorVendors { get; set; }

    public virtual ICollection<ProductAttributeGroupMetaData> ProductAttributeGroupMetaDatas { get; set; }

    public virtual ICollection<ProductAttributeMetaData> ProductAttributeMetaDatas { get; set; }

    public virtual ICollection<ProductBarcode> ProductBarcodes { get; set; }

    public virtual ICollection<ProductDescription> ProductDescriptions { get; set; }

    public virtual ICollection<ProductGroupConnectorVendor> ProductGroupConnectorVendors { get; set; }

    public virtual ICollection<ProductGroupContentVendor> ProductGroupContentVendors { get; set; }

    public virtual ICollection<ConnectorPublicationRule> ConnectorPublicationRules { get; set; }

    public virtual ICollection<ProductGroupVendor> ProductGroupVendors { get; set; }

    public virtual ICollection<ProductImage> ProductImages { get; set; }

    public virtual ICollection<ProductMedia> ProductMedias { get; set; }

    public virtual ICollection<RelatedProduct> RelatedProducts { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; }

    public virtual ICollection<VendorAssortment> VendorAssortments { get; set; }

    public virtual ICollection<VendorMapping> VendorMappings { get; set; }

    public virtual ICollection<VendorMapping> VendorMappings1 { get; set; }

    public virtual ICollection<VendorProductMatch> VendorProductMatches { get; set; }

    public virtual ICollection<VendorSetting> VendorSettings { get; set; }

    public virtual ICollection<VendorStock> VendorStocks { get; set; }

    public virtual ICollection<MissingContent> MissingContents { get; set; }

    public virtual ICollection<VendorProductStatus> VendorProductStatus { get; set; }

    public virtual ICollection<VendorPriceRule> VendorPriceRules { get; set; }

    public virtual ICollection<ConnectorVendorManagementContent> ConnectorVendorManagementContents { get; set; }
    
    public virtual ICollection<BrandVendor> BrandVendors { get; set; }

    public virtual ICollection<ContentStock> ContentStocks { get; set; }

    public virtual String GetVendorSetting(String settingKey, String defaultValue = null)
    {
      if (VendorSettings == null)
      {
        throw new InvalidOperationException("Vendor settings are not included");
      }

      return VendorSettings
        .Where(vendorSetting => vendorSetting.SettingKey.Equals(settingKey, StringComparison.OrdinalIgnoreCase))
        .Select(vendorSetting => vendorSetting.Value)
        .SingleOrDefault() ?? defaultValue;
    }

    public virtual TResult GetVendorSetting<TResult>(String settingKey, TResult defaultValue = default(TResult))
    {
      return GetVendorSetting<TResult>(settingKey, null, defaultValue);
    }

    public virtual TResult GetVendorSetting<TResult>(String settingKey, CultureInfo culture, TResult defaultValue = default(TResult))
    {
      var value = GetVendorSetting(settingKey);

      return value != null
        ? TypeConverterService.ConvertFromString<TResult>(value, culture)
        : defaultValue;
    }

    public override System.Linq.Expressions.Expression<Func<Vendor, bool>> GetFilter()
    {
      return (v => Client.User.VendorIDs.Contains(v.VendorID));
    }

    public override String ToString()
    {
      return String.Format("Vendor {0}: '{1}'", VendorID, Name);
    }
  }
}