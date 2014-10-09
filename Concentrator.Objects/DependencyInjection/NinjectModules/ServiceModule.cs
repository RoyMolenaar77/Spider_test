using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Vendors;
using Ninject.Modules;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Services;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Brands;

using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Dashboards;
using Concentrator.Objects.Models.Media;
using Concentrator.Objects.Models.MastergroupMapping;

namespace Concentrator.Objects.DependencyInjection.NinjectModules
{
  public class ServiceModule : NinjectModule
  {
    public override void Load()
    {
      Bind(typeof(IService<>)).To(typeof(Service<>));
      Bind(typeof(IService<Brand>)).To(typeof(BrandService));
      Bind(typeof(IService<Connector>)).To(typeof(ConnectorService));
      Bind(typeof(IService<Content>)).To(typeof(ContentService));
      Bind(typeof(IService<ContentPrice>)).To(typeof(ContentPriceService));
      Bind(typeof(IService<ContentProduct>)).To(typeof(ContentProductService));
      Bind(typeof(IService<ContentVendorSetting>)).To(typeof(ContentVendorSettingService));
      Bind(typeof(IService<MasterGroupMapping>)).To(typeof(MasterGroupMappingService));
      Bind(typeof(IService<MediaType>)).To(typeof(MediaTypeService));
      Bind(typeof(IService<Order>)).To(typeof(OrderService));
      Bind(typeof(IService<OrderRule>)).To(typeof(OrderRuleService));
      Bind(typeof(IService<Portal>)).To(typeof(PortalService));
      Bind(typeof(IService<Product>)).To(typeof(ProductService));
      Bind(typeof(IService<ProductAttributeMetaData>)).To(typeof(ProductAttributeService));
      Bind(typeof(IService<ProductAttributeGroupName>)).To(typeof(ProductAttributeGroupNameService));
      Bind(typeof(IService<ProductGroupLanguage>)).To(typeof(ProductGroupLanguageService));
      Bind(typeof(IService<ProductGroupMapping>)).To(typeof(ProductGroupMappingService));
      Bind(typeof(IService<ProductMedia>)).To(typeof(MediaService));
      Bind(typeof(IService<RelatedProductType>)).To(typeof(RelatedProductTypeService));
      Bind(typeof(IService<Vendor>)).To(typeof(VendorService));
      Bind(typeof(IService<VendorAssortment>)).To(typeof(VendorAssortmentService));
      Bind(typeof(IService<VendorStockType>)).To(typeof(VendorStockTypeService));
    }
  }
}
