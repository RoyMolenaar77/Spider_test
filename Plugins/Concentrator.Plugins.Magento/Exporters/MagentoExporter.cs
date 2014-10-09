using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.Models.Connectors;
using AuditLog4Net.Adapter;
using System.Xml.Linq;
using SBSimpleSftp;
using Concentrator.Plugins.Magento.Models;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using Concentrator.Plugins.Magento.Helpers;
using System.Threading;
using System.Globalization;
using System.Web;
using System.Collections;
using Concentrator.Web.Services;
using System.Xml;
using Concentrator.Objects.AssortmentService;
using ServiceStack.Text;

namespace Concentrator.Plugins.Magento.Exporters
{
  public class MagentoExporter : BaseExporter
  {
    private static string OutputFolder = @"e:\magento\";
    private static string CacheFile = @"e:\magento\assortment-{0}-{1}.xml";
    private static string AttributesCacheFile = @"e:\magento\attributes-{0}-{1}.xml";
    private static string ContentCacheFile = @"e:\magento\content-{0}-{1}.xml";
    private static string AccruelsCacheFile = @"e:\magento\accruels-{0}-{1}.xml";
    private static string PriceSetsCacheFile = @"e:\magento\pricesets-{0}-{1}.xml";

    #region Processing Variables
    Dictionary<string, List<int>> ProductsPerAttributeSet;
    Dictionary<int, XElement> AttributeGroups;
    Dictionary<int, Dictionary<int, AttributeValueGroupInfo>> AttributeValueGroups = new Dictionary<int, Dictionary<int, AttributeValueGroupInfo>>();
    //private XDocument AssortmentXml;
    private XDocument AttributesXml;
    private XDocument ContentXml;
    private List<AssortmentProductInfo> Assortment;

    private XDocument AccruelsXml;
    private XDocument PriceSetsXml;

    private string _nonAssortmentItemsSetName;

    #endregion

    protected bool UseContentDescriptions { get; private set; }
    protected bool UsesConfigurableProducts { get; private set; }
    private Dictionary<int, List<AssortmentProductInfo>> ConfigurableProducts { get; set; }
    private HashSet<int> SimpleProducts { get; set; }
    private string _serializationPath;
    private bool GetOptionScoreFromOtherValueGroupsBasedOnGroupName { get; set; }

    /// <summary>
    /// By default categories with page layouts are always marked as not visible in menu
    /// This disables the behaviour
    /// </summary>
    private bool CategoriesWithPageLayoutShouldNotBeHidden { get; set; }

    private Dictionary<string, List<RelatedProductWithIndex>> RelatedProducts { get; set; }
    private Dictionary<string, List<RelatedProductWithIndex>> CrossSellProducts { get; set; }

    public MagentoExporter(Connector connector, IAuditLogAdapter logger, string serializationPath = "")
      : base(connector, logger)
    {
      _nonAssortmentItemsSetName = Connector.ConnectorSettings.GetValueByKey<string>("NonAssortmentItemSet", string.Empty);
      _serializationPath = serializationPath;

      GetOptionScoreFromOtherValueGroupsBasedOnGroupName = Connector.ConnectorSettings.GetValueByKey<bool>("Magento.GetOptionSortFromOtherValueGroups", false);
      CategoriesWithPageLayoutShouldNotBeHidden = Connector.ConnectorSettings.GetValueByKey<bool>("Magento.CategoriesWithPageLayoutShouldNotBeHidden", false);
    }

    protected override void Process()
    {
      SyncRequiredCatalogAttributes();

      foreach (var language in Languages)
      {
        CurrentLanguage = language;

        if (!StoreList.ContainsKey(CurrentStoreCode))
        {
          Logger.WarnFormat("Language {0} cannot be found in storelist of Magento", CurrentLanguage.Language.DisplayCode);
          continue;
        }

        Logger.DebugFormat("Processing '{0}'", CurrentLanguage.Language.Name);

        Logger.DebugFormat("Retrieving XML Product Information from service");

        #region WebService
        var Soap = new AssortmentService();

        //var assortmentXml = Soap.GetAdvancedPricingAssortment(Connector.ConnectorID, false, true, null, null, true, language.LanguageID, true); //.OuterXml;
        var attributesXml = Soap.GetAttributesAssortmentByLanguage(Connector.ConnectorID, null, null, language.LanguageID); //.OuterXml;
        var contentXml = Soap.GetAssortmentContentDescriptionsByLanguage(Connector.ConnectorID, null, language.LanguageID); //.OuterXml;

        using (XmlNodeReader read = new XmlNodeReader(attributesXml))
        {
          AttributesXml = XDocument.Load(read);
        }

        using (XmlNodeReader read = new XmlNodeReader(contentXml))
        {
          ContentXml = XDocument.Load(read);
        }
        Service service = new Service(Connector.ConnectorID);

#if CACHE
        if (!string.IsNullOrEmpty(_serializationPath))
        {
          if (Directory.Exists(_serializationPath))
          {
            if (File.Exists(Path.Combine(_serializationPath, string.Format("assortment_{0}", Connector.ConnectorID))))
            {
              Assortment = File.ReadAllText(Path.Combine(_serializationPath, string.Format("assortment_{0}", Connector.ConnectorID))).FromJson<List<AssortmentProductInfo>>();
            }
            else
            {
              Assortment = service.GetAssortment(false, true, null, true, language.LanguageID, true);
            }
          }
        }
        Assortment = service.GetAssortment(false, true, null, true, language.LanguageID, true);
#else
        Assortment = service.GetAssortment(false, true, null, true, language.LanguageID, true);
#endif

        try
        {
          if (!string.IsNullOrEmpty(_serializationPath))
          {
            if (Directory.Exists(_serializationPath))
            {
              File.WriteAllText(Path.Combine(_serializationPath, string.Format("assortment_{0}", Connector.ConnectorID)), Assortment.ToJson());
            }
          }
        }
        catch (Exception e)
        {
          Logger.Debug("Serialization and saving failed because of error", e);
        }



        #endregion

        UseContentDescriptions = Connector.ConnectorSettings.GetValueByKey<bool>("UseContentDescriptions", false);

        UsesConfigurableProducts = (from p in Assortment
                                    where p.IsConfigurable
                                    select p
                                      ).Any();


        var ProductList = Assortment.ToDictionary(x => x.ProductID, z => z);

        if (UsesConfigurableProducts)
        {
          //get dictionary for parent-child ids
          /*
           * <RelatedProduct IsConfigured="true" RelatedProductID="156981" Type="Configured product" RelatedProductTypeID="8" >
           */

          var rpc = Assortment.Where(c => c.IsConfigurable).ToList();


          ConfigurableProducts = (from p in Assortment
                                  where p.IsConfigurable
                                  && p.RelatedProducts != null

                                  let children = (from rp in p.RelatedProducts
                                                  let relatedProductId = rp.RelatedProductID
                                                  where rp.IsConfigured
                                                  && ProductList.ContainsKey(relatedProductId)
                                                  select ProductList[relatedProductId]).ToList()

                                  where children.Count() > 0

                                  select new
                                  {
                                    Parent = p,
                                    Children = children
                                  }).ToDictionary(x => x.Parent.ProductID, y => y.Children.ToList());




          SimpleProducts = new HashSet<int>((from p in ConfigurableProducts
                                             select p.Value.Select(x => x.ProductID)
                                             ).SelectMany(x => x).Distinct());
        }

        RelatedProducts = (from p in Assortment
                           where p.RelatedProducts != null
                           let childRelationships = (from rp in p.RelatedProducts
                                                     let relatedProductId = rp.RelatedProductID
                                                     where !rp.IsConfigured


                                                     && (rp.TypeID == 9 || rp.TypeID == 10)

                                                     && ProductList.ContainsKey(relatedProductId)
                                                     select rp)

                           let children = (from rp in childRelationships
                                           select new RelatedProductWithIndex
                                           {
                                             RelatedProductManufacturerID = ProductList[rp.RelatedProductID].ManufacturerID,
                                             Index = rp.Index
                                           }).ToList()
                           where children.Count() > 0
                           select new
                                                      {
                                                        Parent = p,
                                                        Children = children
                                                      }).ToDictionary(c => c.Parent.ManufacturerID, c => c.Children);



        CrossSellProducts = (from p in Assortment
                             where p.RelatedProducts != null
                             let childRelationships = (from rp in p.RelatedProducts
                                                       let relatedProductId = rp.RelatedProductID
                                                       where !rp.IsConfigured


                                                       && (rp.TypeID == 47030 || rp.TypeID == 47031)

                                                       && ProductList.ContainsKey(relatedProductId)
                                                       select rp)

                             let children = (from rp in childRelationships
                                             select new RelatedProductWithIndex
                                             {
                                               RelatedProductManufacturerID = ProductList[rp.RelatedProductID].ManufacturerID,
                                               Index = rp.Index
                                             }).ToList()
                             where children.Count() > 0
                             select new
                             {
                               Parent = p,
                               Children = children
                             }).ToDictionary(c => c.Parent.ManufacturerID, c => c.Children);



        var attributeSets = (from p in Assortment
                             let ph = p.ProductGroups
                             where (ph != null && ph.Count() > 0) || p.IsNonAssortmentItem
                             let d = ph.FirstOrDefault()
                             where (d != null || p.IsNonAssortmentItem)
                             select new
                             {
                               Name = d.Try(c => c.Name, string.Empty),
                               ProductID = p.ProductID
                             }
                                     );


        ProductsPerAttributeSet = new Dictionary<string, List<int>>();
        foreach (var r in attributeSets)
        {
          if (!ProductsPerAttributeSet.ContainsKey(r.Name))
            ProductsPerAttributeSet.Add(r.Name, new List<int>());

          ProductsPerAttributeSet[r.Name].Add(r.ProductID);
        }


        Logger.DebugFormat("Found {0} Attribute Sets", attributeSets.Count());

        AttributeGroups = new Dictionary<int, XElement>();

        AttributeGroups = (from p in AttributesXml.Root.Elements("ProductAttribute")
                           let ag = p.Element("AttributeGroups")
                           where ag != null
                           select ag.Elements("AttributeGroup")).SelectMany(x => x)
                               .GroupBy(g => Convert.ToInt32(g.Attribute("AttributeGroupID").Value))
                               .Select(gr => gr.First())
                               .ToDictionary(x => Convert.ToInt32(x.Attribute("AttributeGroupID").Value), y => y);


        Logger.DebugFormat("Found {0} Attribute Groups", AttributeGroups.Count());

        if (AttributesXml.Root.Element("AttributeValueGroups") != null)
        {
          var source = (from avg in AttributesXml.Root.Element("AttributeValueGroups").Elements("AttributeValueGroup")
                        let attributeId = Convert.ToInt32(avg.Attribute("AttributeID").Value)
                        let name = avg.Attribute("Name").Value
                        let image = avg.Attribute("Image").Value
                        let groupId = Convert.ToInt32(avg.Attribute("AttributeValueGroupID").Value)
                        let score = Convert.ToInt32(avg.Attribute("Score").Value)
                        select new { attributeId, name, image, groupId, score }
                                  );

          AttributeValueGroups = (from avg in source
                                  group avg by avg.attributeId into grouped
                                  select grouped).ToDictionary(x => x.Key,

                                  y => (from a in y
                                        select new AttributeValueGroupInfo() { groupId = a.groupId, score = a.score, image = a.image, name = a.name }
                                       ).ToDictionary(r => r.groupId, rv => rv)
                                  );
        }



        if (IsPrimaryLanguage)
          SyncAttributeSets();

        SyncAttributes();

        if (IsPrimaryLanguage)
          SyncAttributesInSets();

        if (UsesConfigurableProducts)
        {
          using (var helper = new AssortmentHelper(Connector.Connection, Version))
          {

            int storeid = 0;
            if (MultiLanguage && !IsPrimaryLanguage)
              storeid = StoreList[CurrentStoreCode].store_id;

            var attributeList = helper.GetAttributeList(PRODUCT_ENTITY_TYPE_ID);
            foreach (var avg in AttributeValueGroups)
            {

              var groupedAttribute = attributeList["g_" + avg.Key];
              foreach (var avg_value in avg.Value)
              {
                var x = helper.GetOptionId(
                  attribute_id: groupedAttribute.attribute_id,
                  store_id: storeid,
                  value: avg_value.Value.name,
                  concentrator_attribute_value_id: "g_" + avg_value.Key.ToString(),
                  grouped: true,
                  sort_order: avg_value.Value.score
                  );

              }

            }
          }
        }

        ExportCategories();

        SyncProducts();

        if (IsPrimaryLanguage)
          SyncAccruels();

      }

      var mappingIds = (from pg in Assortment.SelectMany(c => c.ProductGroups)
                        let key = GetMappingIDs(pg)
                        select key).ToList().SelectMany(c => c).Distinct().ToList();

      List<int> toRemove = new List<int>();

      using (var helper = new AssortmentHelper(Connector.Connection, Version))
      {
        var categories = helper.GetWebsiteCategories(StoreList[CurrentStoreCode].website_id);

        foreach (var cat in categories)
        {
          if (!mappingIds.Any(x => cat.icecat_value.EndsWith(x)))
            toRemove.Add(cat.entity_id);
        }

        helper.TurnOffCategories(toRemove, MagentoIgnoreCategoryDisabling);
        //helper.RemoveEmptyCategories(toRemove
      }
    }

