using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using System.Xml;
using Concentrator.Objects;
using System.Xml.Linq;
using System.Drawing;
using System.IO;
using System.Configuration;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.Logic;
using System.Data.Linq;
using System.Text;
using Concentrator.Objects.Web;
using Concentrator.Web.Services.Vendors.BAS.WebService;
using System.Web.Script.Serialization;
using Concentrator.Objects.DataAccess.Repository;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Utility;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Media;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Complex;
using Concentrator.Web.Services.Base;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Users;
using System.Data.Objects;
using System.Security.Permissions;
using System.ServiceModel;
using System.Net;
using Concentrator.Objects.Models.Localization;
using System.Data.SqlClient;
using Concentrator.Objects.Environments;
using Concentrator.Objects.Models.Attributes;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Globalization;
using Microsoft.Practices.ServiceLocation;
using PetaPoco;
using Concentrator.Web.Services.Models.Attributes;

namespace Concentrator.Web.Services
{
  /// <summary>
  /// Summary description for Service1
  /// </summary>
  [WebService(Namespace = "http://Concentrator.diract-it.nl/")]
  [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
  [System.ComponentModel.ToolboxItem(false)]

  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  // [System.Web.Script.Services.ScriptService]
  public class AssortmentService
  {
    public AssortmentService() : base() { }

    [WebMethod(Description = "Get Assortment for connector by unique key", BufferResponse = false, EnableSession = true)]
    public XmlDocument GetAssortment(int connectorID, string concentratorProductID, bool showProductGroups)
    {
      return GetAssortmentContent(connectorID, false, false, concentratorProductID, showProductGroups);
    }

    [WebMethod(Description = "Get Assortment with full content for connector by unique key", BufferResponse = false)]
    public XmlDocument GetAssortmentContent(int connectorID, bool importFullContent, bool shopInformation, string concentratorProductID, bool showProductGroups)
    {
      return GetAdvancedPricingAssortment(connectorID, importFullContent, shopInformation, concentratorProductID, string.Empty, showProductGroups, 0, true);
    }

    [WebMethod(Description = "Get Assortment with full content and related products for connector by unique key", BufferResponse = false)]
    public XmlDocument GetAssortmentContentWithRelatedProducts(int connectorID, bool importFullContent, bool shopInformation, string concentratorProductID, bool showProductGroups)
    {
      return GetAdvancedPricingAssortment(connectorID, importFullContent, shopInformation, concentratorProductID, string.Empty, showProductGroups, 0, true);
    }

    [WebMethod(Description = "Get Advanced pricing assortment", BufferResponse = false)]
    public XmlDocument GetAdvancedPricingAssortment(int connectorID, bool importFullContent, bool shopInformation, string concentratorProductID, string customerID, bool showProductGroups, int languageID, bool showRelatedProducts = true)
    {
      PetaPoco.Database db = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient");
      try
      {
        using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {
          Connector con = db.Single<Connector>("SELECT * FROM Connector WHERE ConnectorID = @0", connectorID);


          DataSet customerAss = null;

          var repoProductGroupMapping = unit.Scope.Repository<ProductGroupMapping>();

          var productGroups = unit.Scope.Repository<ContentProductGroup>().GetAll(c => c.ConnectorID == connectorID).ToList();//db.Query<ContentProductGroup>("SELECT * FROM ContentProductGroup WHERE ConnectorID = @0", connectorID).ToList();

          var productGroupsList = (from p in productGroups
                                   group p by p.ProductID into grouped
                                   select grouped).ToDictionary(p => p.Key, r => r.ToList());

          var funcRepo = ((IFunctionScope)unit.Scope).Repository();

          var cStatuses = db.Query<ConnectorProductStatus>("SELECT * FROM ConnectorProductStatus WHERE ConnectorID = @0", connectorID).ToList();

          ProductStatusConnectorMapper mapper = new ProductStatusConnectorMapper(connectorID, cStatuses);
          ContentLogic logic = new ContentLogic(unit.Scope, connectorID);
          logic.FillRetailStock(db);

          var calculatedPrices = db.Query<Concentrator.Objects.Models.Prices.CalculatedPriceView>(
                @"EXECUTE GetCalculatedPriceView @0", connectorID);
          logic.FillPriceInformation(calculatedPrices);

          var connectorSystem = db.Single<ConnectorSystem>("SELECT * FROM ConnectorSystem WHERE ConnectorSystemID = @0", con.ConnectorSystemID.Value);

          bool magento = con.ConnectorSystemID.HasValue && connectorSystem.Name == "Magento";
          var defaultLanguage = db.FirstOrDefault<ConnectorLanguage>("SELECT * FROM ConnectorLanguage WHERE ConnectorID = @0", connectorID);

          defaultLanguage.ThrowArgNull("No default Language specified for connector");

          if (con != null)
          {
            if (languageID == 0)
            {
              //languageID = EntityExtensions.GetValueByKey(con.ConnectorSettings, "LanguageID", 2);
              var language = db.FirstOrDefault<ConnectorLanguage>("SELECT * FROM ConnectorLanguage WHERE ConnectorID = @0", connectorID);
              language.ThrowArgNull("No Language specified for connector");

              languageID = language.LanguageID;
            }


            var recordsSource = db.Query<AssortmentContentView>("EXECUTE GetAssortmentContentView @0", connectorID).Where(x => x.BrandID > 0);
            //var recordsSource = funcRepo.GetAssortmentContentView(connectorID).Where(x => x.BrandID > 0);

            if (!String.IsNullOrEmpty(concentratorProductID))
            {
              int id = 0;
              Int32.TryParse(concentratorProductID, out id);
              if (id > 0)
                recordsSource = recordsSource.Where(x => x.ProductID == id);
            }


            var records = recordsSource.ToList();

            var barcodes = db.Query<ProductBarcodeView>("SELECT * FROM ProductBarcodeView WHERE ConnectorID = @0 and BarcodeType = @1", connectorID, (int)BarcodeTypes.Default).ToList();


            var brands = db.Query<Brand>("SELECT * FROM Brand").ToList();


            List<ImageView> productImages = new List<ImageView>();
            if (importFullContent)
            {
              productImages = db.Query<ImageView>("SELECT * FROM ImageView WHERE ConnectorID = @0 AND ImageType = @1", connectorID, "Product").ToList();
            }

            int totalRecords = records.Count;


            SynchronizedCollection<ProductInfo> assortmentList = new SynchronizedCollection<ProductInfo>();


            ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = 8 };


            Parallel.ForEach(Partitioner.Create(0, totalRecords), options, (range, loopState) =>
            {
              List<ProductInfo> list = new List<ProductInfo>();
              for (int idx = range.Item1; idx < range.Item2; idx++)
              {

                var a = records[idx];
                var productBarcodes = barcodes.Where(x => x.ProductID == a.ProductID).ToList();
                var productImage = productImages.Where(x => x.ProductID == a.ProductID).ToList();

                list.Add(new ProductInfo
                {
                  ProductID = a.ProductID,
                  ManufacturerID = a.VendorItemNumber.Trim(),
                  Brand = a.BrandName,
                  BrandID = a.BrandID,
                  IsNonAssortmentItem = a.IsNonAssortmentItem,
                  BrandVendorCode = a.VendorBrandCode,
                  ShortDescription = a.ShortDescription,
                  LongDescription = a.LongDescription,
                  IsConfigurable = a.IsConfigurable,
                  Prices = logic.CalculatePrice(a.ProductID),
                  ConnectorProductID = a.CustomItemNumber,
                  QuantityAvailable = a.QuantityOnHand,
                  StockStatus = a.ConnectorStatus,
                  PromisedDeliveryDate = a.PromisedDeliveryDate,
                  QuantityToReceive = a.QuantityToReceive,
                  RetailStock = logic.RetailStock(a.ProductID, con),
                  Barcodes = productBarcodes,
                  Images = productImage,
                  LineType = a.LineType,
                  LedgerClass = a.LedgerClass,
                  ProductDesk = a.ProductDesk,
                  ExtendedCatalog = a.ExtendedCatalog,
                  VendorID = a.VendorID,
                  DeliveryHours = a.DeliveryHours,
                  CutOffTime = a.CutOffTime,
                  ProductName = !string.IsNullOrEmpty(a.ProductName.ToString()) ? a.ProductName.ToString() : a.ShortDescription
                });


              }

              foreach (var item in list)
              {

                assortmentList.Add(item);
              }

            });


            var assortment = assortmentList.Distinct().ToList();



            List<CrossLedgerclass> crossLedgerClasses = null;
            if (shopInformation)
            {
              crossLedgerClasses = db.Query<CrossLedgerclass>("SELECT * FROM CrossLedgerClass WHERE ConnectorID = @0", connectorID).ToList();
            }


            var vendorstocktypes = db.Query<VendorStockType>("SELECT * FROM VendorStockTypes").ToList();

            List<ProductGroupMapping> mappingslist = null;
            if (showProductGroups && magento)
              mappingslist = repoProductGroupMapping.Include(x => x.MagentoProductGroupSettings).GetAll(x => x.ConnectorID == connectorID).ToList();
            else if (showProductGroups)
              mappingslist = repoProductGroupMapping.GetAll(x => x.ConnectorID == connectorID).ToList();


            var preferredVendor = db.FirstOrDefault<PreferredConnectorVendor>("SELECT * FROM PreferredConnectorVendor WHERE ConnectorID = @0 AND IsPreferred = 1", connectorID);
            int? vendorid = null;
            if (preferredVendor != null) vendorid = preferredVendor.VendorID;


            string sql = String.Format("SELECT * FROM RelatedProduct RP WHERE (RP.VendorID IS NULL OR RP.VendorID = {0})", vendorid);


            var relatedProductTypeList = db.Query<RelatedProductType>("SELECT * FROM RelatedProductType").ToDictionary(x => x.RelatedProductTypeID, y => y.Type);

            var relatedProductsList = (from rp in db.Query<RelatedProduct>(sql)
                                       group rp by rp.ProductID into grouped
                                       select grouped).ToDictionary(x => x.Key, y => y.ToList());


            //(from rp in unit.Scope.Repository<RelatedProduct>().GetAllAsQueryable()
            //                         where !vendorid.HasValue || rp.VendorID == vendorid
            //                         group rp by rp.ProductID into grouped
            //                         select grouped).ToDictionary(x => x.Key, y => y.ToList());




            var configAttributesListSource = db.Query<ProductAttributeMetaData>(
                     @"SELECT ProductID, PAMD.*
                      FROM ProductAttributeConfiguration PAC
	                    INNER JOIN ProductAttributeMetaData PAMD ON (PAC.AttributeID = PAMD.AttributeID)"
                     );

            var configAttributesList = (from f in configAttributesListSource
                                        group f by f.ProductID into grouped
                                        select grouped).ToDictionary(x => x.Key, y => y.ToList());

            XElement element = new XElement("Assortment",
                                            from a in assortment
                                            let configAttributes = configAttributesList.ContainsKey(a.ProductID) ? configAttributesList[a.ProductID] : null
                                            let productProductGroups = productGroupsList.ContainsKey(a.ProductID) ? productGroupsList[a.ProductID] : new List<ContentProductGroup>()
                                            let relatedProducts = relatedProductsList.ContainsKey(a.ProductID) ? relatedProductsList[a.ProductID] : null
                                            where a.Prices != null
                                            select new XElement("Product",
                                              new XAttribute("IsNonAssortmentItem", a.IsNonAssortmentItem.HasValue ? a.IsNonAssortmentItem.Value.ToString() : "false"),
                                              new XAttribute("IsConfigurable", a.IsConfigurable),
                                              new XAttribute("ManufacturerID", a.ManufacturerID != null ? a.ManufacturerID.Trim() : string.Empty),
                                              new XAttribute("CustomProductID", !string.IsNullOrEmpty(a.ConnectorProductID) ? a.ConnectorProductID : string.Empty),
                                              new XAttribute("ProductID", a.ProductID),
                                              new XAttribute("LineType", !string.IsNullOrEmpty(a.LineType) ? a.LineType : string.Empty),
                                              new XAttribute("CutOffTime", a.CutOffTime.HasValue ? a.CutOffTime.Value.ToString("HH:mm") : string.Empty),
                                              new XAttribute("DeliveryHours", a.DeliveryHours.HasValue ? a.DeliveryHours.Value.ToString() : string.Empty),
                                                GetBrandsHierarchy(a.BrandID, a.BrandVendorCode, brands),
                                                showProductGroups ? GetProductGroupHierarchy(productProductGroups, languageID, unit.Scope.Repository<ProductGroupLanguage>(), mappingslist, magento, defaultLanguage.LanguageID) : null,

                                                (from pr in a.Prices
                                                 select new XElement("Price",
                                                   new XAttribute("TaxRate", pr.TaxRate.HasValue ? pr.TaxRate.Value : 19),
  new XAttribute("CommercialStatus", pr.Price.HasValue ? mapper.GetForConnector(pr.ConcentratorStatusID) : mapper.GetForConnector(6)),
  new XAttribute("MinimumQuantity", pr.MinimumQuantity.HasValue ? pr.MinimumQuantity.Value : 0),
                                                   new XElement("UnitPrice", pr.Price),
  new XElement("CostPrice", pr.CostPrice.HasValue ? pr.CostPrice.Value : 0),
  new XElement("SpecialPrice", pr.SpecialPrice.HasValue ? pr.SpecialPrice.Value : 0)
  )).Distinct(),
                                             new XElement("Stock",
                                             new XAttribute("InStock", a.QuantityAvailable),
                                            new XAttribute("PromisedDeliveryDate", (a.PromisedDeliveryDate.HasValue) ? a.PromisedDeliveryDate.Value.ToLocalTime().ToShortDateString() : string.Empty),
                                             new XAttribute("QuantityToReceive", a.QuantityToReceive.HasValue ? a.QuantityToReceive : 0),
  new XAttribute("StockStatus", string.IsNullOrEmpty(a.StockStatus) ? string.Empty : a.StockStatus)
  ,
  new XElement("Retail",
                                               (from r in a.RetailStock
                                                select new XElement("RetailStock",
                                                  new XAttribute("Name", r.VendorStockTypeID == 1 ? r.vendorName : vendorstocktypes.Where(x => x.VendorStockTypeID == r.VendorStockTypeID).Select(x => x.StockType).FirstOrDefault()),
                                                  new XAttribute("InStock", r.QuantityOnHand),
                                            new XAttribute("PromisedDeliveryDate", (r.PromisedDeliveryDate.HasValue) ? r.PromisedDeliveryDate.Value.ToLocalTime().ToShortDateString() : string.Empty),
                                            new XAttribute("QuantityToReceive", r.QuantityToReceive.HasValue ? r.QuantityToReceive : 0),
                                                  new XAttribute("VendorCode", !string.IsNullOrEmpty(r.BackendVendorCode) ? r.BackendVendorCode : string.Empty),
  new XAttribute("StockStatus", mapper.GetForConnector(r.ConcentratorStatusID)),
  new XAttribute("CostPrice", r.UnitCost.HasValue ? r.UnitCost : 0)
                                                  )).Distinct())
                                                  ),
                                             new XElement("Content",
                                             new XAttribute("ShortDescription", string.IsNullOrEmpty(a.ShortDescription) ? string.Empty : a.ShortDescription.Trim()),
                                             new XAttribute("LongDescription", a.LongDescription != null ? a.LongDescription.Trim() : string.Empty),
                                             new XAttribute("ProductName", a.ProductName ?? string.Empty)),
                                            new XElement("Barcodes",
                                              from b in a.Barcodes
                                              select new XElement("Barcode", b.Barcode ?? string.Empty)),
                                                            (showRelatedProducts && relatedProducts != null) ?
                                              new XElement("RelatedProducts",
                                              (from u in relatedProducts
                                               where u.ProductID == a.ProductID
                                               select new XElement("RelatedProduct",
                                                 new XAttribute("IsConfigured", u.IsConfigured),
                                                 new XAttribute("RelatedProductID", u.RelatedProductID),
                                                 new XAttribute("Type", relatedProductTypeList[u.RelatedProductTypeID]),
                                                 new XAttribute("RelatedProductTypeID", u.RelatedProductTypeID)))) : null,


  configAttributes != null ?
                                           new XElement("ConfigurableAttributes",

                                                 (from m in configAttributes
                                                  select new XElement("Attribute",
                                                  new XAttribute("AttributeCode", m.AttributeCode),
                                                  new XAttribute("AttributeID", m.AttributeID),
                                                  new XAttribute("ConfigurablePosition", m.ConfigurablePosition ?? 0)
                                                  ))) : null,

                            !shopInformation ? null : new XElement("ShopInformation",
                                               new XElement("LedgerClass", crossLedgerClasses.Where(x => x.LedgerclassCode == a.LedgerClass).Select(x => x.CrossLedgerclassCode).FirstOrDefault() != null ? crossLedgerClasses.Where(x => x.LedgerclassCode == a.LedgerClass).Select(x => x.CrossLedgerclassCode).FirstOrDefault() : a.LedgerClass),
                                               new XElement("OriginalLedgerClass", string.IsNullOrEmpty(a.LedgerClass) ? string.Empty : a.LedgerClass),
                                               new XElement("ProductDesk", string.IsNullOrEmpty(a.ProductDesk) ? string.Empty : a.ProductDesk),
                                               new XElement("ExtendedCatalog", a.ExtendedCatalog.HasValue ? a.ExtendedCatalog.Value.ToString() : false.ToString())),
                                               new XElement("ProductImages",
                                                 from pi in a.Images
                                                 select new XElement("ProductImage",
                 new XAttribute("ProductID", a.ProductID),
                 new XAttribute("BrandID", a.BrandID),
                 new XAttribute("ManufacturerID", string.IsNullOrEmpty(a.ManufacturerID) ? string.Empty : a.ManufacturerID),
                 new XAttribute("Sequence", pi.Sequence.HasValue ? pi.Sequence.Value : 0),
                 string.IsNullOrEmpty(pi.ImagePath) ? string.Empty : pi.ImagePath))
                                             ));


            var xmlDocument = new XmlDocument();
            using (var xmlReader = element.CreateReader())
            {
              xmlDocument.Load(xmlReader);
            }
            return xmlDocument;

          }
        }

        return new XmlDocument();
      }
      catch (Exception ex)
      {
        throw new Exception("ERROR", ex);
      }
    }

    private XElement GetBrandsHierarchy(int brandID, XElement element, List<Brand> brands, string brandvendorCode)
    {


      var brand = brands.FirstOrDefault(x => x.BrandID == brandID);
      var el = new XElement("Brand");
      if (brand != null)
      {
        el = new XElement("Brand",
                                                     new XAttribute("BrandID", brand.BrandID),
                                                     new XAttribute("ParentBrandID", brand.ParentBrandID.HasValue ? brand.ParentBrandID.Value : -1),
                                                     new XElement("Name", brand.Name),
                                                     new XElement("Code", brandvendorCode));

        element.Add(el);


        if (brand.ParentBrandID.HasValue)
        {
          GetBrandsHierarchy(brand.ParentBrandID.Value, el, brands, string.Empty);
        }
      }
      return element;
    }

    private XElement GetBrandsHierarchy(int brandID, string brandvendorCode, List<Brand> brands)
    {


      XElement element = new XElement("Brands");

      return GetBrandsHierarchy(brandID, element, brands, brandvendorCode);
    }

    [WebMethod(Description = "Get Assortment content descriptions for connector by unique key", BufferResponse = false)]
    public XmlDocument GetAssortmentContentDescriptions(int connectorID, string concentratorProductID)
    {
      return GetAssortmentContentDescriptionsByLanguage(connectorID, concentratorProductID, 0);
    }

    [WebMethod(Description = "Get Assortment content descriptions for connector by unique key", BufferResponse = false)]
    public XmlDocument GetAssortmentContentDescriptionsByLanguage(int connectorID, string concentratorProductID, int languageID)
    {
      try
      {
        using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {
          Connector con = unit.Scope.Repository<Connector>().GetSingle(x => x.ConnectorID == connectorID);

          if (con != null)
          {
            if (languageID < 1)
            {
              var language = con.ConnectorLanguages.FirstOrDefault();
              language.ThrowArgNull("No Language specified for connector");

              languageID = language.LanguageID;
            }


            var records = unit.Scope.Repository<ContentView>().GetAllAsQueryable(c => c.ConnectorID == connectorID && (c.LanguageID == languageID || c.LanguageID == 0) && c.BrandID > 0);

            if (!String.IsNullOrEmpty(concentratorProductID))
            {
              int id = 0;
              Int32.TryParse(concentratorProductID, out id);
              if (id > 0)
                records = records.Where(x => x.ProductID == id);
            }

            var assortment = (from a in records.ToList()
                              select new
                              {
                                a.ProductID,
                                ManufacturerID = a.VendorItemNumber,
                                Brand = a.BrandName,
                                BrandID = a.BrandID,
                                BrandVendorCode = a.VendorBrandCode,
                                a.ShortDescription,
                                a.LongDescription,
                                a.ShortContentDescription,
                                a.LongContentDescription,
                                a.ProductName,
                                a.ModelName,
                                a.WarrantyInfo
                              }).ToList();


            XElement element = new XElement("ProductContent",
                                            from a in assortment
                                            select new XElement("Product",
                                           new XAttribute("ManufacturerID", a.ManufacturerID.Trim()),
                                           new XAttribute("ProductID", a.ProductID),
new XElement("Brand",
                                              new XAttribute("BrandID", a.BrandID),
                                              new XElement("Name", a.Brand),
                                              new XElement("Code", a.BrandVendorCode)),
                                            new XElement("Content",
                                              new XAttribute("ProductName", a.ProductName != null ? a.ProductName.Trim() : string.Empty),
                                              new XAttribute("ModelName", a.ModelName != null ? a.ModelName.Trim() : string.Empty),
                                              new XAttribute("WarrantyInfo", a.WarrantyInfo != null ? a.WarrantyInfo.Trim() : string.Empty),
                                                new XAttribute("ShortDescription", a.ShortDescription != null ? a.ShortDescription.Trim() : string.Empty),
                                            new XAttribute("LongDescription", a.LongDescription != null ? a.LongDescription.Trim() : string.Empty),
                                             new XAttribute("ShortContentDescription", a.ShortContentDescription != null ? a.ShortContentDescription : string.Empty),
                                             new XAttribute("LongContentDescription", a.LongContentDescription != null ? a.LongContentDescription.Replace("\\n", "<br/>") : string.Empty))
                                            ));


            var xmlDocument = new XmlDocument();
            using (var xmlReader = element.CreateReader())
            {
              xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
          }
          else
          {
            return new XmlDocument();
          }


        }


      }
      catch (Exception ex)
      {
        throw new Exception("ERROR", ex);
      }
    }

    [WebMethod(Description = "Get short summary attribute list for connector", BufferResponse = false)]
    public string GetAssortmentAttributesSummary(int connectorID, int languageID, int customerID)
    {
      try
      {
        using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {


          Connector con = unit.Scope.Repository<Connector>().GetSingle(c => c.ConnectorID == connectorID);

          if (con != null)
          {
            DataSet customerAss = null;

            if (customerID != null && customerID > 0)
            {
              JdeAssortmentSoapClient soap = new JdeAssortmentSoapClient();
              customerAss = soap.GenerateFullProductList(customerID, 0, 0, 0, false, false);
            }

            var assortment = (from a in unit.Scope.Repository<Content>().GetAllAsQueryable()
                              join p in unit.Scope.Repository<Product>().GetAllAsQueryable() on a.ProductID equals p.ProductID
                              join pdc in unit.Scope.Repository<ProductDescription>().GetAllAsQueryable() on new { a.ProductID, VendorID = p.SourceVendorID }
                              equals new { pdc.ProductID, pdc.VendorID }


                              join pg in unit.Scope.Repository<ContentProductGroup>().GetAllAsQueryable() on
                                  new { a.ProductID, a.ConnectorID } equals new { pg.ProductID, pg.ConnectorID }
                              join pgn in unit.Scope.Repository<ProductGroupLanguage>().GetAllAsQueryable() on pg.ProductGroupMapping.ProductGroupID equals pgn.ProductGroupID
                              where a.ConnectorID == connectorID && pgn.LanguageID == languageID
                                && pdc.LanguageID == languageID

                              select new
                              {
                                Product = a.Product,
                                ShortDescription = pdc.ShortContentDescription,
                                LongDescription = pdc.ShortSummaryDescription,
                                ContentDescription = pdc.LongContentDescription,
                                ProductGroupID = pg.ProductGroupMapping.ProductGroupID,
                                ProductGroupName = pgn.Name
                              });


            var result = (from a in assortment
                          group a by a.Product
                            into grouped
                            select new
                            {
                              VendorItemNumber = grouped.Key.VendorItemNumber,
                              Brand = grouped.Key.Brand,
                              BrandID = grouped.Key.BrandID,
                              ProductGroupID = grouped.First().ProductGroupID,
                              //ParentProductGroupID = grouped.First().ParentProductGroupID,
                              grouped.Key.ProductID,
                              ProductGroupName = grouped.First().ProductGroupName,
                              grouped.First().ShortDescription,
                              grouped.First().LongDescription,
                              grouped.First().ContentDescription
                            }
                         );




            List<Attributes> list = new List<Attributes>();


            //var attributes = ((IFunctionScope)unit.Scope).Repository().GetProductAttributes(null, languageID, con.ConnectorID, null);

            var productattributes = (from a in unit.Scope.Repository<ContentAttribute>().GetAll()
                                     where a.LanguageID == languageID
                                      && (a.ConnectorID == null || a.ConnectorID == con.ConnectorID)
                                     select a).ToList();

            foreach (var product in result)
            {
              DataRow row = null;

              if (customerAss != null)
              {
                row = customerAss.Tables[0].AsEnumerable().Where(x => x.Field<string>("VendorItemNumber").Trim().ToUpper() == product.VendorItemNumber).FirstOrDefault();
                if (row == null)
                  continue;
              }

              var p = product;



              Attributes att = new Attributes();
              att.ProductID = p.ProductID;
              att.ManufacturerID = p.VendorItemNumber;
              att.BrandID = p.BrandID;
              att.ShortDescription = p.ShortDescription;
              att.LongDescription = p.LongDescription;
              att.ContentDescription = p.ContentDescription;
              if (row != null)
              {
                att.IntraStatCode = row.Field<string>("IntraStatCode");
                att.ModelName = row.Field<string>("ModelName");
              }
              att.AttributeList = (from a in productattributes
                                   where !a.ProductID.HasValue || (a.ProductID.HasValue && a.ProductID == p.ProductID)
                                   group a by a.AttributeCode
                                     into grouped
                                     select grouped.First()).ToList();

              list.Add(att);
            }


            using (var stringWriter = new StringWriterWithEncoding(Encoding.UTF8))
            {

              using (var writer = new XmlTextWriter(stringWriter) { Formatting = Formatting.None })
              {

                writer.WriteStartDocument(true);
                writer.WriteStartElement("ProductAttributes");



                foreach (var a in list.Where(x => x.AttributeList.Count > 0))
                {

                  List<XElement> contentAttributes = new List<XElement>();
                  contentAttributes.Add(
                    new XElement("Attribute",
                                 new XAttribute("AttributeCode", "ShortDescription"),
                                 new XAttribute("Type", "Basic"),
                                 new XElement("Name", "ShortDescription"),
                                 new XElement("Value", HttpUtility.HtmlEncode(a.ShortDescription)),
                                 new XElement("Sign", String.Empty)
                      ));

                  contentAttributes.Add(
                 new XElement("Attribute",
                              new XAttribute("AttributeCode", "LongDescription"),
                              new XAttribute("Type", "Basic"),
                              new XElement("Name", "LongDescription"),
                              new XElement("Value", HttpUtility.HtmlEncode(a.LongDescription)),
                              new XElement("Sign", String.Empty)
                   ));

                  contentAttributes.Add(
                 new XElement("Attribute",
                              new XAttribute("AttributeCode", "Content"),
                                                            new XAttribute("Type", "Basic"),

                              new XElement("Name", "Content"),
                              new XElement("Value", HttpUtility.HtmlEncode(a.ContentDescription)),
                              new XElement("Sign", String.Empty)
                   ));

                  var productAttributes = (from p in a.AttributeList
                                           //where !string.IsNullOrEmpty(p.AttributeValue)
                                           select new XElement("Attribute",
                                                               new XAttribute("AttributeCode",
                                                                              !string.IsNullOrEmpty(
                                                                                 p.AttributeCode)
                                                                                ? p.AttributeCode
                                                                                : string.Empty),
new XAttribute("Type", "Specifications"),
                                                               new XElement("Name",
                                                                            HttpUtility.HtmlEncode(
                                                                              p.AttributeName)),
                                                               new XElement("Value",
                                                                            HttpUtility.HtmlEncode(!string.IsNullOrEmpty(
                                                                               p.AttributeValue)
                                                                              ? p.AttributeValue
                                                                              : string.Empty)),
                                                               new XElement("Sign",
                                                                            HttpUtility.HtmlEncode(
                                                                              !string.IsNullOrEmpty(
                                                                                 p.Sign)
                                                                                ? p.Sign
                                                                                : string.Empty))
                                             )).Distinct();


                  productAttributes = productAttributes.Union(contentAttributes);

                  #region Element
                  var element = new XElement("Product",
                                             new XAttribute("ProductID", a.ProductID),
                                             new XAttribute("ManufacturerID", a.ManufacturerID),
                                             new XAttribute("ModelName", !string.IsNullOrEmpty(a.ModelName) ? a.ModelName : string.Empty),
                                             new XAttribute("IntraStatCode", !string.IsNullOrEmpty(a.IntraStatCode) ? a.IntraStatCode : string.Empty),
                                             new XElement("Attributes", productAttributes));

                  element.WriteTo(writer);
                  writer.Flush();
                  #endregion

                }


                writer.WriteEndElement(); // ProductAttributes
                writer.WriteEndDocument();


                writer.Flush();
              }

              stringWriter.Flush();
              return stringWriter.ToString();
            }


          }
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }

      return String.Empty;

    }

    [WebMethod(Description = "Get Assortment for connector by unique key", BufferResponse = false)]
    public XmlDocument GetAttributesAssortment_TESTFull(int connectorID)
    {
      return GetAttributesAssortment(connectorID, null, null);
    }

    [WebMethod(Description = "Get Assortment for connector by unique key", BufferResponse = false)]
    public XmlDocument GetAttributesAssortment_TEST(int connectorID, int pid)
    {
      return GetAttributesAssortment(connectorID, new int[] { pid }, null);
    }

    [WebMethod(Description = "Get Assortment for connector by unique key", BufferResponse = false)]
    public XmlDocument GetAttributesAssortment(int connectorID, int[] ProductIDs, DateTime? LastUpdate)
    {
      return GetAttributesAssortmentByLanguage(connectorID, ProductIDs, LastUpdate, 0);
    }

    public XDocument GetAttributesAssortmentByLanguagePartial(int connectorID, DateTime? LastUpdate, int languageID)
    {
      var imageUrl = new Uri(ConfigurationManager.AppSettings["ConcentratorImageUrl"].ToString());

      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {

        db.CommandTimeout = 15 * 60;

        if (languageID < 1)
        {
          languageID = db.ExecuteScalar<int>(@"Select top 1 languageid from connectorlanguage where connectorid = @0", connectorID);
        }

        var connector = db.FirstOrDefault<Connector>(@"select * from connector where connectorid = @0", connectorID);

        var query = string.Format(@";with groups as (
	                                      select pavg.*, pav.Score, pav.ImagePath from
	                                      ProductAttributeValueConnectorValueGroup pavg 
	                                      
	                                      inner join productattributevaluegroup pav on pav.AttributeValueGroupID = pavg.AttributeValueGroupID and (pav.ConnectorID = pavg.connectorid or pav.connectorid = {2})
	                                      where pavg.connectorid = {0} or pavg.connectorid = {2}
                                      )

                                      select 
	                                      p.VendorItemNumber as ManufacturerID, 
	                                      p.ProductID, 
	                                      p.BrandID, 
	                                      ca.GroupID as AttributeGroupID, 
	                                      ca.GroupIndex as AttributeGroupIndex, 
	                                      ca.GroupName as AttributeGroupName,
	                                      ca.IsConfigurable,
	                                      ca.AttributeID,
	                                      ca.AttributeValueID,
	                                      ca.AttributeOriginalValue as OriginalValue, 
	                                      ca.AttributeCode as AttributeCode ,
	                                      ca.OrderIndex as [Index],
	                                      ca.IsSearchable,
	                                      ca.NeedsUpdate,
	                                      ca.AttributeName,
	                                      ca.AttributeValue as Value,
	                                      ca.Sign,
	                                      (case 
		                                      when 
			
			                                      (ca.vendorid in (
				                                      select vendorid from PreferredConnectorVendor where connectorid = ca.ConnectorID and isContentVisible = 1
			                                      ) and 
			                                      ca.IsVisible = 1 )			
			                                      then cast(1 as bit) 			
		                                      else 
			                                      ca.IsVisible 
	                                      end
	                                      ) as KeyFeature,
	                                      ca.AttributePath,

	                                      valueGroup.AttributeValueGroupID as ProductAttributeValueGroupID,
	                                      valueGroup.Score as ProductAttributeValueGroupScore,
	                                      valueGroupName.Name as ProductAttributeValueGroupName,
	                                      valueGroup.ImagePath as ProductAttributeValueGroupImage

	                                      from Content c 
                                      inner join product p on c.productid = p.productid
                                      left join ContentAttribute CA on ca.productid = c.productid and ca.connectorid = c.connectorid and ca.languageid = {1}
                                      inner join connector ctor on ctor.connectorid = c.connectorid 
                                      left join groups valueGroup on valuegroup.Value = ca.AttributeOriginalValue and ca.attributeid = valuegroup.attributeid
                                      left join productattributevaluegroupname valueGroupName on valueGroupName.AttributeValueGroupID = valueGroup.AttributeValueGroupID and valuegroupname.LanguageID = {1}

                                      where c.connectorid = {0}
                                      order by productid", connectorID, languageID, connector.ParentConnectorID ?? 0);

        var attributeResult = db.Query<dynamic>(query).ToList();



        SortedList<int, ProductAttributeValueGroupResult> filterGroups = new SortedList<int, ProductAttributeValueGroupResult>();
        SortedList<int, ProductAttributeResult> productAttributes = new SortedList<int, ProductAttributeResult>();

        foreach (var result in attributeResult)
        {
          int? attributeID = (int?)result.AttributeID;

          if (!attributeID.HasValue)
            continue;

          int productID = (int)result.ProductID;

          ProductAttributeResult productResult = null;
          if (productAttributes.ContainsKey(productID))
            productResult = productAttributes[productID];
          else
          {
            productResult = new ProductAttributeResult()
            {
              ProductID = productID,
              ManufacturerID = (string)result.ManufacturerID,
              BrandID = (int)result.BrandID
            };

            productAttributes.Add(productID, productResult);
          }

          if (!attributeID.HasValue)
            continue;

          //filterGroups
          var productAttributeValueGroupID = (int?)result.ProductAttributeValueGroupID;
          if (productAttributeValueGroupID.HasValue)
          {
            if (!filterGroups.ContainsKey(productAttributeValueGroupID.Value))
            {
              filterGroups.Add(productAttributeValueGroupID.Value, new ProductAttributeValueGroupResult
              {
                AttributeID = attributeID.Value,
                ProductAttributeValueGroupID = productAttributeValueGroupID.Value,
                ProductAttributeValueGroupImage = (string)result.ProductAttributeValueGroupImage,
                ProductAttributeValueGroupScore = (int?)result.ProductAttributeValueGroupScore,
                ProductAttributeValueGroupName = (string)result.ProductAttributeValueGroupName,
              });
            }
          }
          //end of filter groups


          ProductAttributeValuesResult valueResult = null;
          //attributes
          if (!productResult.ProductAttributeValues.ContainsKey(attributeID.Value))
          {

            valueResult = new ProductAttributeValuesResult()
            {
              AttributeCode = (string)result.AttributeCode,
              Value = (string)result.Value,
              Sign = (string)result.Sign,
              IsSearchable = (bool)result.IsSearchable,
              AttributeGroupID = (int)result.AttributeGroupID,
              AttributeGroupIndex = (int)result.AttributeGroupIndex,
              AttributeGroupName = (string)result.AttributeGroupName,
              AttributeID = attributeID.Value,
              AttributePath = (string)result.AttributePath,
              AttributeValueID = (int)result.AttributeValueID,
              Index = (int)result.Index,
              IsConfigurable = (bool)result.IsConfigurable,
              KeyFeature = (bool)result.KeyFeature,
              Name = (string)result.AttributeName,
              NeedsUpdate = (bool)result.NeedsUpdate,
              OriginalValue = (string)result.OriginalValue
            };
            productResult.ProductAttributeValues.Add(attributeID.Value, valueResult);
          }
          else
          {
            valueResult = productResult.ProductAttributeValues[attributeID.Value];
          }
          //end of attributes

          if (productAttributeValueGroupID.HasValue && !valueResult.AttributeValueGroupIDs.Contains(productAttributeValueGroupID.Value))
            valueResult.AttributeValueGroupIDs.Add(productAttributeValueGroupID.Value);
        }

        SynchronizedCollection<XElement> productElements = new SynchronizedCollection<XElement>();
        ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = 4 };

        if (productAttributes.Count == 0)
        {
          return null;
        }

        Parallel.ForEach(Partitioner.Create(0, productAttributes.Count), options,
          (range, loopState) =>
          {
            List<XElement> inPartitionList = new List<XElement>();
            for (int i = range.Item1; i < range.Item2; i++)
            {
              var productAttribute = productAttributes.ElementAt(i).Value;

              #region Product attribute xml element
              var element = new XElement("ProductAttribute",
                                        new XAttribute("ProductID", productAttribute.ProductID),
                                        new XAttribute("ManufacturerID", productAttribute.ManufacturerID),
                                        new XElement("Brand", new XAttribute("BrandID", productAttribute.BrandID)),
                                        new XElement("AttributeGroups", (from pag in productAttribute.ProductAttributeValues
                                                                         .Values
                                                                         .Select(c => new { c.AttributeGroupID, c.AttributeGroupIndex, c.AttributeGroupName })
                                                                         .Distinct()


                                                                         select new XElement("AttributeGroup",
                                                                                                new XAttribute("AttributeGroupID",
                                                                                                               pag.AttributeGroupID),
                                                                                                new XAttribute("AttributeGroupIndex",
                                                                                                               pag.AttributeGroupIndex),
                                                                                                new XAttribute("Name",
                                                                                                             pag.AttributeGroupName))
                                                                           )),
                                        new XElement("Attributes",
                                          (from p in productAttribute.ProductAttributeValues.Values
                                           select new XElement("Attribute",
                                                                 new XAttribute("IsConfigurable", p.IsConfigurable),
                                                                new XAttribute("ConfigurableAttributeType", p.IsConfigurable), //deprecated. Kept only not to break xml
                                                                new XAttribute("AttributeID", p.AttributeID),
                                                                 new XAttribute("AttributeValueID", p.AttributeValueID),
                                                                new XAttribute("AttributeOriginalValue", p.OriginalValue),
                                                                 new XAttribute("AttributeCode", !string.IsNullOrEmpty(p.AttributeCode) ? p.AttributeCode : string.Empty),
                                                                 new XAttribute("KeyFeature", p.KeyFeature || p.IsConfigurable),
                                                                 new XAttribute("Index", p.Index),
                                                                          new XAttribute("IsSearchable", p.IsSearchable),
                                                                           new XAttribute("AttributeGroupID", p.AttributeGroupID),
                                                                          new XAttribute("NeedsUpdate", p.NeedsUpdate),
                                                                          new XElement("Name", HttpUtility.HtmlEncode(p.Name)),
                                                                          new XElement("Value",
                                                                                       !string.IsNullOrEmpty(
                                                                                          p.Value)
                                                                                         ? p.Value
                                                                                         : string.Empty),
                                                                          new XElement("Sign",
                                                                                       HttpUtility.HtmlEncode(
                                                                                         !string.IsNullOrEmpty(
                                                                                            p.Sign)
                                                                                           ? p.Sign
                                                                                           : string.Empty)),
                                                                                            new XElement("Image", !string.IsNullOrEmpty(p.AttributePath) ? string.Format("{0}", p.AttributePath) : string.Empty

                                        ),
                                        new XElement("AttributeValueGroups",
                                           (from f in p.AttributeValueGroupIDs
                                            select new XElement("Group", new XAttribute("ID", f))
                                          )
                                        )))));
              #endregion

              inPartitionList.Add(element);
            }
            foreach (var element in inPartitionList) productElements.Add(element);
          });

        XElement docElement = new XElement("ProductAttributes");
        docElement.Add(productElements.ToArray());

        var atributeFilterGroup = new XElement("AttributeValueGroups", (from pag in filterGroups.Values
                                                                        select new XElement("AttributeValueGroup",
                                                                                       new XAttribute("AttributeValueGroupID",
                                                                                                      pag.ProductAttributeValueGroupID),
                                                                                                       new XAttribute("AttributeID",
                                                                                                      pag.AttributeID),
                                                                                       new XAttribute("Score",
                                                                                                      pag.ProductAttributeValueGroupScore),
                                                                                       new XAttribute("Name",
                                                                                                    pag.ProductAttributeValueGroupName),
                                                                                                    new XAttribute("Image", string.IsNullOrEmpty(pag.ProductAttributeValueGroupImage) ? string.Empty : new Uri(imageUrl, pag.ProductAttributeValueGroupImage).ToString())
                                                                                                    )
                                                                        ));


        docElement.Add(atributeFilterGroup);
        return new XDocument(docElement);
      }
    }

    [WebMethod(Description = "Get Assortment for connector by unique key for specified language", BufferResponse = false)]
    public XmlDocument GetAttributesAssortmentByLanguage(int connectorID, int[] ProductIDs, DateTime? LastUpdate, int languageID, bool onlyKeyFeatures = false)
    {
      try
      {
        var imageUrl = new Uri(ConfigurationManager.AppSettings["ConcentratorImageUrl"].ToString());

        using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {
          Connector con = unit.Scope.Repository<Connector>().GetSingle(c =>
                           c.ConnectorID == connectorID);

          if (con != null)
          {
            if (languageID < 1)
            {
              if (con.ConnectorLanguages.Count > 0)
                languageID = con.ConnectorLanguages.FirstOrDefault().LanguageID;
              else
                languageID = con.ConnectorSettings.GetValueByKey<int>("LanguageID", 2);
            }

            var contentAss = (from a in unit.Scope.Repository<Content>().GetAllAsQueryable()
                              where a.ConnectorID == connectorID
                              select new
                              {
                                VendorItemNumber = a.Product.VendorItemNumber,
                                Brand = a.Product.Brand,
                                BrandID = a.Product.BrandID,
                                ParentProductGroupID = 0,
                                a.ProductID,
                                CreationTime = a.CreationTime
                              });


            var tmpAssortment = (from a in contentAss
                                 select a).AsEnumerable();

            if (ProductIDs != null && ProductIDs.Count() > 0)
              tmpAssortment = (from a in contentAss.ToList().AsQueryable().InRange(x => x.ProductID, 1500, ProductIDs)
                               select a);

            var assortment = tmpAssortment.ToList();

            List<Attributes> list = new List<Attributes>();

            List<ContentAttribute> attributesSource = new List<ContentAttribute>();

            if (LastUpdate.HasValue)
            {
              var attributeList = (from a in unit.Scope.Repository<ContentAttribute>().GetAllAsQueryable()
                                   join p in unit.Scope.Repository<Product>().GetAllAsQueryable() on a.ProductID equals p.ProductID
                                   join c in unit.Scope.Repository<Content>().GetAllAsQueryable() on a.ProductID equals c.ProductID
                                   where c.ConnectorID == connectorID
                                    && (a.ConnectorID == null || a.ConnectorID == connectorID)
                                    && a.LanguageID == languageID
                                   //&& (!exportOnlyVisible || (exportOnlyVisible && a.IsVisible))
                                   select new
                                   {
                                     ContentAttribute = a,
                                     ProductCreationTime = p.CreationTime,
                                     ContentCreationTime = c.CreationTime
                                   });

              DateTime ealier = LastUpdate.Value.AddDays(-1);

              attributeList = attributeList.Where(x => x.ProductCreationTime >= ealier
                || (x.ContentAttribute.LastUpdate.HasValue && x.ContentAttribute.LastUpdate.Value >= LastUpdate.Value)
                || x.ProductCreationTime.ToLocalTime() >= LastUpdate.Value
                );

              if (ProductIDs == null || ProductIDs.Count() > 10)
              {
                attributesSource = attributeList.Select(x => x.ContentAttribute).ToList();
              }
              else
              {
                ProductIDs.ForEach((prod, idx) =>
                {
                  attributesSource.AddRange(attributeList.Where(x => !x.ContentAttribute.ProductID.HasValue || x.ContentAttribute.ProductID.Value == prod).Select(x => x.ContentAttribute).Distinct().ToList());
                });
              }
            }
            else
            {
              var attributeList = (from a in unit.Scope.Repository<ContentAttribute>().GetAll(x => x.ConnectorID == connectorID
                                    && x.LanguageID == languageID)
                                   //&& (!exportOnlyVisible || (exportOnlyVisible && x.IsVisible)))
                                   select a);


              if (ProductIDs == null || ProductIDs.Count() > 10)
              {
                attributesSource = attributeList.ToList();
              }
              else
              {
                ProductIDs.ForEach((prod, idx) =>
                {
                  attributesSource.AddRange(attributeList.Where(x => !x.ProductID.HasValue || x.ProductID.Value == prod).ToList());
                });
              }
            }

            var blankAttributes = (from a in attributesSource
                                   where !a.ProductID.HasValue
                                   select a).ToArray();

            Dictionary<int, List<ContentAttribute>> attributes = (from a in attributesSource
                                                                  where a.ProductID.HasValue
                                                                  group a by a.ProductID.Value
                                                                    into grouped
                                                                    select grouped).ToDictionary(x => x.Key,
                                                                    y => y.GroupBy(a => a.AttributeID).Select(g => g.First()).ToList());

            //var productattributes = attributes.ToList();

            assortment.ForEach(product =>
            {
              var p = product;

              Attributes att = new Attributes();
              att.ProductID = p.ProductID;
              att.ManufacturerID = p.VendorItemNumber;
              att.BrandID = p.BrandID;

              List<ContentAttribute> pa = null;
              if (attributes.TryGetValue(p.ProductID, out pa))
                att.AttributeList = pa;
              else
                att.AttributeList = new List<ContentAttribute>();

              list.Add(att);
            });

            var attributeGroups = (from att in list.SelectMany(x => x.AttributeList)
                                   select new ImportAttributeGroup
                                   {
                                     AttributeGroupID = att.GroupID,
                                     AttributeGroupIndex = att.GroupIndex,
                                     AttributeGroupName = att.GroupName
                                   }).Distinct().ToList();


            var filterGroups = (from fg in unit.Scope.Repository<ProductAttributeValueConnectorValueGroup>().GetAll(x => !x.ConnectorID.HasValue || x.ConnectorID == connectorID || (con.ParentConnectorID.HasValue && x.ConnectorID == con.ParentConnectorID))
                                join gn in unit.Scope.Repository<ProductAttributeValueGroupName>().GetAll(x => x.LanguageID == languageID) on fg.AttributeValueGroupID equals gn.AttributeValueGroupID
                                select new
                                {
                                  AttributeGroupValueID = fg.AttributeValueGroupID,
                                  AttributeID = fg.AttributeID,
                                  GroupScore = fg.ProductAttributeValueGroup.Score,
                                  Name = gn.Name,
                                  Value = fg.Value,
                                  ImagePath = fg.ProductAttributeValueGroup != null ? fg.ProductAttributeValueGroup.ImagePath : string.Empty
                                }).Distinct().ToList();


            var attributeValueGroups = (from fg in filterGroups
                                        join ca in list.SelectMany(x => x.AttributeList) on fg.Value.Trim() equals ca.AttributeOriginalValue.Trim()
                                        where fg.AttributeID == ca.AttributeID
                                        group fg by new { ca.AttributeID, fg.AttributeGroupValueID, fg.GroupScore, fg.Name, fg.ImagePath } into grouped
                                        select new
                                        {
                                          AttributeGroupValueID = grouped.Key.AttributeGroupValueID,
                                          AttributeGroupValueScore = grouped.Key.GroupScore,
                                          AttributeGroupValueName = grouped.Key.Name,
                                          AttributeID = grouped.Key.AttributeID,
                                          ImagePath = grouped.Key.ImagePath
                                        }).ToList();



            var contentVisible = unit.Scope.Repository<PreferredConnectorVendor>().GetAllAsQueryable(cv => cv.ConnectorID == connectorID).ToDictionary(x => x.VendorID, x => x.isContentVisible);


            var imageValueIds = (from att in list.SelectMany(x => x.AttributeList)
                                 where att.AttributeValueID.HasValue
                                 && !String.IsNullOrEmpty(att.AttributePath)
                                 group att by att.AttributeID into grouped
                                 select grouped).ToDictionary(c => c.Key, b => (int)b.First().AttributeValueID.Value);


            var configurableAttributeTypes = (from p in unit.Scope.Repository<Product>().GetAll(x => x.IsConfigurable)
                                              join a in unit.Scope.Repository<ContentAttribute>().GetAll(x => x.ConnectorID == connectorID) on p.ProductID equals a.ProductID
                                              select a.AttributeID).ToList();


            XElement docElement = new XElement("ProductAttributes");

            list.Where(x => x.AttributeList.Count > 0).ForEach((a, idx) =>
            {
              var groupIds = a.AttributeList.Select(x => x.GroupID).Distinct().ToList();
              var productAttributeGroups = attributeGroups.Where(x => groupIds.Contains(x.AttributeGroupID));

              #region Element
              var element = new XElement("ProductAttribute",
                                         new XAttribute("ProductID", a.ProductID),
                                         new XAttribute("ManufacturerID", a.ManufacturerID),
                                         new XElement("Brand", new XAttribute("BrandID", a.BrandID)),
                                         new XElement("AttributeGroups", (from pag in productAttributeGroups


                                                                          select new XElement("AttributeGroup",
                                                                                                 new XAttribute("AttributeGroupID",
                                                                                                                pag.AttributeGroupID),
                                                                                                 new XAttribute("AttributeGroupIndex",
                                                                                                                pag.AttributeGroupIndex),
                                                                                                 new XAttribute("Name",
                                                                                                              pag.AttributeGroupName))
                                                                            )),


                                         new XElement("Attributes",
                                                      (from p in a.AttributeList
                                                       let attributeValueIDforImage = imageValueIds.ContainsKey(p.AttributeID) ? imageValueIds[p.AttributeID].ToString() : String.Empty
                                                       let filterGroup = filterGroups.Where(x => x.AttributeID == p.AttributeID && x.Value == p.AttributeOriginalValue)
                                                       let keyFeature = contentVisible.ContainsKey(p.VendorID.Value) ? (p.IsVisible && contentVisible[p.VendorID.Value] ? p.IsVisible : false) : p.IsVisible
                                                       where ((onlyKeyFeatures && (keyFeature || p.IsConfigurable)) || !onlyKeyFeatures)
                                                       select new XElement("Attribute",
                                                                           new XAttribute("IsConfigurable", p.IsConfigurable),
                                                                           new XAttribute("ConfigurableAttributeType", configurableAttributeTypes.Contains(p.AttributeID) ? true : false),
                                                                           new XAttribute("AttributeID",
                                                                                          p.AttributeID),
                                                                           new XAttribute("AttributeValueID", p.AttributeValueID),
                                                         //new XAttribute("ConfigurablePosition", p.ConfigurablePosition ?? 0),
                                                                           new XAttribute("AttributeOriginalValue", p.AttributeOriginalValue),
                                                                           new XAttribute("AttributeCode",
                                                                                          !string.IsNullOrEmpty(p.AttributeCode) ? p.AttributeCode : string.Empty
                                                                                           ),

                                                                           new XAttribute("KeyFeature", keyFeature || p.IsConfigurable),
                                                                           new XAttribute("Index", p.OrderIndex),
                                                                           new XAttribute("IsSearchable", p.IsSearchable),
                                                                            new XAttribute("AttributeGroupID", p.GroupID),
                                                                           new XAttribute("NeedsUpdate", p.NeedsUpdate),
                                                                           new XElement("Name", HttpUtility.HtmlEncode(p.AttributeName)),
                                                                           new XElement("Value",
                                                                                        !string.IsNullOrEmpty(
                                                                                           p.AttributeValue)
                                                                                          ? p.AttributeValue
                                                                                          : string.Empty),
                                                                           new XElement("Sign",
                                                                                        HttpUtility.HtmlEncode(
                                                                                          !string.IsNullOrEmpty(
                                                                                             p.Sign)
                                                                                            ? p.Sign
                                                                                            : string.Empty)),

                                         new XElement("Image", !string.IsNullOrEmpty(p.AttributePath) ? string.Format("{0}", p.AttributePath) : string.Empty,
                                                         //                                             new XAttribute("ValueImage", !string.IsNullOrEmpty(p.AttributePath) ? string.Format("{0}AttributeImage.ashx?ProductAttributeValueID={1}", BaseSiteUrl, attributeValueIDforImage) : string.Empty)
                                          new XAttribute("ValueImage", !string.IsNullOrEmpty(p.AttributePath) ? string.Format("{0}AttributeImage.ashx?ProductAttributeValueID={1}", BaseSiteUrl, attributeValueIDforImage) : string.Empty)
                                         ),
                                         new XElement("AttributeValueGroups",
                                            (from f in filterGroup
                                             select new XElement("Group", new XAttribute("ID", f.AttributeGroupValueID))
                                           )
                                         )))
                                    ));

              docElement.Add(element);
              #endregion
            });
            #region Element
            var atributeFilterGroup = new XElement("AttributeValueGroups", (from pag in attributeValueGroups
                                                                            select new XElement("AttributeValueGroup",
                                                                                           new XAttribute("AttributeValueGroupID",
                                                                                                          pag.AttributeGroupValueID),
                                                                                                           new XAttribute("AttributeID",
                                                                                                          pag.AttributeID),
                                                                                           new XAttribute("Score",
                                                                                                          pag.AttributeGroupValueScore),
                                                                                           new XAttribute("Name",
                                                                                                        pag.AttributeGroupValueName),
                                                                                                        new XAttribute("Image", string.IsNullOrEmpty(pag.ImagePath) ? string.Empty : new Uri(imageUrl, pag.ImagePath).ToString())
                                                                                                        )
                                                                          ));


            docElement.Add(atributeFilterGroup);

            #endregion


            var xmlDocument = new XmlDocument();
            using (var xmlReader = docElement.CreateReader())
            {
              xmlDocument.Load(xmlReader);
            }
            return xmlDocument;

          }


        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
      return new XmlDocument();
    }

    [WebMethod(Description = "Get VendorItemNumber", BufferResponse = false)]
    public string GetVendorItemNumber(int concentratorProductID, string manufacturerID, string brand, int connectorID)
    {
      using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
      {

        var _assortmentRepo = unit.Scope.Repository<VendorAssortment>();


        var vendorID = (from c in unit.Scope.Repository<PreferredConnectorVendor>().GetAllAsQueryable()
                        where c.ConnectorID == connectorID && c.isPreferred
                        select c.VendorID).FirstOrDefault();


        if (vendorID != null)
        {
          if (concentratorProductID > 0)
          {
            return (from v in _assortmentRepo.GetAllAsQueryable()
                    where v.VendorID == vendorID && v.ProductID == concentratorProductID
                    select v.CustomItemNumber).FirstOrDefault();
          }
          else
          {
            var product = (from p in unit.Scope.Repository<Product>().GetAllAsQueryable()
                           where p.Brand.Name == brand
                           && p.VendorItemNumber == manufacturerID
                           select p).FirstOrDefault();

            return (from a in _assortmentRepo.GetAllAsQueryable()
                    join v in unit.Scope.Repository<Vendor>().GetAllAsQueryable() on a.VendorID equals v.VendorID
                    where (v.VendorID == vendorID || v.ParentVendorID == vendorID)
                    && a.ProductID == product.ProductID
                    select a.CustomItemNumber).FirstOrDefault();
          }
        }

        return null;
      }
    }

    [WebMethod(Description = "Get Images for connector by unique key directly from vendor")]
    public string GetAssortmentImagesForProduct(int connectorID, int productid)
    {
      using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
      {
        XmlDocument xml = new XmlDocument();
        try
        {
          Connector con = unit.Scope.Repository<Connector>().GetSingle(c => c.ConnectorID == connectorID);
          con.ThrowIfNull("Connector can't be null");

          List<ImageClass> images = new List<ImageClass>();

          var assortment = unit.Scope.Repository<ImageView>().GetAll(x => x.ConnectorID == connectorID && x.ProductID == productid
).ToList();

          var imageUrl = new Uri(ConfigurationManager.AppSettings["ConcentratorImageUrl"].ToString());
          var thumbUrl = new Uri(ConfigurationManager.AppSettings["ConcentratorThumbImageUrl"].ToString());

          var thumbs = (from p in unit.Scope.Repository<ProductMediaTumbnail>().GetAll()
                        where p.ProductMedia.ProductID == productid
                        select new
                        {
                          p.ProductMedia.MediaID,
                          p.ThumbnailGenerator,
                          p.Path
                        }).ToList();

          List<ImageClass> imageList = new List<ImageClass>();

          XElement doc = new XElement("ProductMedia",
              new XElement("Products",
              from p in assortment
              let uri = new Uri(imageUrl, p.ImagePath)
              let thumbnails = thumbs.Where(x => x.MediaID == p.mediaid)
              select new XElement("ProductMedia",
                  new XAttribute("MediaID", p.mediaid),
                  new XAttribute("ProductID", p.ProductID),
                  new XAttribute("Description", p.Description ?? string.Empty),
                  new XAttribute("BrandID", p.BrandID),
                  new XAttribute("ManufacturerID", p.ManufacturerID),
                  new XAttribute("Sequence", p.Sequence),
                  new XAttribute("Type", p.ImageType),
                  new XAttribute("Url", uri),
                  new XAttribute("IsThumbnailImage", p.IsThumbNailImage),
                  thumbnails != null ?
                  new XElement("Thumbs", (from t in thumbnails
                                          select new XElement("Thumb",
                                            new XAttribute("ThumbGeneratorID", t.ThumbnailGenerator.ThumbnailGeneratorID),
                                            new XAttribute("Description", t.ThumbnailGenerator.Description),
                                            new XAttribute("Width", t.ThumbnailGenerator.Width),
                                            new XAttribute("Height", t.ThumbnailGenerator.Height),
                                            new XAttribute("Url", new Uri(thumbUrl, t.Path))
                                          ))) : null
                  )));

          return doc.ToString();
        }
        catch (Exception ex)
        {
          XElement doc = new XElement("ProductImages",
             new XElement("Error", ex.StackTrace));

          return doc.ToString();
        }
      }
    }

    [WebMethod(Description = "Get Images for connector by unique key directly from vendor")]
    public string GetAssortmentImages(int connectorID)
    {
      using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
      {
        XmlDocument xml = new XmlDocument();
        try
        {
          Connector con = unit.Scope.Repository<Connector>().GetSingle(c => c.ConnectorID == connectorID);
          con.ThrowIfNull("Connector can't be null");

          List<ImageClass> images = new List<ImageClass>();

          var assortment = unit.Scope.Repository<ImageView>().GetAll(x => x.ConnectorID == connectorID).ToList();
          var brandMedia = unit.Scope.Repository<BrandMediaView>().GetAll(x => x.ConnectorID == connectorID).ToList();

          var imageUrl = new Uri(ConfigurationManager.AppSettings["ConcentratorImageUrl"].ToString());
          var thumbUrl = new Uri(ConfigurationManager.AppSettings["ConcentratorThumbImageUrl"].ToString());

          var thumbs = (from p in unit.Scope.Repository<ProductMediaTumbnail>().GetAll()
                        select new
                        {
                          p.ProductMedia.MediaID,
                          p.ThumbnailGenerator,
                          p.Path
                        }).ToList();

          List<ImageClass> imageList = new List<ImageClass>();

          XElement doc = new XElement("ProductMedia",
             new XElement("Brands",
             from p in brandMedia
             let uri = new Uri(imageUrl, p.ImagePath)
             select new XElement("BrandImage",
                 new XAttribute("BrandID", p.BrandID),
                 new XAttribute("BrandName", p.name),
                 uri)),
              new XElement("Products",
              from p in assortment.OrderBy(c => c.mediaid)
              let uri = new Uri(imageUrl, p.ImagePath)
              let thumbnails = thumbs.Where(x => x.MediaID == p.mediaid)
              select new XElement("ProductMedia",
                  new XAttribute("MediaID", p.mediaid),
                  new XAttribute("ProductID", p.ProductID),
                  new XAttribute("Description", p.Description ?? string.Empty),
                  new XAttribute("BrandID", p.BrandID),
                  new XAttribute("ManufacturerID", p.ManufacturerID),
                  new XAttribute("Sequence", p.Sequence),
                  new XAttribute("Type", p.ImageType),
                  new XAttribute("Url", uri),
                  new XAttribute("IsThumbnailImage", p.IsThumbNailImage),
                  thumbnails != null ?
                  new XElement("Thumbs", (from t in thumbnails
                                          select new XElement("Thumb",
                                            new XAttribute("ThumbGeneratorID", t.ThumbnailGenerator.ThumbnailGeneratorID),
                                            new XAttribute("Description", t.ThumbnailGenerator.Description),
                                            new XAttribute("Width", t.ThumbnailGenerator.Width),
                                            new XAttribute("Height", t.ThumbnailGenerator.Height),
                                            new XAttribute("Url", new Uri(thumbUrl, t.Path))
                                          ))) : null
                  )));

          return doc.ToString();
        }
        catch (Exception ex)
        {
          XElement doc = new XElement("ProductImages",
             new XElement("Error", ex.ToString(), new XElement("StackTrace", ex.StackTrace)));

          return doc.ToString();
        }
      }
    }

    [WebMethod(Description = "Get Images for connector by unique key by FTP")]
    public string GetFTPAssortmentImages(int connectorID)
    {
      return GetFTPAssortmentImage(connectorID, 0);
    }

    [WebMethod(Description = "Get Single Image for connector by unique key by FTP")]
    public string GetFTPAssortmentImage(int connectorID, int concentratorProductID)
    {
      using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
      {

        XmlDocument xml = new XmlDocument();
        try
        {
          Connector con = unit.Scope.Repository<Connector>().GetSingle(c => c.ConnectorID == connectorID);

          con.ThrowIfNull("Connector can't be null");

          List<ImageClass> images = new List<ImageClass>();

          var assortment = unit.Scope.Repository<ImageView>().GetAll(x => x.ConnectorID == connectorID).ToList();
          var brandMedia = unit.Scope.Repository<BrandMediaView>().GetAll(x => x.ConnectorID == connectorID).ToList();

          if (concentratorProductID > 0)
            assortment = assortment.Where(x => x.ProductID == concentratorProductID).ToList();

          List<ImageClass> imageList = new List<ImageClass>();

          XElement doc = new XElement("ProductMedia",
             new XElement("Brands",
             from p in brandMedia
             select new XElement("BrandImage",
                 new XAttribute("BrandID", p.BrandID),
                 new XAttribute("BrandName", p.name),
                 p.ImagePath)),
              new XElement("Products",
              from p in assortment
              select new XElement("ProductMedia",
                  new XAttribute("ProductID", p.ProductID),
                  new XAttribute("BrandID", p.BrandID),
                  new XAttribute("ManufacturerID", p.ManufacturerID),
                  new XAttribute("Sequence", p.Sequence),
                  new XAttribute("Type", p.ImageType),
                  p.ImagePath)));

          return doc.ToString();
        }
        catch (Exception ex)
        {
          XElement doc = new XElement("ProductImages",
             new XElement("Error", ex.StackTrace));

          return doc.ToString();
        }
      }
    }

    [WebMethod(Description = "Get O Assortment for connector")]
    public string GetOAssortment(int connectorID, bool costPrice, string customItemNumber)
    {
      XElement doc = null;

      try
      {
        using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {

          XmlDocument xml = new XmlDocument();
          ContentLogic logic = new ContentLogic(unit.Scope, connectorID);

          logic.FillCustomItemNumbers();


          Connector con = unit.Scope.Repository<Connector>().GetSingle(c => c.ConnectorID == connectorID);

          con.ThrowIfNull("Connector can't be null");


          int preferredVendorID = con.PreferredConnectorVendors.FirstOrDefault(c => c.isPreferred).VendorID;

          JdeAssortmentSoapClient soap = new JdeAssortmentSoapClient();

          DataSet ds = new DataSet();
          DataSet cds = new DataSet();

          int assortmentImportID = con.ConnectorSettings.GetValueByKey<int>("AssortmentImportID", 0);
          int costpriceImportID = con.ConnectorSettings.GetValueByKey<int>("CostPriceImportID", 0);
          bool _shopAssortment = con.ConnectorSettings.GetValueByKey<bool>("ShopAssortment", false);


          if (!string.IsNullOrEmpty(customItemNumber))
          {
            ds = soap.GetSingleShopProductInformation(assortmentImportID, customItemNumber, _shopAssortment);
            if (costpriceImportID > 0)
              cds = soap.GetSingleShopProductInformation(costpriceImportID, customItemNumber, _shopAssortment);

            if (cds == null || cds.Tables.Count < 1 || cds.Tables[0].Rows.Count < 1)
              cds = ds;
          }
          else
            ds = soap.GenerateFullProductListWithNonStock(assortmentImportID, 0, 1, 0, false, true);


          if (costPrice && string.IsNullOrEmpty(customItemNumber))
          {
            cds = soap.GenerateFullProductListWithNonStock(costpriceImportID, 0, 1, 0, false, false);
          }

          if (ds != null)
          {
            var assortment = (from a in unit.Scope.Repository<Content>().GetAll(a => a.ConnectorID == connectorID).ToList()
                              select new
                              {
                                ProductID = a.ProductID,
                                ConnectorProductID = logic.CustomItemNumberList(a.ProductID, con),
                              }).ToList();

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
              if (!string.IsNullOrEmpty(customItemNumber) && dr.Field<string>("LongItemNumber").Trim() != customItemNumber)
              {
                dr.Delete();
                continue;
              }

              var item = (from a in assortment
                          where a.ConnectorProductID == dr.Field<string>("LongItemNumber").Trim()
                          select a).FirstOrDefault();

              if (item != null)
                dr.Delete();
            }
            ds.AcceptChanges();



            var crossLedgerClasses = unit.Scope.Repository<CrossLedgerclass>().GetAll(x => x.ConnectorID == connectorID).ToList();

            int? parentVendor = (from v in unit.Scope.Repository<Vendor>().GetAllAsQueryable()
                                 where v.VendorID == preferredVendorID
                                 select v.ParentVendorID).FirstOrDefault();

            var brandVendors = unit.Scope.Repository<BrandVendor>().GetAll(bv => bv.VendorID == preferredVendorID || (parentVendor.HasValue && parentVendor.Value == parentVendor)).ToList();

            if (costPrice)
            {
              doc = new XElement("Assortment",
                                                    from a in ds.Tables[0].AsEnumerable()
                                                    join b in cds.Tables[0].AsEnumerable() on a.Field<double>("ShortItemNumber") equals b.Field<double>("ShortItemNumber")
                                                    //where prgroup != null
                                                    select new XElement("Product",
                                                      new XAttribute("ManufacturerID", a.Field<string>("VendorItemNumber").Trim()),
                                                      new XAttribute("CustomProductID", a.Field<double>("ShortItemNumber")),
                                                      new XAttribute("CommercialStatus", a.Try(c => c.Field<string>("CommercialStatus"), string.Empty)),
                                                      new XAttribute("LineType", a.Field<string>("LineType")),
                                                      new XAttribute("BrandCode", a.Field<string>("Brand").Trim()),
                                                      new XElement("Price",
                                                       new XAttribute("TaxRate", a.Field<double>("TaxRate")),
                                                       new XElement("UnitPrice", a.Field<decimal>("UnitPrice")),

                                                       new XElement("CostPrice", b.Field<decimal>("UnitPrice"))),
                                                   new XElement("ShopInformation",
                                                     new XElement("LedgerClass", crossLedgerClasses.Where(x => x.LedgerclassCode == a.Field<string>("LedgerClass")).Select(x => x.CrossLedgerclassCode).FirstOrDefault() != null ? crossLedgerClasses.Where(x => x.LedgerclassCode == a.Field<string>("LedgerClass")).Select(x => x.CrossLedgerclassCode).FirstOrDefault() : a.Field<string>("LedgerClass")),
                                                     new XElement("ProductDesk", a.Field<string>("ProductDesk")),
                                                     new XElement("ExtendedCatalog", a.Field<bool?>("Extendedcatalog"))),
                                                     new XElement("Content",
                                                       new XAttribute("ShortDescription", a.Field<string>("Description1")),
                                                       new XAttribute("LongDescription", a.Field<string>("Description2"))
                                                       ),
                                                       new XElement("Barcodes",
                                                         new XElement("Barcode", a.Field<string>("Barcode")))
                                                    ));
            }
            else
            {
              doc = new XElement("Assortment",
                                                   from a in ds.Tables[0].AsEnumerable()
                                                   //where prgroup != null
                                                   select new XElement("Product",
                                                     new XAttribute("ManufacturerID", a.Field<string>("VendorItemNumber").Trim()),
                                                     new XAttribute("CustomProductID", a.Field<double>("ShortItemNumber")),
                                                     new XAttribute("CommercialStatus", a.Try(c => c.Field<string>("CommercialStatus"), string.Empty)),
                                                     new XAttribute("LineType", a.Field<string>("LineType")),
                                                              new XAttribute("BrandCode", a.Field<string>("Brand").Trim()),
                                                                                                         new XElement("Brand",
                                                     new XAttribute("BrandID", brandVendors.Where(x => x.VendorBrandCode == a.Field<string>("Brand").Trim()).FirstOrDefault().BrandID),
                                                     new XElement("Name", brandVendors.Where(x => x.VendorBrandCode == a.Field<string>("Brand").Trim()).FirstOrDefault().Brand.Name)
                                                                                                   ),
                                                     new XElement("Price",
                                                      new XAttribute("TaxRate", a.Field<double>("TaxRate")),
                                                      new XElement("UnitPrice", a.Field<decimal>("UnitPrice"))),
                                                     a.Table.Columns.Contains("LedgerClass") ? new XElement("ShopInformation",
                                                      new XElement("LedgerClass", crossLedgerClasses.Where(x => x.LedgerclassCode == a.Field<string>("LedgerClass")).Select(x => x.CrossLedgerclassCode).FirstOrDefault() != null ? crossLedgerClasses.Where(x => x.LedgerclassCode == a.Field<string>("LedgerClass")).Select(x => x.CrossLedgerclassCode).FirstOrDefault() : a.Field<string>("LedgerClass")),
                                                     new XElement("ProductDesk", a.Field<string>("ProductDesk")),
                                                     new XElement("ExtendedCatalog", a.Field<bool?>("Extendedcatalog"))) : null,
                                                    new XElement("Content",
                                                      new XAttribute("ShortDescription", a.Field<string>("Description1")),
                                                      new XAttribute("LongDescription", a.Field<string>("Description2"))
                                                      ),
                                                      new XElement("Barcodes",
                                                        new XElement("Barcode", a.Field<string>("Barcode")))
                                                        ));
            }
            return doc.ToString();
          }

        }
      }
      catch (Exception ex)
      {
        doc = new XElement("GetOAssortment",
            new XElement("Error", ex.StackTrace));


      }
      return doc.ToString();
    }

    [WebMethod(Description = "Get ProductInformation for connector")]
    public string GetProductInformation(int connectorID, string customItemNumber)
    {
      XmlDocument xml = new XmlDocument();
      XElement doc = null;
      try
      {
        using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {

          Connector con = unit.Scope.Repository<Connector>().GetSingle(x => x.ConnectorID == connectorID);

          con.ThrowIfNull("Connector can't be null");

          int preferredVendorID = con.PreferredConnectorVendors.FirstOrDefault(c => c.isPreferred).VendorID;

          int? parentVendorID = (from v in unit.Scope.Repository<Vendor>().GetAllAsQueryable()
                                 where v.VendorID == preferredVendorID
                                 select v.ParentVendorID).FirstOrDefault();

          JdeAssortmentSoapClient soap = new JdeAssortmentSoapClient();
          DataSet ds = new DataSet();
          int assortmentImportID = con.ConnectorSettings.GetValueByKey<int>("AssortmentImportID", 0);
          ds = soap.GetSingeProductInformation(assortmentImportID, customItemNumber);

          if (ds != null && ds.Tables[0].Rows.Count > 0)
          {
            var brandVendorCode = ds.Tables[0].Rows[0].Field<string>("Brand");
            var brand = unit.Scope.Repository<BrandVendor>().GetSingle(b => b.VendorBrandCode == ds.Tables[0].Rows[0].Field<string>("Brand") && b.BrandID > 0);

            Dictionary<int, ImportAttributeGroup> groupList = new Dictionary<int, ImportAttributeGroup>();

            Attributes att = new Attributes();


            //int languageID = con.ConnectorSettings.GetValueByKey<int>("LanguageID", 2);
            var language = con.ConnectorLanguages.FirstOrDefault();
            language.ThrowArgNull("No Language specified for connector");
            var languageID = language.LanguageID;

            int prodID = 0;
            if (brand != null)
            {
              prodID = (from p in unit.Scope.Repository<Product>().GetAllAsQueryable()
                        where p.VendorItemNumber == ds.Tables[0].Rows[0].Field<string>("VendorItemNumber").Trim()
                        && p.BrandID == brand.BrandID
                        select p.ProductID).FirstOrDefault();

              //var attribute = ((IFunctionScope)unit.Scope).Repository().GetProductAttributes(prodID, languageID, con.ConnectorID, null);

              att.AttributeList = (from a in unit.Scope.Repository<ContentAttribute>().GetAll()
                                   where a.ProductID == prodID
                                    && a.LanguageID == languageID
                                    && (a.ConnectorID == null || a.ConnectorID == con.ConnectorID)
                                   select a).ToList();

              att.AttributeList.ForEach(a =>
              {
                if (!groupList.ContainsKey(a.GroupID))
                {
                  ImportAttributeGroup group = new ImportAttributeGroup();
                  group.AttributeGroupID = a.GroupID;
                  group.AttributeGroupIndex = a.GroupIndex;
                  group.AttributeGroupName = a.GroupName;
                  groupList.Add(a.GroupID, group);
                }
              });
            }

            doc = new XElement("Assortment",
                                                from a in ds.Tables[0].AsEnumerable()
                                                select new XElement("Product",
                                                  new XAttribute("ManufacturerID", a.Field<string>("VendorItemNumber").Trim()),
                                                  new XAttribute("CustomProductID", customItemNumber),
                                                  new XAttribute("CommercialStatus", a.Try(c => c.Field<string>("CommercialStatus"), string.Empty)),
                                                  new XAttribute("ProductID", prodID),
                                                  new XElement("Brand",
                                                     new XAttribute("BrandID", brand != null ? brand.BrandID : 0),
                                                     new XElement("Name", brand != null ? brand.Brand.Name : a.Field<string>("Brand").Trim())
                                                     ),
new XElement("Price",
                                                   new XAttribute("TaxRate", a.Field<double>("TaxRate")),
                                                   new XElement("UnitPrice", a.Field<decimal>("UnitPrice"))),
                                                new XElement("Content",
                                                   new XAttribute("ShortDescription", a.Field<string>("Description1").Trim()),
                                                   new XAttribute("LongDescription", a.Field<string>("Description2").Trim()),
                                                 new XElement("Barcodes",
                                                   new XElement("Barcode", a.Field<string>("Barcode"))),
                                                    new XElement("AttributeGroups",
                      (from ag in groupList.Values
                       select new XElement("AttributeGroup",
                         new XAttribute("AttributeGroupID", ag.AttributeGroupID),
                         new XAttribute("AttributeGroupIndex", ag.AttributeGroupIndex),
                         new XElement("Name", ag.AttributeGroupName))).Distinct()),
                    new XElement("Attributes",
                      att.AttributeList != null ?
                      (from p in att.AttributeList
                       where !string.IsNullOrEmpty(p.AttributeValue)
                       select new XElement("Attribute",
                         new XAttribute("AttributeID", p.AttributeID),
                         new XAttribute("KeyFeature", p.IsVisible),
                         new XAttribute("Index", p.OrderIndex),
                         new XAttribute("IsSearchable", p.IsSearchable),
                         new XAttribute("AttributeGroupID", p.GroupID),
                         new XElement("Name", p.AttributeName),
                         new XElement("Value", p.AttributeValue),
                         new XElement("Sign", p.Sign)
                         )).Distinct() : null)
                                                   )));


            xml.LoadXml(doc.ToString());
          }
        }

      }
      catch (Exception ex)
      {
        throw new Exception("ERROR", ex);
      }

      return xml.InnerXml;
    }

    [WebMethod(Description = "Get ProductInformation")]
    public string GetProductsInformation(List<ProductInformation> products, int vendorID, int connectorID, int languageid)
    {
      int realVendor = vendorID;

      XmlDocument xml = new XmlDocument();
      try
      {
        using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {

          int? parentVendorID = (from v in unit.Scope.Repository<Vendor>().GetAllAsQueryable()
                                 where v.VendorID == vendorID
                                 select v.ParentVendorID).FirstOrDefault();

          if (parentVendorID.HasValue)
            vendorID = parentVendorID.Value;

          var vendorBrands = (from b in unit.Scope.Repository<BrandVendor>().GetAllAsQueryable()
                              where b.VendorID == vendorID
                              select new
                              {
                                BrandCode = b.VendorBrandCode,
                                VendorBrandName = b.Name,
                                Name = b.Brand.Name
                              }).ToList();


          var conn = unit.Scope.Repository<Connector>().GetSingle(c => c.ConnectorID == connectorID);

          var defaultLanguage = conn.ConnectorLanguages.FirstOrDefault();
          defaultLanguage.ThrowArgNull("No default Language specified for connector");

          var preferredVendorID =
            conn.PreferredConnectorVendors.FirstOrDefault(c => c.isPreferred).VendorID;

          var mappingslist = unit.Scope.Repository<ProductGroupMapping>().GetAll(x => x.ConnectorID == connectorID).ToList();

          var contentProducts = (from cp in unit.Scope.Repository<Content>().GetAllAsQueryable()
                                 join cpp in unit.Scope.Repository<ContentProductGroup>().GetAllAsQueryable() on new { cp.ConnectorID, cp.ProductID } equals new { cpp.ConnectorID, cpp.ProductID }
                                 join va in unit.Scope.Repository<VendorAssortment>().GetAllAsQueryable() on new { cpp.ProductID, cp.ContentProduct.VendorID } equals new { va.ProductID, va.VendorID }
                                 where cp.ConnectorID == connectorID
                                 select new
                                 {
                                   customerproduct = va.CustomItemNumber,
                                   contentproductgroups = cpp
                                 }).ToList();

          foreach (ProductInformation inf in products)
          {
            var contentProductGroups = contentProducts.Where(x => x.customerproduct == inf.CustomProductID.ToString()).Select(x => x.contentproductgroups);

            if (contentProductGroups != null)
            {
              var hierarchy = GetProductGroupHierarchy(contentProductGroups, languageid, unit.Scope.Repository<ProductGroupLanguage>(), mappingslist, false, defaultLanguage.LanguageID);

              var brand = vendorBrands.Where(x => x.BrandCode.Trim() == inf.BrandCode.Trim()).FirstOrDefault();

              if (brand == null)
              {
                BrandVendor bv = new BrandVendor()
                {
                  BrandID = -1,
                  VendorBrandCode = inf.BrandCode.Trim(),
                  VendorID = vendorID
                };

                unit.Scope.Repository<BrandVendor>().Add(bv);
                vendorBrands.Add(new
                {
                  BrandCode = inf.BrandCode.Trim(),
                  VendorBrandName = string.Empty,
                  Name = bv.Name
                });
                unit.Save();
              }
              else
                inf.Brandname = brand.Name;

              var brandCode = inf.BrandCode.Trim();
              var productSubGroupCode = inf.ProductGroupCode.Trim();
              var productGroupCode = inf.ParentProductGroupCode.Trim();

              if (hierarchy.Elements("ProductGroup").Count() > 0)
              {
                inf.ProductGroupName = hierarchy.Elements("ProductGroup").FirstOrDefault().Attribute("Name").Value;

                if (hierarchy.Elements("ProductGroup").FirstOrDefault().Element("ProductGroup") != null)
                  inf.ParentProductGroupName = hierarchy.Elements("ProductGroup").FirstOrDefault().Element("ProductGroup").Attribute("Name").Value;
              }
            }
          }

          XElement doc = new XElement("ProductInformation",
            from p in products
            select new XElement("Product",
              new XAttribute("ProductID", p.CustomProductID),
              new XAttribute("ProductGroup", string.IsNullOrEmpty(p.ParentProductGroupName) ? string.Empty : p.ParentProductGroupName),
              new XAttribute("ProudctSubGroup", string.IsNullOrEmpty(p.ProductGroupName) ? string.Empty : p.ProductGroupName),
              new XAttribute("BrandName", string.IsNullOrEmpty(p.Brandname) ? string.Empty : p.Brandname)));

          xml.LoadXml(doc.ToString());

        }
        return xml.OuterXml;
      }
      catch (Exception ex)
      {
        throw new Exception("ERROR", ex);
      }
    }

    [WebMethod(Description = "Get Advanced pricing assortment", BufferResponse = false)]
    public string GetFullInformation(int connectorID, bool importFullContent, bool shopInformation, string concentratorProductID, string customerID, bool showProductGroups, int languageID)
    {
      XmlDocument xml = new XmlDocument();
      try
      {
        using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {

          DataSet customerAss = null;

          //TODO: Load options
          //DataLoadOptions options = new DataLoadOptions();
          //options.LoadWith<ContentProductGroup>(x => x.ProductGroupMapping);
          //// options.LoadWith<ProductGroup>(x => x.ProductGroupMappings);
          //options.LoadWith<ProductGroup>(x => x.ProductGroupLanguage);
          //options.LoadWith<ProductGroupMapping>(x => x.ProductGroup);
          //options.LoadWith<Content>(x => x.Product);
          //options.LoadWith<Product>(x => x.Brand);
          //options.LoadWith<Brand>(x => x.BrandContentVendor);
          //options.LoadWith<RelatedProduct>(x => x.RelatedProductType);
          //context.LoadOptions = options;

          ProductStatusConnectorMapper mapper = new ProductStatusConnectorMapper(connectorID, unit.Scope.Repository<ConnectorProductStatus>());
          ContentLogic logic = new ContentLogic(unit.Scope, connectorID);
          logic.FillRetailStock();
          //logic.FillPriceInformation();

          Connector con = unit.Scope.Repository<Connector>().GetSingle(x => x.ConnectorID == connectorID);

          var preferredVendor = con.PreferredConnectorVendors.FirstOrDefault(c => c.isPreferred);

          if (!string.IsNullOrEmpty(customerID))
          {
            JdeAssortmentSoapClient soap = new JdeAssortmentSoapClient();
            customerAss = soap.GenerateFullProductList(int.Parse(customerID), 0, 0, 0, false, false);
          }

          int preferredVendorID = preferredVendor.VendorID;

          int? parentVendorID = unit.Scope.Repository<Vendor>().GetSingle(v => v.VendorID == preferredVendorID).Try(c => c.ParentVendorID, null);

          var defaultLanguage = con.ConnectorLanguages.FirstOrDefault();
          defaultLanguage.ThrowArgNull("No default Language specified for connector");

          if (con != null)
          {
            if (languageID == 0)
            {
              //languageID = EntityExtensions.GetValueByKey(con.ConnectorSettings, "LanguageID", 2);
              var language = con.ConnectorLanguages.FirstOrDefault();
              language.ThrowArgNull("No Language specified for connector");
              languageID = language.LanguageID;
            }


            var records = unit.Scope.Repository<AssortmentContentView>().GetAllAsQueryable(c => c.ConnectorID == connectorID && c.BrandID > 0);


            if (!String.IsNullOrEmpty(concentratorProductID))
            {
              int id = 0;
              Int32.TryParse(concentratorProductID, out id);
              if (id > 0)
                records = records.Where(x => x.ProductID == id);
            }

            //var contents = records.ToList();
            var productGroups = unit.Scope.Repository<ContentProductGroup>().Include(x => x.ProductGroupMapping.ProductGroup).GetAllAsQueryable(x => x.ConnectorID == connectorID
              || (x.Connector.ParentConnectorID.HasValue && x.Connector.ParentConnectorID.Value == connectorID)).ToList();

            var barcodes = unit.Scope.Repository<ProductBarcodeView>().GetAll(b => b.ConnectorID == connectorID).ToList();
            var _relatedProductRepo = unit.Scope.Repository<RelatedProduct>();
            var recordList = records.ToList();
            List<ImageView> productImages = new List<ImageView>();
            List<RelatedProduct> relatedProductList = new List<RelatedProduct>();
            if (importFullContent)
            {
              productImages = unit.Scope.Repository<ImageView>().GetAll(pi => pi.ConnectorID == connectorID
                               && pi.ImageType == "Product").ToList();


              relatedProductList = (from rp in _relatedProductRepo.GetAllAsQueryable().ToList()
                                    join r in recordList on rp.ProductID equals r.ProductID
                                    select rp).ToList();

              relatedProductList.Union((from rp in unit.Scope.Repository<RelatedProduct>().GetAll().ToList()
                                        join r in recordList on rp.RelatedProductTypeID equals r.ProductID
                                        select rp).ToList());
            }

            var assortment = (from a in recordList
                              let productBarcodes = barcodes.Where(x => x.ProductID == a.ProductID)
                              let productImage = productImages.Where(x => x.ProductID == a.ProductID)
                              let relatedProducts = relatedProductList.Where(x => x.ProductID == a.ProductID | x.RelatedProductID == a.ProductID)
                              let advancedPrice = (customerAss != null ? (customerAss.Tables[0].AsEnumerable().Where(x => x.Field<string>("VendorItemNumber").Trim().ToUpper() == a.VendorItemNumber).Select(x => new AdvancedPricing
                              {
                                MinimumQuantity = (x.Field<int>("MinimumQuantity") > 0 ? x.Field<int>("MinimumQuantity") : 0),
                                Price = x.Field<decimal>("UnitPrice"),
                                TaxRate = (decimal)(x.Field<double>("TaxRate")),
                                ProductID = a.ProductID
                              })).ToList() : null)
                              //barcodes.ContainsKey(a.ProductID) ? barcodes[a.ProductID] : new List<string>()
                              select new
                              {
                                a.ProductID,
                                ManufacturerID = a.VendorItemNumber,
                                Brand = a.BrandName,
                                BrandID = a.BrandID,
                                BrandVendorCode = a.VendorBrandCode,
                                a.ShortDescription,
                                a.LongDescription,
                                /*ShortContentDescription = content.ContainsKey(a.ProductID) ? content[a.ProductID].ShortContentDescription : null,
                                LongContentDescription = content.ContainsKey(a.ProductID) ? content[a.ProductID].LongContentDescription : null,*/
                                //a.ShortContentDescription,
                                //a.LongContentDescription,
                                //Prices = logic.CalculatePrice(a.ProductID, con, a.ProductContentID, productGroups.Where(x => x.ProductID == a.ProductID).ToList(), a.BrandID, advancedPrice).Distinct(),
                                Prices = logic.CalculatePrice(a.ProductID).Distinct(),
                                ConnectorProductID = a.CustomItemNumber,
                                QuantityAvailible = a.QuantityOnHand,
                                StockStatus = a.ConnectorStatus,
                                a.PromisedDeliveryDate,
                                a.QuantityToReceive,
                                RetailStock = logic.RetailStock(a.ProductID, con),
                                Barcodes = productBarcodes,
                                Images = productImage,
                                a.LineType,
                                a.LedgerClass,
                                a.ProductDesk,
                                a.ExtendedCatalog,
                                a.VendorID,
                                a.DeliveryHours,
                                a.CutOffTime,
                                ProductName = !string.IsNullOrEmpty(a.ProductName.ToString()) ? a.ProductName.ToString() : a.ShortDescription,
                                relatedProducts
                              }).Distinct().ToList();

            List<CrossLedgerclass> crossLedgerClasses = null;
            if (shopInformation)
            {
              crossLedgerClasses = unit.Scope.Repository<CrossLedgerclass>().GetAll(x => x.ConnectorID == connectorID).ToList();
            }

            var vendorstocktypes = unit.Scope.Repository<VendorStockType>().GetAll().ToList();

            List<ProductGroupMapping> mappingslist = null;
            if (showProductGroups)
              mappingslist = unit.Scope.Repository<ProductGroupMapping>().GetAll(x => x.ConnectorID == connectorID).ToList();


            using (var stringWriter = new StringWriterWithEncoding(Encoding.UTF8))
            {

              using (var writer = new XmlTextWriter(stringWriter) { Formatting = Formatting.None })
              {

                writer.WriteStartDocument(true);

                XElement element = new XElement("Assortment",
                                                from a in assortment
                                                let productProductGroups = productGroups.Where(x => x.ProductID == a.ProductID)
                                                //                                                  where productGroups.Select(x => x.ProductID).ToList().Contains(a.ProductID)
                                                where a.Prices != null
                                                select new XElement("Product",
                                                new XAttribute("ManufacturerID", a.ManufacturerID.Trim()),
                                                new XAttribute("CustomProductID", !string.IsNullOrEmpty(a.ConnectorProductID) ? a.ConnectorProductID : string.Empty),
                                                new XAttribute("ProductID", a.ProductID),
new XAttribute("LineType", !string.IsNullOrEmpty(a.LineType) ? a.LineType : string.Empty),
                                                   new XAttribute("CutOffTime", a.CutOffTime.HasValue ? a.CutOffTime.Value.ToString("HH:mm") : string.Empty),
                                                    new XAttribute("DeliveryHours", a.DeliveryHours.HasValue ? a.DeliveryHours.Value.ToString() : string.Empty),
new XElement("Brand",
                                                   new XAttribute("BrandID", a.BrandID),
                                                   new XElement("Name", a.Brand),
                                                   new XElement("Code", a.BrandVendorCode)),

                                                   showProductGroups ? GetProductGroupHierarchy(productProductGroups, languageID, unit.Scope.Repository<ProductGroupLanguage>(), mappingslist, false, defaultLanguage.LanguageID) : null,

(from pr in a.Prices
 select new XElement("Price",
                                                      new XAttribute("TaxRate", pr.TaxRate.HasValue ? pr.TaxRate.Value : 19),
 new XAttribute("CommercialStatus", pr.Price.HasValue ? mapper.GetForConnector(pr.ConcentratorStatusID) : mapper.GetForConnector(6)),
 new XAttribute("MinimumQuantity", pr.MinimumQuantity.HasValue ? pr.MinimumQuantity.Value : 0),
                                                      new XElement("UnitPrice", pr.Price),
 new XElement("CostPrice", pr.CostPrice.HasValue ? pr.CostPrice.Value : 0))).Distinct(),
                                                 new XElement("Stock",
                                                 new XAttribute("InStock", a.QuantityAvailible),
                                                new XAttribute("PromisedDeliveryDate", (a.PromisedDeliveryDate.HasValue) ? a.PromisedDeliveryDate.Value.ToLocalTime().ToString("dd-MM-yyyy") : string.Empty),
                                                 new XAttribute("QuantityToReceive", a.QuantityToReceive.HasValue ? a.QuantityToReceive : 0),
new XAttribute("StockStatus", string.IsNullOrEmpty(a.StockStatus) ? string.Empty : a.StockStatus)
,
new XElement("Retail",
                                                   (from r in a.RetailStock
                                                    select new XElement("RetailStock",
                                                      new XAttribute("Name", r.VendorStockTypeID == 1 ? r.vendorName : vendorstocktypes.Where(x => x.VendorStockTypeID == r.VendorStockTypeID).Select(x => x.StockType).FirstOrDefault()),
                                                      new XAttribute("InStock", r.QuantityOnHand),
                                                new XAttribute("PromisedDeliveryDate", (r.PromisedDeliveryDate.HasValue) ? r.PromisedDeliveryDate.Value.ToLocalTime().ToShortDateString() : string.Empty),
                                                new XAttribute("QuantityToReceive", r.QuantityToReceive.HasValue ? r.QuantityToReceive : 0),
                                                      new XAttribute("VendorCode", !string.IsNullOrEmpty(r.BackendVendorCode) ? r.BackendVendorCode : string.Empty),
new XAttribute("StockStatus", mapper.GetForConnector(r.ConcentratorStatusID)),
new XAttribute("CostPrice", r.UnitCost.HasValue ? r.UnitCost : 0)
                                                      )).Distinct())
                                                      ),
                                                 new XElement("Content",
                                                 new XAttribute("ShortDescription", a.ShortDescription.Trim()),
                                                 new XAttribute("LongDescription", a.LongDescription != null ? a.LongDescription.Trim() : string.Empty),
                                                 new XAttribute("ProductName", a.ProductName)),
                                                  //new XAttribute("ShortContentDescription", importFullContent == true ? (a.ShortContentDescription != null ? a.ShortContentDescription : string.Empty) : string.Empty),
                                                  //new XAttribute("LongContentDescription", importFullContent == true ? (a.LongContentDescription != null ? a.LongContentDescription.Replace("\\n", "<br/>") : string.Empty) : string.Empty)),                                                 
                                                new XElement("Barcodes",
                                                  from b in a.Barcodes
                                                  select new XElement("Barcode", b.Barcode)),
!shopInformation ? null : new XElement("ShopInformation",
                                                   new XElement("LedgerClass", crossLedgerClasses.Where(x => x.LedgerclassCode == a.LedgerClass).Select(x => x.CrossLedgerclassCode).FirstOrDefault() != null ? crossLedgerClasses.Where(x => x.LedgerclassCode == a.LedgerClass).Select(x => x.CrossLedgerclassCode).FirstOrDefault() : a.LedgerClass),
                                                   new XElement("ProductDesk", a.ProductDesk),
                                                   new XElement("ExtendedCatalog", a.ExtendedCatalog)),
                                                   new XElement("ProductImages",
                                                     from pi in a.Images
                                                     select new XElement("ProductImage",
                     new XAttribute("ProductID", a.ProductID),
                     new XAttribute("BrandID", a.BrandID),
                     new XAttribute("ManufacturerID", a.ManufacturerID),
                     new XAttribute("Sequence", pi.Sequence),
                     pi.ImagePath)),
                     new XElement("RelatedProducts",
                       from rp in a.relatedProducts
                       select new XElement("RelatedProduct",
                   new XAttribute("ProductID", rp.ProductID),
                   new XAttribute("Type", rp.RelatedProductType.Type))
                       )));

                element.WriteTo(writer);
                writer.Flush();
                writer.WriteEndDocument();
                writer.Flush();
              }

              stringWriter.Flush();
              return stringWriter.ToString();
            }

          }
          return string.Empty;
        }
      }
      catch (Exception ex)
      {
        throw new Exception("ERROR", ex);
        return string.Empty;
      }
    }

    [WebMethod(Description = "Get Expert Product Reviews")]
    public string GetExpertProductReviews()
    {
      using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
      {

        var reviews = (from r in unit.Scope.Repository<ProductReview>().GetAllAsQueryable()
                       group r by r.ProductID
                         into g
                         select new
                         {
                           ProductID = g.Key,
                           Reviews = g.ToList()
                         });

        XDocument doc = new XDocument(new XElement("ExpertReviews",
                                      new XElement("Products",
                                      from review in reviews
                                      select new XElement("Product", new XAttribute("ID", review.ProductID),
                                        new XElement("Reviews",
                                        from rv in review.Reviews
                                        select new XElement("Review", new XAttribute("SourceID", rv.SourceID), new XAttribute("isSummary", rv.IsSummary), new XAttribute("ConcentratorID", rv.ReviewID),
                                          new XElement("Author", rv.Author),
                                          new XElement("Date", rv.Date),
                                          new XElement("Rating", rv.Rating),
                                          new XElement("Title", rv.Title),
                                          new XElement("Summary", rv.Summary),
                                          new XElement("Verdict", rv.Verdict),
                                          new XElement("ReviewURL", rv.ReviewURL),
                                          new XElement("RatingImageURL", rv.RatingImageURL)
                                          )))),

                                      new XElement("Sources",
                                        from c in unit.Scope.Repository<ReviewSource>().GetAllAsQueryable()
                                        select new XElement("Source", new XAttribute("ID", c.SourceID),
                                          new XElement("Name", c.Name),
                                          new XElement("LanguageCode", c.LanguageCode),
                                          new XElement("CountryCode", c.CountryCode),
                                          new XElement("SourceURL", c.SourceUrl),
                                          new XElement("SourceLogoURL", c.SourceLogoUrl),
                                          new XElement("SourceRank", c.SourceRank))
                                        )));

        return doc.ToString();
      }
    }

    [WebMethod(Description = "Get Stock assortment", BufferResponse = false)]
    public XmlDocument GetStockAssortment(int connectorID, string concentratorProductID)
    {
      XmlDocument xml = new XmlDocument();
      try
      {
        using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {


          DataSet customerAss = null;

          ProductStatusConnectorMapper mapper = new ProductStatusConnectorMapper(connectorID, unit.Scope.Repository<ConnectorProductStatus>());
          ContentLogic logic = new ContentLogic(unit.Scope, connectorID);
          logic.FillRetailStock();

          Connector con = unit.Scope.Repository<Connector>().GetSingle(x => x.ConnectorID == connectorID);

          var vendorstocktypes = unit.Scope.Repository<VendorStockType>().GetAll().ToList();


          if (con != null)
          {
            var records = unit.Scope.Repository<AssortmentContentView>().GetAllAsQueryable(c => c.ConnectorID == connectorID && c.BrandID > 0);

            if (!String.IsNullOrEmpty(concentratorProductID))
            {
              int id = 0;
              Int32.TryParse(concentratorProductID, out id);
              if (id > 0)
                records = records.Where(x => x.ProductID == id);
            }

            var assortment = (from a in records.ToList()
                              select new
                              {
                                a.ProductID,
                                ManufacturerID = a.VendorItemNumber,
                                Brand = a.BrandName,
                                BrandID = a.BrandID,
                                BrandVendorCode = a.VendorBrandCode,
                                a.ShortDescription,
                                a.LongDescription,
                                ConnectorProductID = a.CustomItemNumber,
                                QuantityAvailible = a.QuantityOnHand,
                                StockStatus = a.ConnectorStatus,
                                a.PromisedDeliveryDate,
                                a.QuantityToReceive,
                                a.LineType,
                                a.LedgerClass,
                                a.ProductDesk,
                                a.ExtendedCatalog,
                                a.VendorID,
                                a.DeliveryHours,
                                a.CutOffTime,
                                RetailStock = logic.RetailStock(a.ProductID, con)
                              }).ToList();


            XElement element = new XElement("Assortment",
                                            from a in assortment
                                            select new XElement("Product",
                                            new XAttribute("ManufacturerID", a.ManufacturerID.Trim()),
                                            new XAttribute("CustomProductID", !string.IsNullOrEmpty(a.ConnectorProductID) ? a.ConnectorProductID : string.Empty),
                                            new XAttribute("ProductID", a.ProductID),
new XAttribute("LineType", !string.IsNullOrEmpty(a.LineType) ? a.LineType : string.Empty),
                                               new XAttribute("CutOffTime", a.CutOffTime.HasValue ? a.CutOffTime.Value.ToString("HH:mm") : string.Empty),
                                                new XAttribute("DeliveryHours", a.DeliveryHours.HasValue ? a.DeliveryHours.Value.ToString() : string.Empty),
new XElement("Brand",
                                               new XAttribute("BrandID", a.BrandID),
                                               new XElement("Name", a.Brand),
                                               new XElement("Code", a.BrandVendorCode)),
                                             new XElement("Stock",
                                             new XAttribute("InStock", a.QuantityAvailible),
                                            new XAttribute("PromisedDeliveryDate", (a.PromisedDeliveryDate.HasValue) ? a.PromisedDeliveryDate.Value.ToLocalTime().ToShortDateString() : string.Empty),
                                             new XAttribute("QuantityToReceive", a.QuantityToReceive.HasValue ? a.QuantityToReceive : 0),
new XAttribute("StockStatus", string.IsNullOrEmpty(a.StockStatus) ? string.Empty : a.StockStatus)
,
new XElement("Retail",
                                               (from r in a.RetailStock
                                                select new XElement("RetailStock",
                                                  new XAttribute("Name", r.VendorStockTypeID == 1 ? r.vendorName : vendorstocktypes.Where(x => x.VendorStockTypeID == r.VendorStockTypeID).Select(x => x.StockType).FirstOrDefault()),
                                                  new XAttribute("InStock", r.QuantityOnHand),
                                            new XAttribute("PromisedDeliveryDate", (r.PromisedDeliveryDate.HasValue) ? r.PromisedDeliveryDate.Value.ToLocalTime().ToShortDateString() : string.Empty),
                                            new XAttribute("QuantityToReceive", r.QuantityToReceive.HasValue ? r.QuantityToReceive : 0),
                                                  new XAttribute("VendorCode", !string.IsNullOrEmpty(r.BackendVendorCode) ? r.BackendVendorCode : string.Empty),
new XAttribute("StockStatus", mapper.GetForConnector(r.ConcentratorStatusID)),
new XAttribute("CostPrice", r.UnitCost.HasValue ? r.UnitCost : 0)
                                                  )).Distinct()))));


            using (var xmlReader = element.CreateReader())
            {
              xml.Load(xmlReader);


              //xml.LoadXml(doc.ToString());

              return xml;
            }
          }
          return new XmlDocument();
        }
      }
      catch (Exception ex)
      {
        throw new Exception("ERROR", ex);
        return xml;
      }
    }

    [WebMethod(Description = "Get Price assortment", BufferResponse = false)]
    public string GetPriceAssortment(int connectorID, string concentratorProductID, string customerID)
    {
      XmlDocument xml = new XmlDocument();
      try
      {
        using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {


          DataSet customerAss = null;

          var funcRepo = ((IFunctionScope)unit.Scope).Repository();

          ProductStatusConnectorMapper mapper = new ProductStatusConnectorMapper(connectorID, unit.Scope.Repository<ConnectorProductStatus>());
          ContentLogic logic = new ContentLogic(unit.Scope, connectorID);
          logic.FillPriceInformation(funcRepo.GetCalculatedPriceView(connectorID));

          Connector con = unit.Scope.Repository<Connector>().GetSingle(x => x.ConnectorID == connectorID);

          var preferredVendor = con.PreferredConnectorVendors.FirstOrDefault(c => c.isPreferred);

          if (!string.IsNullOrEmpty(customerID))
          {
            JdeAssortmentSoapClient soap = new JdeAssortmentSoapClient();
            customerAss = soap.GenerateFullProductList(int.Parse(customerID), 0, 0, 0, false, false);
          }

          if (con != null)
          {
            var records = funcRepo.GetAssortmentContentView(connectorID).Where(x => x.BrandID > 0);

            if (!String.IsNullOrEmpty(concentratorProductID))
            {
              int id = 0;
              Int32.TryParse(concentratorProductID, out id);
              if (id > 0)
                records = records.Where(x => x.ProductID == id);
            }

            //var contents = records.ToList();
            //var productGroups = unit.Scope.Repository<ContentProductGroup>().GetAll(x => x.ConnectorID == connectorID).ToList();

            var assortment = (from a in records.ToList()
                              let advancedPrice = (customerAss != null ? (customerAss.Tables[0].AsEnumerable().Where(x => x.Field<string>("VendorItemNumber").Trim().ToUpper() == a.VendorItemNumber).Select(x => new AdvancedPricing
                              {
                                MinimumQuantity = (x.Field<int>("MinimumQuantity") > 0 ? x.Field<int>("MinimumQuantity") : 0),
                                Price = x.Field<decimal>("UnitPrice"),
                                TaxRate = (decimal)(x.Field<double>("TaxRate")),
                                ProductID = a.ProductID
                              })).ToList() : null)
                              //barcodes.ContainsKey(a.ProductID) ? barcodes[a.ProductID] : new List<string>()
                              select new
                              {
                                a.ProductID,
                                ManufacturerID = a.VendorItemNumber,
                                Brand = a.BrandName,
                                BrandID = a.BrandID,
                                BrandVendorCode = a.VendorBrandCode,
                                //Prices = logic.CalculatePrice(a.ProductID, con, a.ProductContentID, productGroups.Where(x => x.ProductID == a.ProductID).ToList(), a.BrandID, advancedPrice).Distinct(),
                                Prices = logic.CalculatePrice(a.ProductID).Distinct(),
                                ConnectorProductID = a.CustomItemNumber
                              }).ToList();


            using (var stringWriter = new StringWriterWithEncoding(Encoding.UTF8))
            {

              using (var writer = new XmlTextWriter(stringWriter) { Formatting = Formatting.None })
              {

                writer.WriteStartDocument(true);

                XElement element = new XElement("Assortment",
                                                from a in assortment
                                                where a.Prices != null
                                                select new XElement("Product",
                                                new XAttribute("ManufacturerID", a.ManufacturerID.Trim()),
                                                new XAttribute("CustomProductID", !string.IsNullOrEmpty(a.ConnectorProductID) ? a.ConnectorProductID : string.Empty),
                                                new XAttribute("ProductID", a.ProductID),
new XElement("Brand",
                                                   new XAttribute("BrandID", a.BrandID),
                                                   new XElement("Name", a.Brand),
                                                   new XElement("Code", a.BrandVendorCode)),
(from pr in a.Prices
 select new XElement("Price",
                                                      new XAttribute("TaxRate", pr.TaxRate.HasValue ? pr.TaxRate.Value : 19),
 new XAttribute("CommercialStatus", pr.Price.HasValue ? mapper.GetForConnector(pr.ConcentratorStatusID) : mapper.GetForConnector(6)),
 new XAttribute("MinimumQuantity", pr.MinimumQuantity.HasValue ? pr.MinimumQuantity.Value : 0),
                                                      new XElement("UnitPrice", pr.Price),
 new XElement("CostPrice", pr.CostPrice.HasValue ? pr.CostPrice.Value : 0))).Distinct()
                                                 ));

                element.WriteTo(writer);
                writer.Flush();
                writer.WriteEndDocument();
                writer.Flush();
              }

              stringWriter.Flush();
              return stringWriter.ToString();
            }
          }
          return string.Empty;
        }
      }
      catch (Exception ex)
      {
        throw new Exception("ERROR", ex);
        return string.Empty;
      }
    }

    [WebMethod(Description = "Get Vendor Price assortment", BufferResponse = false)]
    public string GetVendorPriceAssortment(int vendorID)
    {
      XmlDocument xml = new XmlDocument();
      try
      {
        using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {



          ProductStatusVendorMapper mapper = new ProductStatusVendorMapper(unit.Scope.Repository<VendorProductStatus>(), vendorID);

          var assortment = (from a in unit.Scope.Repository<VendorAssortment>().GetAll(x => x.VendorID == vendorID)
                            join p in unit.Scope.Repository<VendorPrice>().GetAll() on a.VendorAssortmentID equals p.VendorAssortmentID into prices
                            select new
                            {
                              ManufacturerID = a.Product.VendorItemNumber,
                              ConnectorProductID = a.CustomItemNumber,
                              a.ProductID,
                              Brand = a.Product.Brand.Name,
                              a.Product.BrandID,
                              BrandVendorCode = a.Product.Brand.BrandVendors.FirstOrDefault(x => x.VendorID == vendorID).Name,
                              Prices = (from x in prices
                                        select new
                                        {
                                          x.TaxRate,
                                          Price = x.Price,
                                          x.MinimumQuantity,
                                          CostPrice = x.CostPrice,
                                          x.CommercialStatus
                                        })

                            }).ToList();


          using (var stringWriter = new StringWriterWithEncoding(Encoding.UTF8))
          {

            using (var writer = new XmlTextWriter(stringWriter) { Formatting = Formatting.None })
            {

              writer.WriteStartDocument(true);

              XElement element = new XElement("Assortment",
                                              from a in assortment
                                              where a.Prices != null
                                              select new XElement("Product",
                                              new XAttribute("ManufacturerID", a.ManufacturerID.Trim()),
                                              new XAttribute("CustomProductID", !string.IsNullOrEmpty(a.ConnectorProductID) ? a.ConnectorProductID : string.Empty),
                                              new XAttribute("ProductID", a.ProductID),
new XElement("Brand",
                                                 new XAttribute("BrandID", a.BrandID),
                                                 new XElement("Name", a.Brand),
                                                 new XElement("Code", a.BrandVendorCode)),
(from pr in a.Prices
 select new XElement("Price",
                                                      new XAttribute("TaxRate", pr.TaxRate.HasValue ? pr.TaxRate.Value : 19),
 new XAttribute("CommercialStatus", pr.CommercialStatus.IfNullOrEmpty("")),
 new XAttribute("MinimumQuantity", pr.MinimumQuantity),
                                                      new XElement("UnitPrice", pr.Price),
 new XElement("CostPrice", pr.CostPrice.HasValue ? pr.CostPrice.Value : 0))).Distinct()
                                               ));

              element.WriteTo(writer);
              writer.Flush();
              writer.WriteEndDocument();
              writer.Flush();
            }

            stringWriter.Flush();
            return stringWriter.ToString();
          }
        }
        return string.Empty;

      }
      catch (Exception ex)
      {
        throw new Exception("ERROR", ex);
        return string.Empty;
      }
    }

    [WebMethod(Description = "Get ProductInformation for connector")]
    public string GetFullConcentratorProducts()
    {
      return GetFullConcentratorProductsByVendor(null);
    }

    [WebMethod(Description = "Get ProductInformation for connector")]
    public string GetFullConcentratorProductsByVendor(int? vendorID)
    {
      System.Xml.Linq.XDocument xml = new XDocument();
      try
      {
        using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {
          var products = (from p in unit.Scope.Repository<Product>().GetAll()
                          where !p.IsConfigurable && (vendorID == null || p.VendorAssortments.Any(l => l.VendorID == vendorID && l.IsActive))
                          let vendorPrice = p.VendorAssortments.FirstOrDefault().VendorPrices.FirstOrDefault()
                          select new
                                                    {
                                                      p.VendorItemNumber,
                                                      BrandName = p.Brand.Name,
                                                      p.ProductID,
                                                      QuantityToReceive = p.VendorStocks.Sum(v => v.QuantityToReceive) == null ? 0 : p.VendorStocks.Sum(v => v.QuantityToReceive),
                                                      QuantityOnHand = p.VendorStocks.Sum(v => v.QuantityOnHand) == null ? 0 : p.VendorStocks.Sum(v => v.QuantityOnHand),
                                                      p.ProductBarcodes,
                                                      p.VendorAssortments.FirstOrDefault().ShortDescription,
                                                      p.VendorAssortments.FirstOrDefault().LongDescription,
                                                      SeasonCode = p.ParentProduct.ProductAttributeValues.FirstOrDefault(attributeValue => attributeValue.ProductAttributeMetaData.AttributeCode == "Season").Value,
                                                      SellingPrice = vendorPrice.Price,
                                                      SpecialPrice = vendorPrice.SpecialPrice == null ? null : vendorPrice.SpecialPrice
                                                    }).ToList();

          List<XElement> productElements = new List<XElement>();

          foreach (var p in products)
          {
            productElements.Add(new XElement("Product",
                                      new XAttribute("ProductID", p.ProductID),
                                      new XAttribute("VendorItemNumber", p.VendorItemNumber == null ? "" : p.VendorItemNumber),
                                      new XElement("ShortDescription", p.ShortDescription == null ? "" : p.ShortDescription),
                                      new XElement("LongDescription", p.LongDescription == null ? "" : p.LongDescription),
                                      new XElement("BrandName", p.BrandName == null ? "" : p.BrandName),
                                      new XElement("Price", new XAttribute("SellingPrice", p.SellingPrice.HasValue ? p.SellingPrice.Value.ToString(CultureInfo.InvariantCulture) : string.Empty), new XAttribute("SpecialPrice", p.SpecialPrice.HasValue ? p.SpecialPrice.Value.ToString(CultureInfo.InvariantCulture) : string.Empty)),
                                      new XElement("Season", p.SeasonCode),
                                      new XElement("Barcodes",
                                          (from pb in p.ProductBarcodes.Where(c => !string.IsNullOrEmpty(c.Barcode)).ToList()
                                           select new XElement("Barcode",
                                             new XAttribute("type", Enum.Parse(typeof(BarcodeTypes), pb.BarcodeType.Value.ToString()).ToString()),
                                             pb.Barcode)))
                                      ));
          }

          XElement element = new XElement("Products", productElements.ToArray());
          xml.Add(element);
        }

      }
      catch (Exception ex)
      {
        throw new Exception("Error while generating XML of products", ex);
      }

      return xml.ToString();
    }


    /**  TODO:::: If needed
     * JSON Object
     * addtionalData : {
     * Brand : "Some brand",
     * Description : "Some description",
     *  ......
     * }
     */

    [WebMethod(Description = "Insert Unmapped Product", BufferResponse = false)]
    public string InsertUnmappedProduct(int vendorID, string name, string vendorItemNumber)
    {
      if (string.IsNullOrEmpty(vendorItemNumber)) vendorItemNumber = name;

      using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
      {

        JavaScriptSerializer serializer = new JavaScriptSerializer();


        Product newProduct = new Product()
        {
          BrandID = -1,
          VendorItemNumber = name,
          SourceVendorID = vendorID
        };

        unit.Scope.Repository<Product>().Add(newProduct);

        VendorAssortment assortment = new VendorAssortment()
        {
          Product = newProduct,
          VendorID = vendorID,
          CustomItemNumber = vendorItemNumber
        };

        unit.Scope.Repository<VendorAssortment>().Add(assortment);

        try
        {
          unit.Save();

          var productObject = new
          {
            productID = newProduct.ProductID,
            success = true,
            message = "Product has been created"
          };

          return serializer.Serialize(productObject);
        }
        catch (Exception ex)
        {
          var emptyObject = new
          {
            sucess = false,
            message = string.Format("Insertion was not successful: {0}", ex.Message)
          };

          return serializer.Serialize(emptyObject);
        }
      }
    }

    private Dictionary<int, Dictionary<int, string>> ProductGroupLanguageList =
      new Dictionary<int, Dictionary<int, string>>();

    private XElement GetProductGroupHierarchy(IEnumerable<ContentProductGroup> groups, int languageid, IRepository<ProductGroupLanguage> repoLanguage, List<ProductGroupMapping> mappingList, bool magento, int defaultLanguageID)
    {
      XElement element = new XElement("ProductGroupHierarchy");


      foreach (var mapping in groups)
      {
        var map = mappingList.Where(x => x.ProductGroupMappingID == mapping.ProductGroupMappingID).FirstOrDefault();

        if (map == null)
        {
          map = mapping.ProductGroupMapping;
        }

        GetProductGroupHierarchy(map, element, languageid, repoLanguage, mappingList, magento, defaultLanguageID);
      }
      return element;
    }

    private XElement GetProductGroupHierarchy(ProductGroupMapping mapping, XElement element, int languageid, IRepository<ProductGroupLanguage> repoLanguage, List<ProductGroupMapping> mappingList, bool magento, int defaultLanguageID)
    {
      Dictionary<int, string> productGroupLanguageList = null;
      Dictionary<int, string> defaultProductGroupLanguageList = null;

      if (!ProductGroupLanguageList.ContainsKey(languageid))
      {
        ProductGroupLanguageList.Add(languageid,
                                     repoLanguage.GetAllAsQueryable(x => x.LanguageID == languageid).ToDictionary(
                                       x => x.ProductGroupID,
                                       y => y.Name));
      }

      if (!ProductGroupLanguageList.ContainsKey(defaultLanguageID))
      {
        ProductGroupLanguageList.Add(defaultLanguageID, repoLanguage.GetAllAsQueryable(x => x.LanguageID == defaultLanguageID).ToDictionary(
                                    x => x.ProductGroupID,
                                    y => y.Name));
      }

      ProductGroupLanguageList.TryGetValue(languageid, out productGroupLanguageList);
      ProductGroupLanguageList.TryGetValue(defaultLanguageID, out defaultProductGroupLanguageList);
      string attributeSetName = "Unknown";
      string name = "Unknown";
      productGroupLanguageList.TryGetValue(mapping.ProductGroupID, out name);
      defaultProductGroupLanguageList.TryGetValue(mapping.ProductGroupID, out attributeSetName);

      if (string.IsNullOrEmpty(name))
      {
        ProductGroupLanguageList.TryGetValue(defaultLanguageID, out productGroupLanguageList);
        productGroupLanguageList.TryGetValue(mapping.ProductGroupID, out name);
      }

      var imageUrl = ConfigurationManager.AppSettings["ConcentratorImageUrl"];

      var image = string.IsNullOrEmpty(mapping.ProductGroupMappingPath) ? mapping.ProductGroup.ImagePath : mapping.ProductGroupMappingPath;

      if (!string.IsNullOrEmpty(imageUrl) && !string.IsNullOrEmpty(image))
      {
        imageUrl = new Uri(new Uri(imageUrl), "Mapping/" + image).ToString();
      }
      else
        imageUrl = image;

      var imageThumbUrl = ConfigurationManager.AppSettings["ConcentratorImageUrl"];

      var imageThumb = string.IsNullOrEmpty(mapping.MappingThumbnailImagePath) ? string.Empty : mapping.MappingThumbnailImagePath;

      if (!string.IsNullOrEmpty(imageThumbUrl) && !string.IsNullOrEmpty(imageThumb))
      {
        imageThumbUrl = new Uri(new Uri(imageThumbUrl), "Mapping/" + imageThumb).ToString();
      }
      else
        imageThumbUrl = imageThumb;

      var el = new XElement("ProductGroup", new XAttribute("ID", mapping.ProductGroupID),
                              new XAttribute("Name", !string.IsNullOrEmpty(mapping.CustomProductGroupLabel) ? mapping.CustomProductGroupLabel : (name ?? "Unknown")),
                              new XAttribute("Index", mapping.Score.HasValue ? mapping.Score.Value : mapping.ProductGroup.Score),
                              new XElement("Image", imageUrl),
                              new XElement("ThumbnailImage", imageThumbUrl),
                              new XElement("Description", mapping.ProductGroupMappingDescriptions != null ? mapping.ProductGroupMappingDescriptions.FirstOrDefault(c => c.LanguageID == languageid).Try(l => l.Description, string.Empty) : string.Empty),
                              new XElement("CustomName", mapping.ProductGroupMappingLabel),
                              new XElement("AttributeSet", attributeSetName),
                              new XAttribute("MappingID", mapping.ProductGroupMappingID));


      if (magento)
      {
        var magentoSetting = mapping.MagentoProductGroupSettings.FirstOrDefault();

        if (magentoSetting != null && (magentoSetting.ShowInMenu.HasValue || magentoSetting.DisabledMenu.HasValue || magentoSetting.IsAnchor.HasValue))
        {
          el.Add(new XElement("MagentoSetting",
            //new XAttribute("ShowInMenu", magentoSetting.ShowInMenu.HasValue ? magentoSetting.ShowInMenu.Value : false),
            new XAttribute("DisableMenu", magentoSetting.DisabledMenu.HasValue ? magentoSetting.DisabledMenu.Value : false),
            new XAttribute("HideInMenu", magentoSetting.ShowInMenu.HasValue ? magentoSetting.ShowInMenu.Value : false),
            new XAttribute("PageLayoutCode", magentoSetting.ProductGroupMapping.MagentoPageLayout != null ? magentoSetting.ProductGroupMapping.MagentoPageLayout.LayoutCode : ""),
            new XAttribute("IsAnchor", magentoSetting.IsAnchor.HasValue ? magentoSetting.IsAnchor.Value : false)));

        }
      }

      element.Add(el);

      if (mapping.ParentProductGroupMappingID.HasValue)
      {
        GetProductGroupHierarchy(mapping.ParentMapping, el, languageid, repoLanguage, mappingList, magento, defaultLanguageID);
      }

      return element;
    }

    [WebMethod(Description = "Get FreeGoods for connector", BufferResponse = false)]
    public string GetFreeGoods(int connectorID)
    {
      XmlDocument xml = new XmlDocument();
      try
      {
        using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {

          var freeGoods = (from f in unit.Scope.Repository<VendorFreeGood>().GetAllAsQueryable()
                           let freeGood = f.Product.VendorAssortments.Where(x => x.VendorID == f.VendorAssortment.VendorID).OrderBy(x => x.IsActive).FirstOrDefault()
                           where f.VendorAssortment.Vendor.ContentProducts.Any(x => x.ConnectorID == connectorID)
                           select new
                           {
                             Product = new
                             {
                               ManufacturerID = f.VendorAssortment.Product.VendorItemNumber,
                               ConnectorProductID = f.VendorAssortment.CustomItemNumber,
                               ProductID = f.VendorAssortment.ProductID,
                               BrandID = f.VendorAssortment.Product.Brand.BrandID,
                               BrandName = f.VendorAssortment.Product.Brand.Name
                             },
                             FreeGood = new
                             {
                               ManufacturerID = freeGood.Product.VendorItemNumber,
                               ConnectorProductID = freeGood.CustomItemNumber,
                               ProductID = freeGood.ProductID,
                               BrandID = freeGood.Product.Brand.BrandID,
                               BrandName = freeGood.Product.Brand.Name,
                               f.MinimumQuantity,
                               f.OverOrderedQuantity,
                               f.FreeGoodQuantity,
                               f.UnitPrice,
                               f.Description
                             }
                           }).ToList();

          using (var stringWriter = new StringWriterWithEncoding(Encoding.UTF8))
          {

            using (var writer = new XmlTextWriter(stringWriter) { Formatting = Formatting.None })
            {

              writer.WriteStartDocument(true);

              XElement element = new XElement("Assortment",
                                              from a in freeGoods
                                              group a by a.Product into grouped
                                              select new XElement("Product",
                                              new XAttribute("ManufacturerID", grouped.Key.ManufacturerID.Trim()),
                                              new XAttribute("CustomProductID", !string.IsNullOrEmpty(grouped.Key.ConnectorProductID) ? grouped.Key.ConnectorProductID : string.Empty),
                                              new XAttribute("ProductID", grouped.Key.ProductID),
new XElement("Brand",
                                                 new XAttribute("BrandID", grouped.Key.BrandID),
                                                 new XElement("Name", grouped.Key.BrandName)),
                                                 new XElement("FreeGoods",
                                                 from f in freeGoods
                                                 where f.Product.ProductID == grouped.Key.ProductID
                                                 select new XElement("FreeGood",
                                                   new XAttribute("ManufacturerID", f.FreeGood.ManufacturerID.Trim()),
                                              new XAttribute("CustomProductID", !string.IsNullOrEmpty(f.FreeGood.ConnectorProductID) ? f.FreeGood.ConnectorProductID : string.Empty),
                                              new XAttribute("ProductID", f.FreeGood.ProductID),
                                              new XAttribute("MinimumQuantity", f.FreeGood.MinimumQuantity),
                               new XAttribute("OverOrderedQuantity", f.FreeGood.OverOrderedQuantity),
                               new XAttribute("FreeGoodQuantity", f.FreeGood.FreeGoodQuantity),
                               new XAttribute("UnitPrice", f.FreeGood.UnitPrice),
                               new XAttribute("Description", f.FreeGood.Description),
new XElement("Brand",
                                                 new XAttribute("BrandID", f.FreeGood.BrandID),
                                                 new XElement("Name", f.FreeGood.BrandName))))));

              element.WriteTo(writer);
              writer.Flush();
              writer.WriteEndDocument();
              writer.Flush();
            }

            stringWriter.Flush();
            return stringWriter.ToString();
          }

          //xml.LoadXml(doc.ToString());

          return string.Empty;
        }
      }
      catch (Exception ex)
      {
        throw new Exception("ERROR", ex);
        return string.Empty;
      }
    }

    [WebMethod(Description = "Get Accruels for connector", BufferResponse = false)]
    public string GetAccruels(int connectorID)
    {
      XmlDocument xml = new XmlDocument();
      try
      {
        using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {

          var accruels = (from a in unit.Scope.Repository<VendorAccruel>().GetAllAsQueryable()
                          where a.VendorAssortment.Vendor.ContentProducts.Any(x => x.ConnectorID == connectorID)
                          select new
                          {
                            Product = new
                            {
                              ManufacturerID = a.VendorAssortment.Product.VendorItemNumber,
                              ConnectorProductID = a.VendorAssortment.CustomItemNumber,
                              ProductID = a.VendorAssortment.ProductID,
                              BrandID = a.VendorAssortment.Product.Brand.BrandID,
                              BrandName = a.VendorAssortment.Product.Brand.Name
                            },
                            Accruel = a
                          }).ToList();

          using (var stringWriter = new StringWriterWithEncoding(Encoding.UTF8))
          {

            using (var writer = new XmlTextWriter(stringWriter) { Formatting = Formatting.None })
            {

              writer.WriteStartDocument(true);

              XElement element = new XElement("Assortment",
                                              from a in accruels
                                              group a by a.Product into grouped
                                              select new XElement("Product",
                                              new XAttribute("ManufacturerID", grouped.Key.ManufacturerID.Trim()),
                                              new XAttribute("CustomProductID", !string.IsNullOrEmpty(grouped.Key.ConnectorProductID) ? grouped.Key.ConnectorProductID : string.Empty),
                                              new XAttribute("ProductID", grouped.Key.ProductID),
new XElement("Brand",
                                                 new XAttribute("BrandID", grouped.Key.BrandID),
                                                 new XElement("Name", grouped.Key.BrandName)),
                                                 new XElement("Accruels",
                                                 from f in accruels
                                                 where f.Product.ProductID == grouped.Key.ProductID
                                                 select new XElement("Accruel",
                                                   new XAttribute("AccruelCode", f.Accruel.AccruelCode),
                                                   new XAttribute("Description", f.Accruel.Description),
                                                   new XAttribute("UnitPrice", f.Accruel.UnitPrice),
                                                   new XAttribute("MinimumQuantity", f.Accruel.MinimumQuantity)
                                                   ))));

              element.WriteTo(writer);
              writer.Flush();
              writer.WriteEndDocument();
              writer.Flush();
            }

            stringWriter.Flush();
            return stringWriter.ToString();
          }




          //xml.LoadXml(doc.ToString());

          return string.Empty;
        }
      }
      catch (Exception ex)
      {
        throw new Exception("ERROR", ex);
        return string.Empty;
      }
    }

    [WebMethod(Description = "Get assortment hierarchy", BufferResponse = false)]
    public string GetProductGroupHierarchy(int connectorID, int languageID, string concentratorProductID)
    {
      XmlDocument xml = new XmlDocument();
      try
      {
        using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {

          var connector = unit.Scope.Repository<Connector>().GetSingle(x => x.ConnectorID == connectorID);
          var productGroupmapping = unit.Scope.Repository<ProductGroupMapping>().GetAll(x => (x.ConnectorID == connectorID || (connector.ParentConnectorID.HasValue && x.ConnectorID == connector.ParentConnectorID.Value)) && !x.ParentProductGroupMappingID.HasValue).OrderByDescending(x => x.Score).ToList();

          using (var stringWriter = new StringWriterWithEncoding(Encoding.UTF8))
          {

            using (var writer = new XmlTextWriter(stringWriter) { Formatting = Formatting.None })
            {

              writer.WriteStartDocument(true);


              XElement element = new XElement("ProductGroupHierarchy");

              foreach (var mapping in productGroupmapping)
              {
                element = GetProductGroupHierarchy(languageID, unit.Scope.Repository<ProductGroupLanguage>(), mapping, element);
              }
              //XElement element = GetProductGroupHierarchy(languageID, unit.Scope.Repository<ProductGroupLanguage>(), productGroupmapping, false);

              element.WriteTo(writer);
              writer.Flush();
              writer.WriteEndDocument();
              writer.Flush();
            }

            stringWriter.Flush();
            return stringWriter.ToString();
          }

        }
        return string.Empty;
      }
      catch (Exception ex)
      {
        throw new Exception("ERROR", ex);
        return string.Empty;
      }
    }

    private XElement GetProductGroupHierarchy(int languageid, IRepository<ProductGroupLanguage> repoLanguage, ProductGroupMapping mapping, XElement element)
    {
      Dictionary<int, string> productGroupLanguageList = null;

      if (!ProductGroupLanguageList.ContainsKey(languageid))
      {
        ProductGroupLanguageList.Add(languageid,
                                     repoLanguage.GetAllAsQueryable(x => x.LanguageID == languageid).ToDictionary(
                                       x => x.ProductGroupID,
                                       y => y.Name));
      }

      if (!ProductGroupLanguageList.ContainsKey(1))
      {
        if (languageid != 1)
        {
          ProductGroupLanguageList.Add(1, repoLanguage.GetAllAsQueryable(x => x.LanguageID == 1).ToDictionary(
                                      x => x.ProductGroupID,
                                      y => y.Name));
        }
      }

      ProductGroupLanguageList.TryGetValue(languageid, out productGroupLanguageList);
      string name = "Unknown";
      productGroupLanguageList.TryGetValue(mapping.ProductGroupID, out name);

      if (string.IsNullOrEmpty(name))
      {
        ProductGroupLanguageList.TryGetValue(1, out productGroupLanguageList);
        productGroupLanguageList.TryGetValue(mapping.ProductGroupID, out name);
      }

      var imageUrl = ConfigurationManager.AppSettings["ConcentratorImageUrl"];
      var thumbnailImageUrl = ConfigurationManager.AppSettings["ConcentratorImageUrl"];
      var image = string.IsNullOrEmpty(mapping.ProductGroupMappingPath) ? mapping.ProductGroup.ImagePath : string.Empty;
      var thumbnailImage = mapping.MappingThumbnailImagePath ?? string.Empty;


      if (!string.IsNullOrEmpty(imageUrl) && !string.IsNullOrEmpty(image))
      {
        imageUrl = new Uri(new Uri(imageUrl), "Productgroup/" + image).ToString();
      }
      else
        imageUrl = image;

      if (!string.IsNullOrEmpty(imageUrl) && !string.IsNullOrEmpty(image))
      {
        imageUrl = new Uri(new Uri(imageUrl), "Productgroup/" + thumbnailImage).ToString();
      }
      else
        imageUrl = thumbnailImage;


      var el = new XElement("ProductGroup", new XAttribute("ID", mapping.ProductGroupID),
                              new XAttribute("Name", !string.IsNullOrEmpty(mapping.CustomProductGroupLabel) ? mapping.CustomProductGroupLabel : name),
                              new XAttribute("Index", mapping.Score.HasValue ? mapping.Score.Value : mapping.ProductGroup.Score),
                              new XElement("Image", imageUrl),
                              new XElement("ThumbnailImage", thumbnailImageUrl),
                              new XElement("CustomName", mapping.ProductGroupMappingLabel),
                              new XAttribute("MappingID", mapping.ProductGroupMappingID));

      element.Add(el);

      foreach (var childmap in mapping.ChildMappings.OrderByDescending(x => x.Score))
      {
        GetProductGroupHierarchy(languageid, repoLanguage, childmap, el);
      }

      return element;
    }

    [WebMethod(Description = "Get Price Sets")]
    public XmlDocument GetPriceSets(int connectorID)
    {
      XmlDocument result = new XmlDocument();

      try
      {
        using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {

          var priceSets = unit.Scope.Repository<PriceSet>().GetAll(c => c.ConnectorID == connectorID);
          List<XElement> priceSetElements = new List<XElement>();
          foreach (var priceSet in priceSets)
          {
            List<XElement> productPriceSetElements = new List<XElement>();

            foreach (var productPriceSet in priceSet.ProductPriceSets)
            {
              productPriceSetElements.Add(new XElement("Product",
                  new XAttribute("Quantity", productPriceSet.Quantity),
                  new XAttribute("ProductID", productPriceSet.Product.ProductID)
                )
              );
            }

            object priceValue;
            object discountPercentageValue;

            if (priceSet.Price.HasValue)
            {
              priceValue = priceSet.Price.Value;
            }
            else
            {
              priceValue = "";
            }

            if (priceSet.DiscountPercentage.HasValue)
            {
              discountPercentageValue = priceSet.DiscountPercentage.Value;
            }
            else
            {
              discountPercentageValue = "";
            }

            priceSetElements.Add(new XElement("PriceSet",
                new XAttribute("PriceSetID", priceSet.PriceSetID),
                new XAttribute("IsCatalog", priceSet.IsCatalog),
                new XAttribute("Name", priceSet.Name),
                new XAttribute("Description", priceSet.Description),
                new XAttribute("Price", priceValue),
                new XAttribute("DiscountPercentage", discountPercentageValue),
                productPriceSetElements.ToArray()
              )
            );
          }

          XElement PriceSetExportElement = new XElement("PriceSets", priceSetElements.ToArray());

          using (var reader = PriceSetExportElement.CreateReader())
          {
            result.Load(reader);
          }

        }
      }
      catch (Exception ex)
      {
        throw new Exception("Error generating xml of PriceSets", ex);
      }
      return result;
    }

    private string BaseSiteUrl
    {
      get
      {
        HttpContext context = HttpContext.Current;
        string baseUrl = context.Request.Url.Scheme + "://" + context.Request.Url.Authority + context.Request.ApplicationPath.TrimEnd('/') + '/';
        return baseUrl;
      }
    }

  }

  [Serializable]
  public class ImageClass
  {
    public int ProductID { get; set; }
    public string ManufacturerID { get; set; }
    public int BrandID { get; set; }
    public string BrandVendorCode { get; set; }
    public int ProductGroupID { get; set; }
    public ImageType Type { get; set; }
    public string ImageUrl { get; set; }
    public string ImagePath { get; set; }
    public int Sequence { get; set; }
    public string CustomItemNumber { get; set; }
    public ImageClass()
    {
    }
  }

  [Serializable]
  public enum ImageType
  {
    ProductImage,
    ProductGroupImage,
    BrandImage
  }

  [Serializable]
  public class ProductInformation
  {
    public int CustomProductID { get; set; }
    public string BrandCode { get; set; }
    public string ParentProductGroupCode { get; set; }
    public string ProductGroupCode { get; set; }
    public string Brandname { get; set; }
    public string ParentProductGroupName { get; set; }
    public string ProductGroupName { get; set; }
  }

  public class ProductGroupVendorName
  {
    public string ParentProductGroupCode { get; set; }
    public string ParentProductGroupName { get; set; }
    public string ProductGroupCode { get; set; }
    public string ProductGroupName { get; set; }
    public string BrandCode { get; set; }
  }

  public class StringWriterWithEncoding : StringWriter
  {
    Encoding encoding;
    public StringWriterWithEncoding(StringBuilder builder, Encoding encoding)
      : base(builder)
    {
      this.encoding = encoding;
    }

    public StringWriterWithEncoding(Encoding encoding)
      : base()
    {
      this.encoding = encoding;
    }
    public override Encoding Encoding { get { return encoding; } }
  }

}