    private List<string> GetMappingIDs(AssortmentProductGroup gro, List<string> ids = null)
    {
      if (ids == null) ids = new List<string>();

      string key = (gro.ParentProductGroup != null ? gro.ParentProductGroup.MappingID.ToString() + "-" : string.Empty) + gro.MappingID;
      ids.Add(key);

      if (gro.ParentProductGroup != null)
        GetMappingIDs(gro.ParentProductGroup, ids);

      return ids;
    }

    private void SyncPriceSets()
    {

      var t = typeof(Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient);
      var method = t.GetMethod("GetPriceSets");

      if (method == null)
        return;


      //GetPriceSets
      Logger.DebugFormat("Exporting price sets for language '{0}'", CurrentLanguage.Language.Name);



      PHPSerializer serializer = new PHPSerializer();

      var priceSets = (from p in PriceSetsXml.Root.Elements("PriceSet")
                       select p);

      using (var helper = new PriceSetHelper(Connector.Connection))
      {
        var currentRules = helper.GetSalesRules(0);
        var customerGroups = helper.GetCustomerGroups();

        var first = currentRules.First();


        object con = serializer.Deserialize(first.conditions_serialized);
        object act = serializer.Deserialize(first.actions_serialized);
        foreach (var ps in priceSets)
        {

          decimal newPrice = Convert.ToDecimal(ps.Attribute("Price").Value);
          var products = ps.Elements("Product");

          int qty = Convert.ToInt32(products.First().Attribute("Quantity").Value);

          Hashtable conditions = BuildConditions(ps);
          Hashtable actions = BuildActions(ps, qty);

          var entity = currentRules.SingleOrDefault(x => x.rule_id == Convert.ToInt32(ps.Attribute("PriceSetID").Value) * 100);

          if (entity == null)
          {
            entity = new salesrule() { rule_id = Convert.ToInt32(ps.Attribute("PriceSetID").Value) * 100 };
            entity.from_date = DateTime.Now;
          }
          entity.is_active = true;

          entity.name = ps.Attribute("Name").Value;
          entity.description = ps.Attribute("Description").Value;
          entity.label = entity.name;


          entity.conditions_serialized = serializer.Serialize(conditions);
          entity.actions_serialized = serializer.Serialize(actions);
          entity.simple_action = "by_fixed";
          entity.is_advanced = true;
          entity.discount_step = 1;
          entity.is_rss = true;
          entity.website_ids = "1";
          entity.coupon_type = 1;
          entity.uses_per_customer = 0;
          entity.customer_group_ids = String.Join(",", customerGroups);


          if (products.Count() == 1)
          {

            entity.discount_amount = newPrice / (decimal)qty;
            entity.discount_step = qty;
          }

          helper.SyncSalesRule(entity);

        }
      }



    }

    private Hashtable BuildConditions(XElement ps)
    {
      Hashtable result = new Hashtable();

      result.Add("attribute", null);
      result.Add("is_value_processed", null);
      result.Add("type", "salesrule/rule_condition_combine");
      result.Add("aggregator", "all");
      result.Add("operator", null);
      result.Add("value", "1");
      return result;
    }

    private Hashtable BuildActions(XElement ps, int qty)
    {
      var cd = new ArrayList();


      var cd1 = new Hashtable();
      cd1.Add("value", qty);
      cd1.Add("is_value_processed", false);
      cd1.Add("type", "salesrule/rule_condition_product");
      cd1.Add("operator", ">=");
      cd1.Add("attribute", "quote_item_qty");

      var cd2 = new Hashtable();
      cd2.Add("value", "1EF5033111");
      cd2.Add("is_value_processed", false);
      cd2.Add("type", "salesrule/rule_condition_product");
      cd2.Add("operator", "{}"); // contains
      cd2.Add("attribute", "sku");

      cd.Add(cd1);
      cd.Add(cd2);



      Hashtable result = new Hashtable();
      result.Add("conditions", cd);
      result.Add("attribute", null);
      result.Add("is_value_processed", null);
      result.Add("type", "salesrule/rule_condition_product_combine");
      result.Add("aggregator", "all");
      result.Add("operation", null);
      result.Add("value", "1");
      return result;
    }

    private void SyncAccruels()
    {
      Logger.DebugFormat("Exporting accruels for language '{0}'", CurrentLanguage.Language.Name);


      using (var Soap = new Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient())
      {
        AccruelsXml = XDocument.Parse(Soap.GetAccruels(Connector.ConnectorID));
      }



      using (var helper = new AccruelsHelper(Connector.Connection))
      {

        var attributeList = helper.GetAttributeList(PRODUCT_ENTITY_TYPE_ID);
        var skuList = helper.GetSkuList();

        helper.EnsureAttributeGroup("Prices", PRODUCT_ENTITY_TYPE_ID);

        var attributeGroupsPerSet = helper.GetAttributeGroupsPerSet(PRODUCT_ENTITY_TYPE_ID);

        var accruelTypes = (from r in AccruelsXml.Root.Elements("Product").Elements("Accruels").Elements("Accruel")
                            select new
                            {
                              Name = r.Attribute("Description").Value,
                              Code = r.Attribute("AccruelCode").Value
                            }).Distinct().ToList();


        eav_attribute attribute = null;
        List<eav_attribute> accruelAttributes = new List<eav_attribute>();

        foreach (var accruelType in accruelTypes)
        {
          if (!attributeList.TryGetValue(accruelType.Code, out attribute))
          {
            attribute = helper.CreateAttribute(accruelType.Code, PRODUCT_ENTITY_TYPE_ID, accruelType.Name, datatype: "weee", addToDefaultSet: false, is_required: false, is_user_defined: false);
            attributeList.Add(accruelType.Code, attribute);

          }
          else
          {

            attribute.backend_model = "weee/attribute_backend_weee_tax";
            attribute.backend_type = "static";
            attribute.frontend_input = "weee";
          }
          helper.SyncAttribute(attribute, false);

          helper.EnsureEntityAttribute(attribute, PRODUCT_ENTITY_TYPE_ID, "Prices");
        }

        var currentAccruels = helper.GetWeeeTaxList(0);


        List<int> touched_valued_id_list = new List<int>();

        catalog_product_entity entity = null;
        foreach (var accruelProduct in AccruelsXml.Root.Elements("Product"))
        {

          var sku = accruelProduct.Attribute("ManufacturerID").Value;
          if (!skuList.TryGetValue(sku, out entity))
            continue;


          var accruels = accruelProduct.Element("Accruels").Elements("Accruel");
          foreach (var accruel in accruels)
          {

            var code = accruel.Attribute("AccruelCode").Value;


            var value = Convert.ToDecimal(accruel.Attribute("UnitPrice").Value, new CultureInfo("en-US"));

            var accruel_entity = currentAccruels.SingleOrDefault(a => a.entity_id == entity.entity_id
              && a.attribute_id == attributeList[code].attribute_id
              && a.entity_type_id == PRODUCT_ENTITY_TYPE_ID
              && a.country == "BE"
              );

            if (accruel_entity == null)
            {
              accruel_entity = new weee_tax()
              {
                website_id = 0,
                entity_id = entity.entity_id,
                attribute_id = attributeList[code].attribute_id,
                entity_type_id = PRODUCT_ENTITY_TYPE_ID,
                country = "BE"
              };

            }
            accruel_entity.value = value;
            accruel_entity.state = "*";


            helper.SyncWeeeAttribute(accruel_entity);

            touched_valued_id_list.Add(accruel_entity.value_id);

          }


        }

        Logger.DebugFormat("Cleaning old weee records from table");
        helper.CleanupWeeeAttributes(touched_valued_id_list, CurrentLanguage.Country);


      }


      Logger.DebugFormat("Finished exporting accruels language '{0}'", CurrentLanguage.Language.Name);

    }

    public class ProductAttributeInfo
    {
      public int ProductID { get; set; }
      public string SKU { get; set; }
      public IEnumerable<XElement> AttributeGroups { get; set; }
      public IEnumerable<XElement> Attributes { get; set; }

    }

    private void SyncProducts()
    {
      Logger.DebugFormat("Exporting products for language '{0}'", CurrentLanguage.Language.Name);

      var doRetailStock = Connector.ConnectorSystemType != null && ((ConnectorSystemType)Connector.ConnectorSystemType).Has(ConnectorSystemType.ExportRetailStock);

      var retailStockList = new Dictionary<int, IEnumerable<XElement>>();
      if (IsPrimaryLanguage && doRetailStock)
      {
        var soap = new AssortmentService();
        var retailStock = new XDocument(soap.GetStockAssortment(Connector.ConnectorID, null));
        if (retailStock.Root != null)
          retailStockList = (from r in retailStock.Root.Elements("Product")
                             select new
                             {
                               product_id = Convert.ToInt32(r.Attribute("ProductID").Value),
                               store_records = r.Element("Stock").Element("Retail").Elements("RetailStock")
                             })
                              .Distinct()
                              .ToDictionary(x => x.product_id, y => y.store_records);
      }

      var stockStoreList = new Dictionary<string, int>();
      if (doRetailStock)
      {
        using (var helper = new AssortmentHelper(Connector.Connection, Version))
        {
          stockStoreList = helper.GetStockStoreList();
        }
      }

      var productsSource = (from p in Assortment
                            let ph = p.ProductGroups
                            where ((ph != null && p.ProductGroups.Any()) || p.IsNonAssortmentItem)
                            select p);

      var products = productsSource.Where(x => !x.IsConfigurable).ToList();

      ProcessProductCollection(retailStockList, stockStoreList, products);

      #region Config products relations
      if (UsesConfigurableProducts)
      {
        products = productsSource.Where(x => x.IsConfigurable).ToList();

        ProcessProductCollection(retailStockList, stockStoreList, products);

        var ftpInfo = SftpHelper.GetFtpInfo(Connector);

        var basePath = ftpInfo.FtpPath.Replace("/catalog/product", "");
        if (!basePath.EndsWith("/"))
          basePath += "/";

        var client = SftpHelper.GetSFTPClient(ftpInfo);
        SftpHelper.EnsurePath(client, basePath + "colorcodes");
        SftpHelper.EnsurePath(client, basePath + "colorcodes/basic");

        using (var helper = new AssortmentHelper(Connector.Connection, Version))
        {

          var attributeValueGroups = AttributesXml.Root.Element("AttributeValueGroups");
          if (attributeValueGroups != null)
          {
            var groups = (from el in attributeValueGroups.Elements("AttributeValueGroup")
                          where el.Attribute("Image") != null && !String.IsNullOrEmpty(el.Attribute("Image").Value)
                          group el by Convert.ToInt32(el.Attribute("AttributeID").Value) into grouped
                          select grouped);

            foreach (var groupedAttribute in groups)
            {
              var attribute = helper.GetAttributeList(PRODUCT_ENTITY_TYPE_ID)["g_" + groupedAttribute.Key];
              var options = helper.GetAttributeOptions(attribute.attribute_id);

              foreach (var attributeValueGroup in groupedAttribute)
              {

                string attribute_option_key = "g_" + attributeValueGroup.Attribute("AttributeValueGroupID").Value;
                if (options.ContainsKey(attribute_option_key))
                {
                  string file = String.Format("colorcodes/basic/color_{0}.png", options[attribute_option_key]);

                  DownloadImage(new Uri(attributeValueGroup.Attribute("Image").Value), client,
                    basePath + file, Logger);
                  //sync image

                }

              }

            }
          }

        }
      }

      #endregion

      #region related products
      //check for related products
      if (RelatedProducts != null)
      {
        using (var helper = new AssortmentHelper(Connector.Connection, Version))
        {
          var skuList = helper.GetSkuList();
          foreach (var rp in RelatedProducts)
          {
            if (!skuList.ContainsKey(rp.Key))
              continue;

            int parent_id = skuList[rp.Key].entity_id;

            Dictionary<int, int> childrenWithIndex = new Dictionary<int, int>();
            foreach (var child in rp.Value)
            {
              var childId = skuList[child.RelatedProductManufacturerID].entity_id;
              if (skuList.ContainsKey(child.RelatedProductManufacturerID) && !childrenWithIndex.ContainsKey(childId))
                childrenWithIndex.Add(childId, child.Index);
            }
            if (childrenWithIndex.Count > 0)
              helper.SyncProductLink(parent_id, childrenWithIndex, 4);
          }
        }
      }

      if (CrossSellProducts != null)
      {
        using (var helper = new AssortmentHelper(Connector.Connection, Version))
        {
          var skuList = helper.GetSkuList();
          foreach (var rp in CrossSellProducts)
          {
            if (!skuList.ContainsKey(rp.Key))
              continue;

            int parent_id = skuList[rp.Key].entity_id;

            Dictionary<int, int> childrenWithIndex = new Dictionary<int, int>();
            foreach (var child in rp.Value)
            {
              var childId = skuList[child.RelatedProductManufacturerID].entity_id;
              if (skuList.ContainsKey(child.RelatedProductManufacturerID) && !childrenWithIndex.ContainsKey(childId))
                childrenWithIndex.Add(childId, child.Index);
            }
            if (childrenWithIndex.Count > 0)
              helper.SyncProductLink(parent_id, childrenWithIndex, 5);
          }
        }
      }

      if (RelatedProducts != null)
      {
        products = productsSource.Where(x => x.IsConfigurable).ToList();
      }

      #endregion
      // deactive old products      

      DeactivateProducts(productsSource);

      Logger.DebugFormat("Finished exporting products for language '{0}'", CurrentLanguage.Language.Name);
    }

    private void DeactivateProducts(IEnumerable<AssortmentProductInfo> productsSource)
    {
      #region Deactivating

      using (var helper = new AssortmentHelper(Connector.Connection, Version))
      {
        var skuList = helper.GetActiveSkus();
        var skuInXmlList = (from p in productsSource
                            where !p.IsNonAssortmentItem
                            select p.ManufacturerID).ToList();


        var toDeactivate = skuList.Where(x => !skuInXmlList.Contains(x.Key));

        //exclude shipping costs
        toDeactivate = toDeactivate.Where(x => !x.Key.StartsWith("ShippingCostProductID_"));

        if (toDeactivate.Count() > 0)
        {
          SortedDictionary<string, eav_attribute> attributeList = helper.GetAttributeList(PRODUCT_ENTITY_TYPE_ID);


          foreach (var p in toDeactivate)
          {
            SetProductStatus(p.Value, (int)PRODUCT_STATUS.DISABLED, helper, attributeList, StoreList[CurrentStoreCode].store_id);

            if (RequiresStockUpdate)
            {
              helper.SyncStock(new cataloginventory_stock_item()
              {
                product_id = p.Value,
                qty = 0,
                is_in_stock = false
              });
            }

            //cleanup its category relations
            if (IsPrimaryLanguage)
              helper.CleanupCategoryProductRelations(p.Value, helper.GetWebsiteCategories(StoreList[CurrentStoreCode].website_id).Select(c => c.entity_id).ToList());
          }
        }
      }
      #endregion
    }

    private void ProcessProductCollection(Dictionary<int, IEnumerable<XElement>> retailStockList, Dictionary<string, int> stockStoreList, List<AssortmentProductInfo> products)
    {

      var connectorMandantoryAttributes = Connector.ConnectorSettings.GetValueByKey<string>("MandantoryAttributes", String.Empty);
      var mandantoryAttributes = (from l in
                                    connectorMandantoryAttributes.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                                  select Convert.ToInt32(l)).ToArray();

      var productAttributes = (from p in AttributesXml.Root.Elements("ProductAttribute")
                               let productId = Convert.ToInt32(p.Attribute("ProductID").Value)
                               let attributesNode = p.Element("Attributes")
                               let groupsNode = p.Element("AttributeGroups")
                               where attributesNode != null
                               select new ProductAttributeInfo
                               {
                                 ProductID = productId,
                                 AttributeGroups = groupsNode != null ? groupsNode.Elements("AttributeGroup") : null,
                                 Attributes = attributesNode.Elements("Attribute").Where(l =>

                                   (Convert.ToBoolean(l.Attribute("KeyFeature").Value)
                                   || mandantoryAttributes.Contains(Convert.ToInt32(l.Attribute("AttributeID").Value)))

                                   )
                               }).ToDictionary(x => x.ProductID, y => y);

      int totalRecords = products.Count();

      Logger.DebugFormat("Found {0} products", totalRecords);




      int totalProcessed = 0;
      SortedDictionary<string, eav_attribute> attributeList;
      List<catalog_category_entity> categoryList;
      List<int> websiteCatList;
      Dictionary<decimal, int> taxClasses;
      using (var helper = new AssortmentHelper(Connector.Connection, Version))
      {
        attributeList = helper.GetAttributeList(PRODUCT_ENTITY_TYPE_ID);
        categoryList = helper.GetCategories();
        websiteCatList = helper.GetWebsiteCategories(StoreList[CurrentStoreCode].website_id).Select(c => c.entity_id).ToList();
        taxClasses = helper.GetTaxClasses();
      }

      try
      {

        ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = 8 };
        //ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = 32 };

        Parallel.ForEach(Partitioner.Create(0, totalRecords), options, (range, loopState) =>
        {

          using (var helper = new AssortmentHelper(Connector.Connection, Version))
          {

            var skuList = helper.GetSkuList();

            for (int index = range.Item1; index < range.Item2; index++)
            {
              int productID = Convert.ToInt32(products[index].ProductID);
              ProductAttributeInfo attributeInfo = null;
              productAttributes.TryGetValue(productID, out attributeInfo);

              IEnumerable<XElement> productRetailStock = null;
              retailStockList.TryGetValue(productID, out productRetailStock);

              ProcessProduct(helper, products[index], skuList, attributeList, categoryList, attributeInfo, stockStoreList, productRetailStock, websiteCatList, taxClasses: taxClasses);

              Interlocked.Increment(ref totalProcessed);
              if (totalProcessed % 100 == 0)
                Logger.DebugFormat(String.Format("Processed {0} of {1} records", totalProcessed, totalRecords));
            }
          }
        });
      }
      catch (Exception e) { }
    }

    private void ProcessProduct(AssortmentHelper helper, AssortmentProductInfo product,
        SortedDictionary<string, catalog_product_entity> skuList,
      SortedDictionary<string, eav_attribute> attributeList,
       List<catalog_category_entity> categoryList,
      ProductAttributeInfo productAttributes,
       Dictionary<string, int> stockStoreList,
      IEnumerable<XElement> productRetailStock,
      List<int> websiteCategoryList = null,
      Dictionary<decimal, int> taxClasses = null
      )
    {

      #region variables
      string store = "admin";
      string sku = product.ManufacturerID;
      int productID = product.ProductID;
      string customProductId = product.CustomProductID;

      bool isNonAssortmentItem = product.IsNonAssortmentItem;
      bool isConfigurable = product.IsConfigurable;

      string type = isConfigurable ? "configurable" : "simple";

      var contentNode = product.Content;

      if (contentNode == null)
        return;

      var additionalContentNode = (UseContentDescriptions && ContentXml != null) ? ContentXml.Root.Elements("Product").FirstOrDefault(x => Convert.ToInt32(x.Attribute("ProductID").Value) == productID) : null;

      bool ignoreProductStatusUpdates = Connector.ConnectorSettings.GetValueByKey<bool>("IgnoreProductStatusUpdates", false);
      bool defaultProductVisibility = Connector.ConnectorSettings.GetValueByKey<bool>("DefaultProductVisibility", true);
      int productStatus = (int)PRODUCT_STATUS.ENABLED; // enabled;

      if (!defaultProductVisibility)
        productStatus = (int)PRODUCT_STATUS.DISABLED; //disabled;
      #endregion

      #region Product Name

      string name = contentNode.ProductName;

      if (UsesShortDescAsName) //compatibility for CCAT
        name = contentNode.ShortDescription;


      if (Connector.ConcatenateBrandName)
      {
        StringBuilder sb = new StringBuilder();
        sb.Append(product.Brand.Name);
        sb.Append(" ");
        sb.Append(name);
        name = sb.ToString();
      }

      if (additionalContentNode != null)
      {
        string additional_name = string.Empty;

        if (UsesShortDescAsName)
          additional_name = additionalContentNode.Element("Content").Attribute("ShortDescription").Value;
        else
          additional_name = additionalContentNode.Element("Content").Attribute("ProductName").Value;

        if (!String.IsNullOrEmpty(additional_name))
          name = additional_name;
      }

      #endregion

      #region Short Description

      var shortContentDescription = contentNode.ShortDescription;

      string shortDescription = shortContentDescription ?? string.Empty;
      shortDescription = shortDescription.Replace("\n", @"<br/>");


      if (additionalContentNode != null)
      {
        var additional_short = additionalContentNode.Element("Content").Attribute("ShortContentDescription").Value;
        if (!String.IsNullOrEmpty(additional_short))
        {
          shortDescription = additional_short;
          if (UsesShortDescAsName)
            name = additional_short;
        }
      }

      #endregion

      #region Long Description

      var longContentDescription = contentNode.LongDescription;

      string longDescription = longContentDescription ?? string.Empty;
      longDescription = longDescription.Replace("\n", @"<br/>");



      if (additionalContentNode != null)
      {
        var additional_long = additionalContentNode.Element("Content").Attribute("LongContentDescription").Value;
        if (!String.IsNullOrEmpty(additional_long))
          longDescription = additional_long;
      }

      #endregion

      #region Price
      decimal? price = null;
      decimal? specialPrice = null;
      decimal? taxRate = null;
      string commercialStatus = "O";
      ExtractPrice(product, ref price, ref specialPrice, ref commercialStatus, ref taxRate);
      #endregion

      #region Barcode
      string primaryBarcode = String.Empty;
      var barcodeNode = product.Barcodes;
      if (barcodeNode != null)
      {
        var barcodes = (from l in barcodeNode
                        select l.Trim()).ToList();
        if (barcodes.Count > 0)
          primaryBarcode = barcodes[0];
      }
      #endregion

      #region
      var brandName = product.Brand.Name;

      catalog_product_entity entity = null;

      if (!skuList.TryGetValue(sku, out entity))
      {
        var entry = ProductsPerAttributeSet.Where(x => x.Value.Contains(productID)).SingleOrDefault();

        eav_attribute_set set = null;

        if (string.IsNullOrEmpty(entry.Key) && isNonAssortmentItem) //non product items -> shipping costs, return costs
        {
          set = helper.GetAttributeSet(name: _nonAssortmentItemsSetName, entity_type_id: PRODUCT_ENTITY_TYPE_ID);
        }
        else
        {
          set = helper.GetAttributeSet(name: entry.Key, entity_type_id: PRODUCT_ENTITY_TYPE_ID);
        }
        entity = new catalog_product_entity()
        {
          sku = sku,
          entity_type_id = PRODUCT_ENTITY_TYPE_ID,
          type_id = type,
          attribute_set_id = set.attribute_set_id
        };

        helper.AddProduct(entity);

        ignoreProductStatusUpdates = false;
      }

      int storeid = 0;
      if (MultiLanguage && !IsPrimaryLanguage)
        storeid = StoreList[CurrentStoreCode].store_id;

      if (IsPrimaryLanguage)
      {
        helper.SyncAttributeValue(attributeList["custom_item_number"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, customProductId);
        helper.SyncAttributeValue(attributeList["concentrator_product_id"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, productID, "int");

        helper.SyncAttributeValue(attributeList["commercial_status"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, commercialStatus);

        //deze doet het niet
        helper.SyncAttributeValue(attributeList["manufacturer"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, brandName, "option");

        int taxClassID = 2;

        if (taxRate.HasValue)
        {
          if (taxClasses.ContainsKey(taxRate.Value)) taxClassID = taxClasses[taxRate.Value];

        }

        helper.SyncAttributeValue(attributeList["tax_class_id"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, taxClassID, "int");

        helper.SyncAttributeValue(attributeList["weight"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, 1, "decimal");

        helper.SyncAttributeValue(attributeList["barcode"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, primaryBarcode);
      }

      helper.SyncAttributeValue(attributeList["name"].attribute_id, PRODUCT_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, entity.entity_id, name);
      helper.SyncAttributeValue(attributeList["short_description"].attribute_id, PRODUCT_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, entity.entity_id, shortDescription);
      helper.SyncAttributeValue(attributeList["description"].attribute_id, PRODUCT_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, entity.entity_id, longDescription, type: "text");

      if (IsPrimaryLanguage)
      {
        helper.SyncAttributeValue(attributeList["name"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, name);
        helper.SyncAttributeValue(attributeList["short_description"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, shortDescription);
        helper.SyncAttributeValue(attributeList["description"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, longDescription, type: "text");
      }



      int visibility = 4; //catalog, search
      if (isNonAssortmentItem || (UsesConfigurableProducts && SimpleProducts.Contains(productID)) || !product.Visible)
      {
        visibility = 1; // not visible individually       
      }

      bool childrenOnStock = false;
      if (isConfigurable && ConfigurableProducts.ContainsKey(productID))
      {
        var children = ConfigurableProducts[productID];

        childrenOnStock = (children.Any(x => x.Stock != null && x.Stock.InStock > 0));

        ExtractPrice(children.First(), ref price, ref specialPrice, ref commercialStatus, ref taxRate);

        var child_ids = (from c in children
                         let child_sku = c.ManufacturerID
                         where skuList.ContainsKey(child_sku)
                         select skuList[child_sku].entity_id).ToList();

        List<catalog_product_super_attribute> configurable_attribute_ids = (from ca
                                              in product.ConfigurableAttributes
                                                                            let key = FormatConfigurableMagentoAttributeCode(ca)
                                                                            where attributeList.ContainsKey(key)
                                                                            select new catalog_product_super_attribute()
                                                                            {
                                                                              attribute_id = attributeList[key].attribute_id,
                                                                              use_default = true,
                                                                              label = attributeList[key].frontend_label,
                                                                              position = ca.ConfigurablePosition,
                                                                              product_id = entity.entity_id,
                                                                              product_super_attribute_id = 0
                                                                            }

                                                ).ToList();


        helper.SyncProductSuperAttributes(configurable_attribute_ids);

        helper.SyncProductRelations(entity.entity_id, child_ids);
        helper.SyncAttributeValue(attributeList["options_container"].attribute_id, PRODUCT_ENTITY_TYPE_ID, storeid, entity.entity_id, "container2", "varchar");

      }


      helper.SyncAttributeValue(attributeList["visibility"].attribute_id, PRODUCT_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, entity.entity_id, visibility, "int");
      helper.SyncAttributeValue(attributeList["price"].attribute_id, PRODUCT_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, entity.entity_id, price, "decimal");

      if (IsPrimaryLanguage)
      {
        helper.SyncAttributeValue(attributeList["visibility"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, visibility, "int");
        helper.SyncAttributeValue(attributeList["price"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, price, "decimal");
      }

      helper.SyncAttributeValue(attributeList["special_price"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, null, "decimal");


      if ((specialPrice ?? 0) > 0)
      {
        helper.SyncAttributeValue(attributeList["special_price"].attribute_id, PRODUCT_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, entity.entity_id, specialPrice, "decimal");
        if (Connector.ConnectorID == 11)
        {
          helper.SyncAttributeValue(attributeList["special_price"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, specialPrice, "decimal");
        }

      }
      else
      {
        helper.DeleteAttributeValue(attributeList["special_price"].attribute_id, PRODUCT_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, entity.entity_id, "decimal");
      }




      if (IsPrimaryLanguage)
      {
        int? quantityOnHand = null;
        int? quantityToReceive = null;
        string stockStatus = "S";
        DateTime? promisedDeliveryDate = null;
        var stockElement = product.Stock;
        if (stockElement != null)
        {
          quantityOnHand = stockElement.InStock;
          quantityToReceive = stockElement.QuantityToReceive;
          stockStatus = stockElement.StockStatus;


          DateTime dt;
          //if (DateTime.TryParseExact(stockElement.Attribute("PromisedDeliveryDate").Value, "dd-M-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
          if (stockElement.PromisedDeliveryDate.HasValue)
            promisedDeliveryDate = stockElement.PromisedDeliveryDate.Value;//dt;

        }

        if (productRetailStock != null)
        {
          if (productRetailStock.Count() > 0)
          {
            foreach (var s in stockStoreList)
            {

              var store_record = productRetailStock.FirstOrDefault(x => x.Attribute("VendorCode").Value == s.Key);
              int qty = (store_record != null) ? Convert.ToInt32(store_record.Attribute("InStock").Value) : 0;

              helper.SyncStoreStock(sku, s.Value, qty);

            }
          }
          else
          {
            helper.ClearStoreStock(sku);
            //no stores left with stock, clear all
          }
        }

        helper.SyncWebsiteProduct(StoreList[CurrentStoreCode].website_id, entity.entity_id);

        if (RequiresStockUpdate)
        {
          helper.SyncAttributeValue(attributeList["quantity_to_receive"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, quantityToReceive ?? 0, "int");
          helper.SyncAttributeValue(attributeList["promised_delivery_date"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, promisedDeliveryDate, "datetime");
          helper.SyncAttributeValue(attributeList["stock_status"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, stockStatus, "varchar");

          bool is_in_stock = quantityOnHand > 0 || (promisedDeliveryDate.HasValue && quantityToReceive > 0) || (productRetailStock != null && productRetailStock.Count() > 0);
          //|| (!String.IsNullOrEmpty(stockStatus) && stockStatus != "O");

          if (isConfigurable)
          {
            if (childrenOnStock)
              is_in_stock = true;
            else
              is_in_stock = false;
          }

          helper.SyncStock(new cataloginventory_stock_item()
          {
            product_id = entity.entity_id,
            qty = quantityOnHand ?? 0,
            is_in_stock = is_in_stock
          });

          if (!is_in_stock)
            productStatus = (int)PRODUCT_STATUS.DISABLED;

          SetProductStatus(entity.entity_id, productStatus, helper, attributeList, 0);
        }
      }
      #endregion

      SetProductStatus(entity.entity_id, productStatus, helper, attributeList, StoreList[CurrentStoreCode].store_id);
      helper.SetProductLastModificationTime(entity.entity_id);

      #region Categories
      List<int> currentCatIds = new List<int>();

      var hg = product.ProductGroups;
      if (hg != null)
      {
        foreach (var pg in hg)
        {
          var key = pg.MappingID.ToString();
          var parent = pg.ParentProductGroup;

          if (parent != null)
          {
            key = parent.MappingID.ToString() + "-" + pg.MappingID.ToString();
          }

          var cat = categoryList.FirstOrDefault(x => x.icecat_value.StartsWith(string.Format("{0}_", StoreList[CurrentStoreCode].website_id)) && x.icecat_value.EndsWith(key));
          if (cat != null)
          {
            helper.SyncCategoryProduct(cat.entity_id, entity.entity_id, 0);
            currentCatIds.Add(cat.entity_id);
          }

          var tmpParent = pg.ParentProductGroup;
          while (tmpParent != null)
          {
            tmpParent = tmpParent.ParentProductGroup;
          }
        }
      }

      helper.CleanupCategoryProducts(entity.entity_id, currentCatIds, websiteCategoryList);
      #endregion

      #region Attributes

      if (productAttributes != null)
      {
        foreach (var attribute in productAttributes.Attributes)
        {

          var attributeValueGroupsElement = attribute.Element("AttributeValueGroups");

          bool isAlsoGrouped = attributeValueGroupsElement != null && attributeValueGroupsElement.Elements("Group").Count() > 0;


          string code = FormatMagentoAttributeCode(attribute);
          eav_attribute product_attribute;
          if (!attributeList.TryGetValue(code, out product_attribute))
            continue;

          int concentrator_attribute_id = Convert.ToInt32(attribute.Attribute("AttributeID").Value);

          int? concentrator_attribute_value_id = null;
          if (attribute.Attribute("AttributeOriginalValue") != null)
          {
            int x = 0;
            if (Int32.TryParse(attribute.Attribute("AttributeOriginalValue").Value, out x))
              concentrator_attribute_value_id = x;

          }


          var value = HttpUtility.HtmlDecode(attribute.Element("Value").Value + " " + attribute.Element("Sign").Value);

          storeid = 0;
          //if (MultiLanguage && !IsPrimaryLanguage)
          storeid = StoreList[CurrentStoreCode].store_id;

          string attributeType = "varchar";

          int sort_order = 0;

          if (Convert.ToBoolean(attribute.Attribute("IsSearchable").Value))
            attributeType = "option";

          if (isAlsoGrouped)
          {
            var avgs = AttributeValueGroups[concentrator_attribute_id];

            var val = avgs.Values.Where(x => x.name == value.Trim()).FirstOrDefault();
            if (val != null)
            {
              sort_order = val.score;
            }
          }
          else
          {
            //TODO: fix nicer
            if (GetOptionScoreFromOtherValueGroupsBasedOnGroupName)
            {
              var valueForScore = (from p in AttributeValueGroups
                                   let valueFromGroup = p.Value.Values.Where(l => l.name.Trim() == value.Trim()).FirstOrDefault()
                                   where valueFromGroup != null
                                   select valueFromGroup).FirstOrDefault();

              if (valueForScore != null)
              {
                sort_order = valueForScore.score;
              }
            }
          }

          helper.SyncAttributeValue(product_attribute.attribute_id, PRODUCT_ENTITY_TYPE_ID, storeid,
            entity.entity_id, value, type: attributeType,
          concentrator_attribute_value_id: concentrator_attribute_value_id.ToString(),
          sort_order: sort_order

          );

          if (!MultiLanguage || IsPrimaryLanguage)
          {
            helper.SyncAttributeValue(product_attribute.attribute_id, PRODUCT_ENTITY_TYPE_ID, 0,
            entity.entity_id, value, type: attributeType,
          concentrator_attribute_value_id: concentrator_attribute_value_id.ToString(),
          sort_order: sort_order
            );
          }

          if (isAlsoGrouped)
          {
            var groupedAttribute = attributeList["g_" + attribute.Attribute("AttributeID").Value];

            var avgs = AttributeValueGroups[concentrator_attribute_id];

            List<int> optionIds = new List<int>();
            foreach (var attributeValueGroup in (from g in attributeValueGroupsElement.Elements("Group")
                                                 select Convert.ToInt32(g.Attribute("ID").Value)))
            {
              optionIds.Add(helper.GetOptionId(
              attribute_id: groupedAttribute.attribute_id,
              store_id: 0,
              value: avgs[attributeValueGroup].name,
              concentrator_attribute_value_id: "g_" + attributeValueGroup.ToString(),
              grouped: true,
              sort_order: avgs[attributeValueGroup].score
              ));
            }


            helper.SyncAttributeValue(groupedAttribute.attribute_id, PRODUCT_ENTITY_TYPE_ID, storeid, entity.entity_id, String.Join(",", optionIds.Distinct()), type: "varchar");

            helper.SyncAttributeValue(groupedAttribute.attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, String.Join(",", optionIds.Distinct()), type: "varchar");
          }
        }
      }

      #endregion

      #region Update Catalog/Product Link & URL Rewrite
      //helper.UpdateCatalogCategoryProductIndex(entity);
      //helper.UpdateURLRewriteIndex(entity);
      #endregion

    }

    void DownloadImage(Uri source, TElSimpleSFTPClient client, string targetFile, IAuditLogAdapter Logger)
    {
      try
      {
        using (WebClient dlClient = new WebClient())
        {
#if DEBUG
          source = new Uri(source.AbsoluteUri.Replace("localhost", "172.16.250.94").Replace("ProductGroup", "Mapping"));
          //source = new Uri(source.AbsoluteUri.Replace("localhost", "10.172.26.1"));
#endif

          using (var stream = new MemoryStream(dlClient.DownloadData(source)))
          {
            client.UploadStream(stream, targetFile, SBUtils.TSBFileTransferMode.ftmOverwrite);
            //client.SetAttributes(targetFile, SftpHelper.DefaultAttributes);
          }
        }
      }
      catch (Exception)
      {
        return;
      }
    }

    private void SetProductStatus(int entity_id, int productStatus, AssortmentHelper helper, SortedDictionary<string, eav_attribute> attributeList, int store_id = 0)
    {
      if (!IgnoreProductStatusUpdates)
      {
        helper.SyncAttributeValue(attributeList["status"].attribute_id, PRODUCT_ENTITY_TYPE_ID, store_id, entity_id, productStatus, "int");
      }
    }

    private void ExtractPrice(AssortmentProductInfo product, ref decimal? price, ref decimal? specialPrice, ref string commercialStatus, ref decimal? taxRate)
    {

      var priceElement = product.Prices.FirstOrDefault();

      if (priceElement != null)
      {
        commercialStatus = priceElement.CommercialStatus.Trim();
        taxRate = priceElement.TaxRate;
        if (bool.Parse(Connector.ConnectorSettings.GetValueByKey("PriceInTax", "True")))
        {
          price = priceElement.UnitPrice *
                     ((priceElement.TaxRate / 100) + 1);


        }
        else
        {
          price = priceElement.UnitPrice;
        }


        if (priceElement.SpecialPrice.HasValue)
        {
          if (bool.Parse(Connector.ConnectorSettings.GetValueByKey("PriceInTax", "True")))
          {
            specialPrice = priceElement.SpecialPrice.Value *
                       ((priceElement.SpecialPrice.Value / 100) + 1);

          }
          else
          {
            specialPrice = priceElement.SpecialPrice.Value;
          }
        }

      }
    }

    private void SyncAttributesInSets()
    {
      Logger.DebugFormat("Synchronizing Attribute/Groups");

      var productList = AttributesXml.Root.Elements("ProductAttribute");

      Dictionary<string, eav_attribute_set> attributeSets = null;
      Dictionary<int, Dictionary<string, eav_attribute_group>> attributeGroups = null;
      SortedDictionary<string, eav_attribute> attributes = null;

      using (var helper = new MagentoMySqlHelper(Connector.Connection))
      {


        attributeSets = helper.GetAttributeSetList(entity_type_id: PRODUCT_ENTITY_TYPE_ID);
        attributeGroups = helper.GetAttributeGroupsPerSet(entity_type_id: PRODUCT_ENTITY_TYPE_ID);
        attributes = helper.GetAttributeList(entity_type_id: PRODUCT_ENTITY_TYPE_ID);

      }


      int totalRecords = ProductsPerAttributeSet.Count;
      int totalProcessed = 0;

      ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = 32 };


      var keys = ProductsPerAttributeSet.Keys.ToList();

      var connectorMandantoryAttributes = Connector.ConnectorSettings.GetValueByKey<string>("MandantoryAttributes", String.Empty);
      var mandantoryAttributes = (from l in
                                    connectorMandantoryAttributes.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                                  select Convert.ToInt32(l)).ToArray();





      Parallel.ForEach(Partitioner.Create(0, totalRecords), options, (range, loopState) =>
      {

        using (var helper = new MagentoMySqlHelper(Connector.Connection))
        {

          for (int index = range.Item1; index < range.Item2; index++)
          {

            List<int> products = ProductsPerAttributeSet[keys[index]];

            eav_attribute_set currentAttributeSet = null;

            if (!attributeSets.TryGetValue(keys[index], out currentAttributeSet))
              continue;

            var attributesInThisSet = (from p in productList
                                       let attributesNode = p.Element("Attributes")
                                       where products.Contains(Convert.ToInt32(p.Attribute("ProductID").Value))
                                       && attributesNode != null
                                       select attributesNode.Elements("Attribute"))
                                       .SelectMany(x => x)
                                       .Where(l => Convert.ToBoolean(
                                         (Convert.ToBoolean(l.Attribute("KeyFeature").Value)
                        || mandantoryAttributes.Contains(Convert.ToInt32(l.Attribute("AttributeID").Value))))
                                       )
                                       .GroupBy(at => at.Attribute("AttributeID").Value).Select(x => x.First()).ToList();

            var currentEntityAttributes = helper.GetEntityAttributeList(entity_type_id: PRODUCT_ENTITY_TYPE_ID, attribute_set_id: currentAttributeSet.attribute_set_id);


            foreach (var attribute in attributesInThisSet)
            {
              bool isAlsoGrouped = attribute.Element("AttributeValueGroups") != null && attribute.Element("AttributeValueGroups").Elements("Group").Count() > 0;

              for (int idx = 0; idx < 1 + (isAlsoGrouped ? 1 : 0); idx++)
              {

                string attributeCode = FormatMagentoAttributeCode(attribute);
                if (idx > 0)
                  attributeCode = "g_" + attribute.Attribute("AttributeID").Value;

                #region Get Attribute (and Group)

                eav_attribute_group currentAttributeGroup = null;

                if (!attributeGroups[currentAttributeSet.attribute_set_id].TryGetValue(AttributeGroups[Convert.ToInt32(attribute.Attribute("AttributeGroupID").Value)].Attribute("Name").Value
                  , out currentAttributeGroup))
                  continue;

                eav_attribute currentAttribute = null;
                if (!attributes.TryGetValue(attributeCode, out currentAttribute))
                  continue;

                #endregion

                eav_entity_attribute eea = currentEntityAttributes.SingleOrDefault(x => x.attribute_group_id == currentAttributeGroup.attribute_set_id && x.attribute_id == currentAttribute.attribute_id);
                if (eea == null)
                {
                  eea = helper.CreateEntityAttribute(
                        entity_type_id: PRODUCT_ENTITY_TYPE_ID,
                        attribute_set_id: currentAttributeSet.attribute_set_id,
                        attribute_group_id: currentAttributeGroup.attribute_group_id,
                        attribute_id: currentAttribute.attribute_id
                        );
                }
                helper.SyncEntityAttribute(eea);
              }

            }

            Interlocked.Increment(ref totalProcessed);
            if (totalProcessed % 100 == 0)
              Logger.DebugFormat(String.Format("Processed {0} of {1} records", totalProcessed, totalRecords));
          }

        }
      });
    }

    private class AttributeValueGroupInfo
    {
      public int groupId { get; set; }
      public string name { get; set; }
      public string image { get; set; }
      public int score { get; set; }
    }

    private class AttributeInfo
    {
      public bool IsConfigurable { get; set; }
      public bool ConfigurableAttributeType { get; set; }
      public bool UsedForPromoRules { get; set; }
      public int AttributeID { get; set; }
      public string Name { get; set; }
      public bool KeyFeature { get; set; }
      public bool Mandantory { get; set; }
      public bool IsSearchable { get; set; }
      public int AttributeGroupID { get; set; }
      public string AttributeCode { get; set; }
      public string MagentoAttributeCode { get; set; }
      public string PresentationValue { get; set; }
      public bool IsMultiValued { get; set; }
      public int Position { get; set; }
      public Dictionary<int, AttributeValueGroupInfo> Values { get; set; }
    }
    private void SyncAttributes()
    {
      Logger.DebugFormat("Synchronizing Attribute Definitions");

      var productList = AttributesXml.Root.Elements("ProductAttribute");

      var attributeGroupSource = (from p in productList
                                  let ag = p.Element("AttributeGroups")
                                  where ag != null
                                  select ag.Elements("AttributeGroup")).SelectMany(x => x)
                 .GroupBy(e => e.Attribute("Name").Value).Select(grp => grp.First())
                 .OrderBy(x => x.Attribute("AttributeGroupIndex").Value);





      var productsWithAttributes = (from p in productList
                                    let attributesNode = p.Element("Attributes")
                                    where attributesNode != null
                                    select attributesNode);

      var attributeList = (from an in productsWithAttributes
                           select an.Elements("Attribute")).SelectMany(x => x);

      var connectorMandantoryAttributes = Connector.ConnectorSettings.GetValueByKey("MandantoryAttributes", String.Empty);
      var connectorNotSearchableAttributes = Connector.ConnectorSettings.GetValueByKey("NotSearchableAttributes", string.Empty);
      var notConfigurableAttributes = Connector.ConnectorSettings.GetValueByKey("NoConfigurableAttributesOverrides", string.Empty);
      var connectorProductAttributeMetaDataReIndex = Connector.ConnectorSettings.GetValueByKey("ProductAttributeMetaDataReIndex", string.Empty);

      var mandantoryAttributes = (from l in
                                    connectorMandantoryAttributes.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                                  select Convert.ToInt32(l)).ToArray();

      var notSearchableAttributes = (from l in
                                       connectorNotSearchableAttributes.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                                     select Convert.ToInt32(l)).ToArray();

      var notConfigurableAttributesOverrides = (from l in
                                                  notConfigurableAttributes.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                                                select Convert.ToInt32(l)).ToArray();


      var reIndexAttributesArray = (from l in
                                      connectorProductAttributeMetaDataReIndex.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                                    select l);

      var reIndexAttributes = reIndexAttributesArray.Select(record => record.ToString().Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries)).ToDictionary(data => Convert.ToInt32(data[0]), data => Convert.ToInt32(data[1]));


      //Attributes will be marked as Used in product listing, apply to configurable types
      var connectorListingAttributes = Connector.ConnectorSettings.GetValueByKey<string>("ListingAttributes", String.Empty);

      var listingAttributes = (from l in connectorListingAttributes.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                               select Convert.ToInt32(l)).ToArray();
      int newIndex = -1;
      var attributes = (from l in attributeList
                        let attributeId = Convert.ToInt32(l.Attribute("AttributeID").Value)
                        let newIndexFound = reIndexAttributes.TryGetValue(attributeId, out newIndex)
                        where (Convert.ToBoolean(l.Attribute("KeyFeature").Value) || mandantoryAttributes.Contains(attributeId))
                        select new AttributeInfo
                        {
                          IsConfigurable = (l.Attribute("IsConfigurable") != null && Boolean.Parse(l.Attribute("IsConfigurable").Value)) && Convert.ToBoolean(l.Attribute("KeyFeature").Value),
                          ConfigurableAttributeType = listingAttributes.Contains(attributeId),
                          AttributeID = attributeId,
                          Name = l.Element("Name").Value,
                          KeyFeature = Convert.ToBoolean(l.Attribute("KeyFeature").Value),
                          Mandantory = mandantoryAttributes.Contains(attributeId),
                          IsSearchable = !notSearchableAttributes.Contains(attributeId) && Convert.ToBoolean(l.Attribute("IsSearchable").Value),
                          AttributeGroupID = Convert.ToInt32(l.Attribute("AttributeGroupID").Value),
                          AttributeCode = l.Attribute("AttributeCode").Value,
                          MagentoAttributeCode = FormatMagentoAttributeCode(l),
                          PresentationValue = HttpUtility.HtmlDecode(l.Element("Value").Value + " " + l.Element("Sign").Value),
                          Position = newIndexFound ? newIndex : Convert.ToInt32(l.Attribute("Index").Value)
                        }).GroupBy(a => a.AttributeID).Select(g => g.First()).ToList();





      if (AttributeValueGroups.Count > 0)
      {
        foreach (var groupedAttribute in AttributeValueGroups)
        {
          var sourceAttribute = attributes.Single(x => x.AttributeID == groupedAttribute.Key);
          var newIndexFound = reIndexAttributes.TryGetValue(sourceAttribute.AttributeID, out newIndex);
          attributes.Add(new AttributeInfo()
          {
            AttributeCode = "g_" + sourceAttribute.AttributeID,
            IsConfigurable = true,
            UsedForPromoRules = true,
            Name = sourceAttribute.Name,
            KeyFeature = true,
            Mandantory = false,
            IsSearchable = true,
            IsMultiValued = true,
            ConfigurableAttributeType = false,
            MagentoAttributeCode = "g_" + sourceAttribute.AttributeID,
            Values = groupedAttribute.Value,
            Position = newIndexFound ? newIndex : sourceAttribute.Position


          });
        }
      }
      Logger.DebugFormat("Found {0} distinct attributes", attributes.Count());

      ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = 8 };
      int totalRecords = attributes.Count;
      int totalProcessed = 0;

      Parallel.ForEach(Partitioner.Create(0, totalRecords), options, (range, loopState) =>
      {

        using (var helper = new MagentoMySqlHelper(Connector.Connection))
        {

          var currentAttributes = helper.GetAttributeList(PRODUCT_ENTITY_TYPE_ID);

          List<eav_attribute_label> currentLabels = helper.GetAttributeLabels();

          for (int index = range.Item1; index < range.Item2; index++)
          {
            var a = attributes[index];

            if (AttributeValueGroups.Any(c => c.Key == a.AttributeID))
              a.IsSearchable = false;

            eav_attribute attribute = null;
            currentAttributes.TryGetValue(a.MagentoAttributeCode, out attribute);

            if (attribute == null && !IsPrimaryLanguage)
              continue;

            if (IsPrimaryLanguage)
            {
              if (attribute == null)
              {
                attribute = helper.CreateAttribute(a.MagentoAttributeCode, PRODUCT_ENTITY_TYPE_ID, a.Name, datatype: "varchar");
              }

              attribute.is_configurable = a.IsConfigurable;
              attribute.is_used_for_promo_rules = a.UsedForPromoRules;

              if (a.IsSearchable || (a.IsConfigurable && !notConfigurableAttributesOverrides.Contains(a.AttributeID)))
              {
                attribute.frontend_input = "select";
                attribute.backend_type = "int";
                attribute.source_model = "eav/entity_attribute_source_table";
              }
              else
              {
                attribute.source_model = null;
                attribute.frontend_input = "text";
                attribute.backend_type = "varchar";
              }

              if (a.IsMultiValued)
              {
                attribute.backend_model = "eav/entity_attribute_backend_array";
                attribute.backend_type = "varchar";
                attribute.frontend_input = "multiselect";

              }


              attribute.frontend_label = a.Name;
              attribute.is_visible = true;

              if (!a.KeyFeature && a.Mandantory)
                attribute.is_visible = false;


              attribute.position = a.Position;
              attribute.is_comparable = attribute.is_filterable = a.IsSearchable;
              helper.SyncAttribute(attribute, configurableAttributeType: a.ConfigurableAttributeType);

            }



            var label = currentLabels.SingleOrDefault(x => x.attribute_id == attribute.attribute_id && x.store_id == StoreList[CurrentStoreCode].store_id);

            if (label == null)
            {
              label = new eav_attribute_label() { store_id = StoreList[CurrentStoreCode].store_id, attribute_id = attribute.attribute_id, value = a.Name };
            }
            helper.SyncAttributeLabel(label);


            Interlocked.Increment(ref totalProcessed);
            if (totalProcessed % 250 == 0)
              Logger.DebugFormat(String.Format("Processed {0} of {1} records", totalProcessed, totalRecords));
          }
        }

      });

      Logger.DebugFormat("Synchronizing Attribute Definitions");
    }

    private static string FormatMagentoAttributeCode(XElement l, bool asConfigurable = false)
    {
      string val = l.Attribute("AttributeID").Value.ToLower();
      bool isConfigurable = l.Attribute("IsConfigurable") != null && Convert.ToBoolean(l.Attribute("IsConfigurable").Value);

      if (isConfigurable || asConfigurable)
        return FormatConfigurableAttributeCode(val);
      else
        return FormatAttributeCode(val);

    }

    private static string FormatConfigurableMagentoAttributeCode(AssortmentConfigurableAttribute l)
    {
      string val = l.AttributeID.ToString().ToLower();

      return FormatConfigurableAttributeCode(val);
    }

    private static string FormatAttributeCode(string attr)
    {
      return "ice_" + attr;
    }

    private static string FormatConfigurableAttributeCode(string attr)
    {
      return "ca_" + attr;
    }

    private static eav_attribute GetAttribute(MySql.Data.MySqlClient.MySqlCommand selectAttributeCommand, eav_attribute attribute)
    {
      using (var reader = selectAttributeCommand.ExecuteReader())
      {
        if (reader.Read())
        {
          attribute = new eav_attribute()
          {
            attribute_id = reader.GetInt32("attribute_id"),
            attribute_code = reader.SafeGetString("attribute_code"),
            entity_type_id = reader.GetInt16("entity_type_id"),
            default_value = reader.SafeGetString("default_value"),
            note = reader.SafeGetString("note"),
            backend_type = reader.SafeGetString("backend_type"),
            frontend_input = reader.SafeGetString("frontend_input"),
            frontend_label = reader.SafeGetString("frontend_label"),
            is_required = reader.GetBoolean("is_required"),
            is_user_defined = reader.GetBoolean("is_user_defined")
          };
        }
      }
      return attribute;
    }

    private void SyncAttributeSets()
    {
      Logger.DebugFormat("Synchronizing Attribute Sets");

      var attributeProductList = AttributesXml.Root.Elements("ProductAttribute").ToList();

      using (var helper = new AssortmentHelper(Connector.Connection, Version))
      {
        var defaultAttributeSetSynchronization = Connector.ConnectorSettings.GetValueByKey("MagentoDefaultAttributeSetSynchronization", false);

        var defaultAttributeSet = defaultAttributeSetSynchronization
        ? helper.GetAttributeSet("Default", entity_type_id: PRODUCT_ENTITY_TYPE_ID)
        : null;

        foreach (var mapping in ProductsPerAttributeSet)
        {
          var attributeGroupSource = (from p in attributeProductList
                                      let ag = p.Element("AttributeGroups")
                                      where mapping.Value.Contains(Convert.ToInt32(p.Attribute("ProductID").Value))
                                      && ag != null
                                      select ag.Elements("AttributeGroup")).SelectMany(x => x)
                               .GroupBy(e => e.Attribute("Name").Value).Select(grp => grp.First())
                               .OrderBy(x => x.Attribute("AttributeGroupIndex").Value);

          var attributeGroups = (from ag in attributeGroupSource
                                 select new
                                 {
                                   AttributeGroupID = Convert.ToInt32(ag.Attribute("AttributeGroupID").Value),
                                   AttributeGroupIndex = Convert.ToInt32(ag.Attribute("AttributeGroupIndex").Value),
                                   Name = ag.Attribute("Name").Value
                                 }).ToList();



          eav_attribute_set attributeset = helper.GetAttributeSet(mapping.Key);
          if (attributeset == null)
          {
            #region New Attribute Set

            attributeset = helper.CreateAttributeSet(entity_type_id: PRODUCT_ENTITY_TYPE_ID, name: mapping.Key);
            helper.SyncAttributeSet(attributeset);

            helper.CopyDefaultSet(attributeset.attribute_set_id);
            #endregion
          }
          else if (defaultAttributeSetSynchronization && defaultAttributeSet != null)
          {
            helper.SyncMissingEntityAttribute(PRODUCT_ENTITY_TYPE_ID, defaultAttributeSet.attribute_set_id, attributeset.attribute_set_id);
          }

          #region Update Concentrator Groups

          var currentGroups = helper.GetAttributeGroupList(attributeset);

          foreach (var concentratorGroup in attributeGroups)
          {

            eav_attribute_group group = null;
            if (!currentGroups.TryGetValue(concentratorGroup.Name, out group))
            {
              group = helper.CreateAttributeGroup(attributeset, concentratorGroup.Name);
            }

            helper.SyncAttributeGroup(group);

          }

          #endregion
        }

        if (!string.IsNullOrEmpty(_nonAssortmentItemsSetName)) // we need to create additionally the attribute set
        {
          var nonAssortmentItemsSet = helper.GetAttributeSet(_nonAssortmentItemsSetName);

          if (nonAssortmentItemsSet == null)
          {
            nonAssortmentItemsSet = helper.CreateAttributeSet(entity_type_id: PRODUCT_ENTITY_TYPE_ID, name: _nonAssortmentItemsSetName);
            helper.SyncAttributeSet(nonAssortmentItemsSet);

            helper.CopyDefaultSet(nonAssortmentItemsSet.attribute_set_id);
          }
        }
      }
    }

    public enum AddressType
    {
      Default = 0,
      AdditionalShipTo = 1,
      ParentBillTo = 2,
      Custom = 3
    }

    public class ProductGroupInfo
    {
      public int ProductGroupID { get; set; }
      public int MappingID { get; set; }
      public string Name { get; set; }
      public int Index { get; set; }
      public bool IsRoot { get; set; }
      public bool IsAnchor { get; set; }
      public string PageLayout { get; set; }
      public bool UseParentLayout { get; set; }
      public bool ShowInMenu { get; set; }
      public bool Enabled { get; set; }
      public string Description { get; set; }
      public string Image { get; set; }
      public string ThumbnailImage { get; set; }
      public List<ProductGroupInfo> Children { get; private set; }

      public ProductGroupInfo()
      {
        ShowInMenu = true;
        IsRoot = false;
        Children = new List<ProductGroupInfo>();
      }

      public override int GetHashCode()
      {
        return MappingID.GetHashCode();
      }

      public override bool Equals(object obj)
      {
        ProductGroupInfo info = obj as ProductGroupInfo;
        if (info == null)
          return false;

        return info.MappingID.Equals(MappingID);
      }
    }

    private void ExportCategories()
    {
      Logger.DebugFormat("Exporting categories for language '{0}'", CurrentLanguage.Language.Name);

      #region Process Xml Data
      var hierarchies = (from pg in Assortment
                         let hg = pg.ProductGroups
                         where hg != null

                         select hg).SelectMany(x => x);

      Dictionary<string, ProductGroupInfo> categories = new Dictionary<string, ProductGroupInfo>();


      foreach (var h in hierarchies)
      {
        BuildCategoryTree(h, categories, firstLevel: false);
      }

      #endregion

      var roots = categories.Where(x => x.Value.IsRoot);

      Logger.DebugFormat("Found {0} Root Categories", roots.Count());

      var helper = new AssortmentHelper(Connector.Connection, Version);

      var websiteRoots = helper.GetRootCategories();

      var currentRoot = websiteRoots.Where(x => x.store_id == StoreList[CurrentStoreCode].store_id).First();

      var defaultAttributeSet = helper.GetAttributeSet(default_set: true, entity_type_id: CATEGORY_ENTITY_TYPE_ID);
      var attributeList = helper.GetAttributeList(CATEGORY_ENTITY_TYPE_ID);

      var currentCategories = helper.GetCategories();

      var ftpInfo = SftpHelper.GetFtpInfo(Connector);

      var client = SftpHelper.GetSFTPClient(ftpInfo);

      SyncCategory(currentRoot, roots.Select(x => x.Value).ToList(), helper, defaultAttributeSet.attribute_set_id, attributeList, currentCategories, client);

      helper.UpdateCategoryChildCount();
    }

    private void SyncCategory(catalog_category_entity parent,
 IEnumerable<ProductGroupInfo> categories, AssortmentHelper helper, int attribute_set_id,
      SortedDictionary<string, eav_attribute> attributeList, List<catalog_category_entity> currentCategories,
      TElSimpleSFTPClient client
      )
    {
      if (parent.level >= 6)
        return;

      foreach (var cat in categories)
      {
        if (parent.level <= 1)
          Logger.DebugFormat("Processing '{0}'", cat.Name);

        var key = StoreList[CurrentStoreCode].website_id + "_" + cat.MappingID.ToString();

        if (!(parent.level <= 1))
        {
          key = parent.icecat_value + "-" + cat.MappingID.ToString();
        }

        int storeid = 0;

        if (MultiLanguage && !IsPrimaryLanguage)
          storeid = StoreList[CurrentStoreCode].store_id;

        var entity = currentCategories.SingleOrDefault(x => x.parent_id == parent.entity_id && x.icecat_value == key && x.store_id == 0); //add current store here

        if (entity == null)
        {
          entity = new catalog_category_entity()
          {
            parent_id = parent.entity_id,
            entity_type_id = CATEGORY_ENTITY_TYPE_ID,
            path = parent.path + "/",
            created_at = DateTime.Now,
            attribute_set_id = attribute_set_id,
            children_count = 0,
            icecat_value = key,
            name = cat.Name,
            level = parent.level + 1,
            position = 10000 - cat.Index
          };
        }
        entity.position = 10000 - cat.Index;
        entity.updated_at = DateTime.Now;
        helper.SyncCategory(entity);


        int store_id = (MultiLanguage && !IsPrimaryLanguage) ? StoreList[CurrentStoreCode].store_id : 0;
        int store_id_actual = StoreList[CurrentStoreCode].store_id;


        helper.SyncAttributeValue(attributeList["name"].attribute_id, CATEGORY_ENTITY_TYPE_ID, store_id_actual, entity.entity_id, cat.Name);

        if (IsPrimaryLanguage)
        {
          helper.SyncAttributeValue(attributeList["name"].attribute_id, CATEGORY_ENTITY_TYPE_ID, 0, entity.entity_id, cat.Name);
        }

        if (cat.Description != null)
        {
          if (cat.Description == string.Empty)
          {
            helper.DeleteAttributeValue(attributeList["description"].attribute_id, CATEGORY_ENTITY_TYPE_ID, store_id_actual, entity.entity_id, type: "text");

            if (IsPrimaryLanguage)
              helper.DeleteAttributeValue(attributeList["description"].attribute_id, CATEGORY_ENTITY_TYPE_ID, 0, entity.entity_id, type: "text");
          }
          else
          {
            helper.SyncAttributeValue(attributeList["description"].attribute_id, CATEGORY_ENTITY_TYPE_ID, store_id_actual, entity.entity_id, cat.Description, type: "text");

            if (IsPrimaryLanguage)
              helper.SyncAttributeValue(attributeList["description"].attribute_id, CATEGORY_ENTITY_TYPE_ID, 0, entity.entity_id, cat.Description, type: "text");
          }
        }

        helper.SyncAttributeValue(attributeList["icecat_cat_id"].attribute_id, CATEGORY_ENTITY_TYPE_ID, 0, entity.entity_id, key);
        helper.SyncAttributeValue(attributeList["is_anchor"].attribute_id, CATEGORY_ENTITY_TYPE_ID, 0, entity.entity_id, cat.IsAnchor ? 1 : 0, "int");

        if (IsPrimaryLanguage)
        {
          helper.SyncAttributeValue(attributeList["is_active"].attribute_id, CATEGORY_ENTITY_TYPE_ID, 0, entity.entity_id, cat.Enabled, "int");
          helper.SyncAttributeValue(attributeList["include_in_menu"].attribute_id, CATEGORY_ENTITY_TYPE_ID, 0, entity.entity_id, cat.ShowInMenu ? 1 : 0, "int");
        }
        helper.SyncAttributeValue(attributeList["is_active"].attribute_id, CATEGORY_ENTITY_TYPE_ID, store_id_actual, entity.entity_id, cat.Enabled, "int");
        helper.SyncAttributeValue(attributeList["include_in_menu"].attribute_id, CATEGORY_ENTITY_TYPE_ID, store_id_actual, entity.entity_id, cat.ShowInMenu ? 1 : 0, "int");

        if (String.IsNullOrEmpty(cat.PageLayout))
        {
          helper.DeleteAttributeValue(attributeList["page_layout"].attribute_id, CATEGORY_ENTITY_TYPE_ID, store_id_actual, entity.entity_id, "varchar");
          if (IsPrimaryLanguage)
          {
            helper.DeleteAttributeValue(attributeList["page_layout"].attribute_id, CATEGORY_ENTITY_TYPE_ID, 0, entity.entity_id, "varchar");
          }
        }
        else
        {
          helper.SyncAttributeValue(attributeList["page_layout"].attribute_id, CATEGORY_ENTITY_TYPE_ID, store_id_actual, entity.entity_id, cat.PageLayout, "varchar");
          if (IsPrimaryLanguage)
          {
            helper.SyncAttributeValue(attributeList["page_layout"].attribute_id, CATEGORY_ENTITY_TYPE_ID, 0, entity.entity_id, cat.PageLayout, "varchar");
          }
        }


        if (attributeList.ContainsKey("custom_use_parent_settings"))
        {
          helper.SyncAttributeValue(attributeList["custom_use_parent_settings"].attribute_id, CATEGORY_ENTITY_TYPE_ID, 0, entity.entity_id, cat.UseParentLayout ? 1 : 0, "int");
        }


        if (cat.Image != null)
        {


          if (cat.Image == String.Empty)
            helper.DeleteAttributeValue(attributeList["image"].attribute_id, CATEGORY_ENTITY_TYPE_ID, store_id, entity.entity_id, type: "varchar");
          else
          {
            var ftpInfo = SftpHelper.GetFtpInfo(Connector);
            var basePath = ftpInfo.FtpPath.Replace("/catalog/product", "/catalog/category");
            if (!basePath.EndsWith("/"))
              basePath += "/";

            Uri source = new Uri(cat.Image);

            string fileName = source.Segments.Last();

            //string exportFilePath = String.Format(@"{0}\{1}\{2}", fileName.Substring(0, 1),
            //                                fileName.Substring(1, 1),
            //                                fileName);
            //string serverPath = String.Format(@"/{0}/{1}/{2}", fileName.Substring(0, 1),
            //                                   fileName.Substring(1, 1),
            //                                   fileName);

            //string magentoPath = String.Format(@"\{0}\{1}", fileName.Substring(0, 1),
            //                                 fileName.Substring(1, 1));

            fileName = HttpUtility.UrlDecode(fileName);
            DownloadImage(new Uri(cat.Image), client, basePath + fileName, Logger);
            helper.SyncAttributeValue(attributeList["image"].attribute_id, CATEGORY_ENTITY_TYPE_ID, store_id_actual, entity.entity_id, fileName, type: "varchar");
            if (IsPrimaryLanguage)
              helper.SyncAttributeValue(attributeList["image"].attribute_id, CATEGORY_ENTITY_TYPE_ID, 0, entity.entity_id, fileName, type: "varchar");
          }
        }
        else
        {
          helper.DeleteAttributeValue(attributeList["image"].attribute_id, CATEGORY_ENTITY_TYPE_ID, store_id, entity.entity_id, type: "varchar");
        }

        if (cat.ThumbnailImage != null)
        {


          if (cat.Image == String.Empty)
            helper.DeleteAttributeValue(attributeList["thumbnail"].attribute_id, CATEGORY_ENTITY_TYPE_ID, store_id, entity.entity_id, type: "varchar");
          else
          {
            var ftpInfo = SftpHelper.GetFtpInfo(Connector);
            var basePath = ftpInfo.FtpPath.Replace("/catalog/product", "/catalog/category");
            if (!basePath.EndsWith("/"))
              basePath += "/";

            Uri source = new Uri(cat.ThumbnailImage);

            string fileName = source.Segments.Last();

            fileName = HttpUtility.UrlDecode(fileName);
            DownloadImage(new Uri(cat.ThumbnailImage), client, basePath + fileName, Logger);
            helper.SyncAttributeValue(attributeList["thumbnail"].attribute_id, CATEGORY_ENTITY_TYPE_ID, store_id_actual, entity.entity_id, fileName, type: "varchar");
            if (IsPrimaryLanguage)
              helper.SyncAttributeValue(attributeList["thumbnail"].attribute_id, CATEGORY_ENTITY_TYPE_ID, 0, entity.entity_id, fileName, type: "varchar");
          }
        }
        else
        {
          helper.DeleteAttributeValue(attributeList["thumbnail"].attribute_id, CATEGORY_ENTITY_TYPE_ID, store_id, entity.entity_id, type: "varchar");
        }

        if (cat.Children.Count > 0)
        {
          if (!String.IsNullOrEmpty(cat.PageLayout))
            cat.Children.ForEach(c => c.UseParentLayout = true);

          SyncCategory(entity, cat.Children, helper, attribute_set_id, attributeList, currentCategories, client);
        }


      }

    }

    private ProductGroupInfo BuildCategoryTree(AssortmentProductGroup group, Dictionary<string, ProductGroupInfo> result, bool firstLevel = false)
    {
      int id = group.ID;
      int mappingID = group.MappingID;
      string mappingKey = mappingID.ToString();
      var parentGroup = group.ParentProductGroup;
      string name = group.Name;
      int index = group.Index;
      bool isAnchor = false;
      bool hiddenInMenu = false;
      bool enabled = true;
      string pageLayout = null;

      var magentoSetting = group.MagentoSetting;
      if (magentoSetting != null)
      {

        if (magentoSetting.IsAnchor.HasValue)
          isAnchor = magentoSetting.IsAnchor.Value;

        if (!string.IsNullOrEmpty(magentoSetting.PageLayoutCode))
          pageLayout = magentoSetting.PageLayoutCode;

        if (magentoSetting.DisableMenu.HasValue)
          enabled = !magentoSetting.DisableMenu.Value;

        if (magentoSetting.HideInMenu.HasValue)
          hiddenInMenu = magentoSetting.HideInMenu.Value;
      }

      ProductGroupInfo parent = null;
      if (parentGroup != null)
      {
        parent = BuildCategoryTree(parentGroup, result);

        int parentGroupMappingID = parentGroup.MappingID;

        mappingKey = parentGroupMappingID.ToString() + "-" + mappingID.ToString();
      }

      string description = (!String.IsNullOrEmpty(group.Description)) ? group.Description : null;

      string image = (!String.IsNullOrEmpty(group.Image)) ? group.Image : null;

      string thumbnailImage = (!String.IsNullOrEmpty(group.ThumbnailImage)) ? group.ThumbnailImage : null;

      ProductGroupInfo info = null;
      if (!result.TryGetValue(mappingKey, out info))
      {
        info = new ProductGroupInfo()
        {
          Index = index,
          Name = name,
          MappingID = mappingID,
          ProductGroupID = id,
          IsAnchor = (isAnchor || firstLevel),
          PageLayout = pageLayout,
          ShowInMenu = (CategoriesWithPageLayoutShouldNotBeHidden ? !hiddenInMenu : ((String.IsNullOrEmpty(pageLayout)) && (!hiddenInMenu))), //WTF? Will be resolved
          Enabled = enabled,
          Description = description,
          Image = image,
          ThumbnailImage = thumbnailImage
        };
        result.Add(mappingKey, info);
      }


      if (parentGroup == null)
      {
        info.IsRoot = true;
      }
      else
      {
        if (!parent.Children.Contains(info))
        {
          parent.Children.Add(info);
        }
      }

      return info;

    }

    private string GetCategories(IEnumerable<XElement> hierarchy, int level = 0)
    {
      List<string> result = new List<string>();
      foreach (var pg in hierarchy)
      {
        string name = pg.Attribute("Name").Value.Trim();

        string entry = String.Format("{0}::{1}::{2}::{3}", name, 1, 1, 1);

        var parent = pg.Elements("ProductGroup");
        if (parent != null && parent.Count() > 0)
          result.Add(GetCategories(parent, level++));
        //if( pg.Element("ProductGroup
        result.Add(entry);

      }

      if (level > 0)
        return String.Join("/", result);
      else
        return String.Join(";;", result);
    }
  }
}
