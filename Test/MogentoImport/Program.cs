using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Spider.Objects;
using System.IO;
using System.Drawing;
using System.Xml.Linq;
using System.Configuration;
using System.Globalization;
using System.Xml;
using MySql.Data.MySqlClient;
//using System.Threading.Tasks;
using MagentoImportService;

namespace MagentoImport
{

  class Program
  {

   // private static string connectionString = "server=jupiter;User Id=magento_demo;password=u5KeCDbU72hsryTW;database=magento_demo;Connect Timeout=30000;Default Command Timeout=30000;port=3310";
    //private static XDocument products;

    static void Main(string[] args)
    {
      MagentoImportService.MagentoImportService.Start();
      Console.ReadLine();
      MagentoImportService.MagentoImportService.Stop();
      //ImportProduct();

    }

    //private static int CATEGORY_ENTITY_TYPE_ID = 3;
    //private static int PRODUCT_ENTITY_TYPE_ID = 4;
    //private static int WEBSITE_ID = 1;

    //private static void ImportProduct()
    //{
    //  using (MagentoWebEntities ctx = new MagentoWebEntities())
    //  {

    //    using (DBImportDataContext dbctx = new DBImportDataContext())
    //    {
    //      ConcentratorContentService.SpiderServiceSoapClient soap =
    //   new MagentoImport.ConcentratorContentService.SpiderServiceSoapClient();

    //      if (ConfigurationManager.AppSettings["ImportCommercialText"] != null)
    //      {
    //        if (bool.Parse(ConfigurationManager.AppSettings["ImportCommercialText"].ToString()))
    //          products = XDocument.Parse(soap.GetAssortmentSingleProductGroup(int.Parse(ConfigurationManager.AppSettings["ConnectorID"].ToString()), true, true, false).OuterXml);
    //        else
    //          products = XDocument.Parse(soap.GetAssortmentSingleProductGroup(int.Parse(ConfigurationManager.AppSettings["ConnectorID"].ToString()), false, true, false).OuterXml);
    //      }
    //      else
    //        products = XDocument.Parse(soap.GetAssortmentSingleProductGroup(int.Parse(ConfigurationManager.AppSettings["ConnectorID"].ToString()), false, true, false).OuterXml);

    //      var parids = (from r in products.Root.Elements("Product").Elements("ProductGroups").Elements("ProductGroup")
    //                    select new
    //                    {
    //                      productgroupid = r.Attribute("ProductGroupID").Value.Replace("\"", string.Empty).Trim(),
    //                      code = r.Element("Code").Value.Replace("\"", string.Empty).Trim(),
    //                      name = r.Element("Name").Value.Replace("\"", string.Empty).Trim(),
    //                      score = r.Attribute("Index").Value.Replace("\"", string.Empty).Trim()
    //                    }
    //               ).Distinct().ToArray();

    //      var subids = (from r in products.Root.Elements("Product").Elements("ProductGroups").Elements("SubProductGroup")
    //                    select new
    //                    {
    //                      subgroupid = r.Attribute("SubProductGroupID").Value.Replace("\"", string.Empty).Trim(),
    //                      productgroupid = r.Attribute("ProductGroupID").Value.Replace("\"", string.Empty).Trim(),
    //                      code = r.Element("Code").Value.Replace("\"", string.Empty).Trim(),
    //                      name = r.Element("Name").Value.Replace("\"", string.Empty).Trim(),
    //                      score = r.Attribute("Index").Value.Replace("\"", string.Empty).Trim()
    //                    }
    //                ).Distinct().ToArray();

    //      var icecatAttribute = GetAttribute(ctx, CATEGORY_ENTITY_TYPE_ID, "icecat_cat_id");

    //      var nameAttribute = GetAttribute(ctx, CATEGORY_ENTITY_TYPE_ID, "name");

    //      var is_activeAttribute = GetAttribute(ctx, CATEGORY_ENTITY_TYPE_ID, "is_active");

    //      #region categories

    //      foreach (var cat in parids)
    //      {
    //        int productGroupID = int.Parse(cat.productgroupid);

    //        catalog_category_entity_int parentCategoryAttribute =
    //        ctx.catalog_category_entity_int.FirstOrDefault(x => x.value == productGroupID
    //                && x.entity_type_id == CATEGORY_ENTITY_TYPE_ID
    //                && x.eav_attribute.attribute_id == icecatAttribute.attribute_id
    //          );

    //        catalog_category_entity parentCategory = (from x in ctx.catalog_category_entity_int
    //                                                  where x.value == productGroupID
    //                                                        && x.entity_type_id == CATEGORY_ENTITY_TYPE_ID
    //                                                        && x.eav_attribute.attribute_id == icecatAttribute.attribute_id
    //                                                  select x.catalog_category_entity).FirstOrDefault();

    //        #region Parent Category
    //        if (parentCategory == null)
    //        {
    //          parentCategory = new catalog_category_entity();
    //          parentCategory.entity_type_id = 3; // category
    //          parentCategory.attribute_set_id = 3; //default category attribute set
    //          parentCategory.parent_id = 2;
    //          parentCategory.position = 1;
    //          parentCategory.level = 2;
    //          parentCategory.path = "1/2/";
    //          parentCategory.children_count = 0;

    //          ctx.AddTocatalog_category_entity(parentCategory);
    //          ctx.SaveChanges();

    //          parentCategory.path = String.Format("1/2/{0}", parentCategory.entity_id);

    //          var newNameAttribute = new catalog_category_entity_varchar();
    //          newNameAttribute.store_id = 0;
    //          newNameAttribute.eav_attribute = nameAttribute;
    //          newNameAttribute.catalog_category_entity = parentCategory;
    //          newNameAttribute.value = cat.name;
    //          newNameAttribute.entity_type_id = CATEGORY_ENTITY_TYPE_ID; // category

    //          ctx.AddTocatalog_category_entity_varchar(newNameAttribute);

    //          parentCategoryAttribute = new catalog_category_entity_int();
    //          parentCategoryAttribute.store_id = 0;
    //          parentCategoryAttribute.entity_type_id = CATEGORY_ENTITY_TYPE_ID; // category
    //          parentCategoryAttribute.eav_attribute = icecatAttribute;
    //          parentCategoryAttribute.catalog_category_entity = parentCategory;
    //          parentCategoryAttribute.value = int.Parse(cat.productgroupid);


    //          ctx.AddTocatalog_category_entity_int(parentCategoryAttribute);
    //          ctx.SaveChanges();

    //        }

    //        #endregion

    //        var subproductgroups = (from spg in subids
    //                                where spg.productgroupid == cat.productgroupid
    //                                select spg).ToList();

    //        #region Category
    //        foreach (var sub in subproductgroups)
    //        {
    //          int subGroupID = int.Parse(sub.subgroupid);

    //          catalog_category_entity_int categoryAttribute =
    //            ctx.catalog_category_entity_int.FirstOrDefault(x => x.value == subGroupID
    //                    && x.entity_type_id == CATEGORY_ENTITY_TYPE_ID
    //                    && x.eav_attribute.attribute_id == icecatAttribute.attribute_id
    //              );

    //          catalog_category_entity category = (from x in ctx.catalog_category_entity_int
    //                                              where x.value == subGroupID
    //                                                    && x.entity_type_id == CATEGORY_ENTITY_TYPE_ID
    //                                                    && x.eav_attribute.attribute_id == icecatAttribute.attribute_id
    //                                              select x.catalog_category_entity).FirstOrDefault();




    //          if (category == null)
    //          {
    //            category = new catalog_category_entity();
    //            category.entity_type_id = 3; // category
    //            category.attribute_set_id = 3; //default category attribute set
    //            category.parent_id = parentCategory.entity_id;
    //            category.position = 1;
    //            category.level = 3;
    //            category.path = "1/2/";
    //            category.children_count = 0;
    //            ctx.AddTocatalog_category_entity(category);
    //            ctx.SaveChanges();

    //            category.path = String.Format("1/2/{0}/{1}", parentCategory.entity_id, category.entity_id);

    //            var newNameAttribute = new catalog_category_entity_varchar();
    //            newNameAttribute.store_id = STORE_ID_UNSPECIFIED;
    //            newNameAttribute.eav_attribute = nameAttribute;
    //            newNameAttribute.catalog_category_entity = category;
    //            newNameAttribute.value = sub.name;
    //            newNameAttribute.entity_type_id = CATEGORY_ENTITY_TYPE_ID; // category

    //            ctx.AddTocatalog_category_entity_varchar(newNameAttribute);

    //            categoryAttribute = new catalog_category_entity_int();
    //            categoryAttribute.store_id = STORE_ID_UNSPECIFIED;
    //            categoryAttribute.entity_type_id = CATEGORY_ENTITY_TYPE_ID; // category
    //            categoryAttribute.eav_attribute = icecatAttribute;
    //            categoryAttribute.catalog_category_entity = category;

    //            categoryAttribute.value = int.Parse(sub.subgroupid);


    //            ctx.AddTocatalog_category_entity_int(categoryAttribute);
    //            ctx.SaveChanges();

    //          }
    //          else
    //          {

    //            var catNameAttribute = (from a in ctx.catalog_category_entity_varchar
    //                                    where a.entity_type_id == CATEGORY_ENTITY_TYPE_ID
    //                                          && a.catalog_category_entity.entity_id == category.entity_id
    //                                          && a.eav_attribute.attribute_id == nameAttribute.attribute_id
    //                                    select a
    //                                   ).FirstOrDefault();


    //            if (catNameAttribute == null)
    //            {
    //              catNameAttribute = new catalog_category_entity_varchar()
    //                                   {
    //                                     catalog_category_entity = category,
    //                                     entity_type_id = CATEGORY_ENTITY_TYPE_ID,
    //                                     store_id = STORE_ID_UNSPECIFIED,
    //                                     eav_attribute = nameAttribute,
    //                                   };
    //              ctx.AddTocatalog_category_entity_varchar(catNameAttribute);

    //            }

    //            catNameAttribute.value = sub.name;


    //            var catActiveAttribute = (from a in ctx.catalog_category_entity_int

    //                                      where a.entity_type_id == CATEGORY_ENTITY_TYPE_ID
    //                                            && a.catalog_category_entity.entity_id == category.entity_id
    //                                            && a.eav_attribute.attribute_id == is_activeAttribute.attribute_id
    //                                      select a
    //                             ).FirstOrDefault();

    //            if (catActiveAttribute == null)
    //            {
    //              catActiveAttribute = new catalog_category_entity_int();
    //              catActiveAttribute.store_id = 0;
    //              catActiveAttribute.entity_type_id = CATEGORY_ENTITY_TYPE_ID; // category
    //              catActiveAttribute.eav_attribute = is_activeAttribute;
    //              catActiveAttribute.catalog_category_entity = category;

    //              ctx.AddTocatalog_category_entity_int(catActiveAttribute);
    //            }


    //        #endregion

    //            ctx.SaveChanges();

    //          }
    //        }


    //      }

    //      #endregion

    //      #region product
    //      eav_attribute productNameAttribute = GetAttribute(ctx, PRODUCT_ENTITY_TYPE_ID, "name");
    //      eav_attribute productDescriptionAttribute = GetAttribute(ctx, PRODUCT_ENTITY_TYPE_ID, "description");
    //      eav_attribute productShortDescriptionAttribute = GetAttribute(ctx, PRODUCT_ENTITY_TYPE_ID, "short_description");
    //      eav_attribute taxClassAttribute = GetAttribute(ctx, PRODUCT_ENTITY_TYPE_ID, "tax_class_id");
    //      eav_attribute productWeightAttribute = GetAttribute(ctx, PRODUCT_ENTITY_TYPE_ID, "weight");

    //      eav_attribute productStatusAttribute = GetAttribute(ctx, PRODUCT_ENTITY_TYPE_ID, "status");
    //      eav_attribute productVisibilityAttribute = GetAttribute(ctx, PRODUCT_ENTITY_TYPE_ID, "visibility");
    //      eav_attribute productPriceAttribute = GetAttribute(ctx, PRODUCT_ENTITY_TYPE_ID, "price");
    //      eav_attribute manufacturerAttribute = GetAttribute(ctx, PRODUCT_ENTITY_TYPE_ID, "manufacturer");

    //      eav_attribute mediaGalleryAttribute = GetAttribute(ctx, PRODUCT_ENTITY_TYPE_ID, "media_gallery");
    //      eav_attribute mediaGalleryImageAttribute = GetAttribute(ctx, PRODUCT_ENTITY_TYPE_ID, "image"); //70
    //      eav_attribute mediaGallerySmallImageAttribute = GetAttribute(ctx, PRODUCT_ENTITY_TYPE_ID, "small_image"); //71
    //      eav_attribute mediaGalleryThumbnailAttribute = GetAttribute(ctx, PRODUCT_ENTITY_TYPE_ID, "thumbnail"); //72

    //      decimal total = products.Root.Elements("Product").Count();
    //      int count = 0;

    //      //Parallel.ForEach(products.Root.Elements("Product"), (prod, state, igor) =>
    //      //                                                      {
              
    //      foreach (XElement prod in products.Root.Elements("Product"))
    //      {
    //        Console.WriteLine("{0} % completed", Decimal.Round((count / total) * 100, 1));
    //        string sku = prod.Attribute("ManufactureID").Value.Replace("\"", string.Empty).Trim();

    //        catalog_product_entity productEntity = (from p in ctx.catalog_product_entity
    //                                                where p.sku == sku
    //                                                select p).FirstOrDefault();
    //        eav_attribute_set DefaultSet = (from c in ctx.eav_attribute_set
    //                                        where c.attribute_set_id == 4
    //                                        select c).FirstOrDefault();

    //        if (productEntity == null)
    //        {
    //          productEntity = new catalog_product_entity();
    //          productEntity.sku = prod.Attribute("ManufactureID").Value.Replace("\"", string.Empty).Trim();
    //          productEntity.entity_type_id = PRODUCT_ENTITY_TYPE_ID;
    //          productEntity.type_id = "simple";
    //          productEntity.created_at = DateTime.Now;
    //          productEntity.has_options = 0;
    //          productEntity.required_options = false;
    //          productEntity.eav_attribute_set = DefaultSet;
    //          ctx.AddTocatalog_product_entity(productEntity);

    //        }
    //        ctx.SaveChanges();
    //        // category
    //        int subProductGroupID = 0;

    //        XElement subProductGroup = prod.Element("ProductGroups").Element("SubProductGroup");
    //        if (subProductGroup != null && !subProductGroup.IsEmpty)
    //        {

    //          subProductGroupID =
    //            int.Parse(
    //              prod.Element("ProductGroups").Element("SubProductGroup").Attribute("SubProductGroupID").Value.Replace(
    //                "\"", string.Empty).Trim());
    //        }
    //        ctx.SaveChanges();
    //        var productCategory = (from c in ctx.catalog_category_entity_int
    //                               where c.entity_type_id == CATEGORY_ENTITY_TYPE_ID
    //                                     && c.eav_attribute.attribute_id == icecatAttribute.attribute_id
    //                                     && c.value == subProductGroupID
    //                               select c.catalog_category_entity).FirstOrDefault();



    //        if (productCategory != null)
    //        {


    //          catalog_category_product ccprod = (from ccp in ctx.catalog_category_product
    //                                             where ccp.category_id == productCategory.entity_id
    //                                             && ccp.product_id == productEntity.entity_id
    //                                             select ccp).FirstOrDefault();
    //          if (ccprod == null)
    //          {

    //            using (MySqlConnection connection = new MySqlConnection(connectionString))
    //            {
    //              MySqlCommand command = connection.CreateCommand();

    //              //MySqlDataReader _reader;

    //              command.Parameters.Add(new MySqlParameter("@category_id", productCategory.entity_id));
    //              command.Parameters.Add(new MySqlParameter("@product_id", productEntity.entity_id));
    //              command.Parameters.Add(new MySqlParameter("@position", 1));

    //              command.CommandText = string.Format("Insert into `magento_demo`.`catalog_category_product` values (@category_id,@product_id,@position)");
    //              connection.Open();
    //              command.ExecuteScalar();
    //            }

    //          }
    //          //List<int> ccps = (from ccp in ctx.catalog_category_product
    //          //                  where ccp.category_id == productCategory.entity_id && ccp.product_id == productEntity.entity_id
    //          //                  select ccp.position).ToList();

    //          //int maxPos = 1;
    //          //if (ccps != null && ccps.Count > 0)
    //          //  maxPos = ccps.Max() + 1;


    //          //var categoryproduct = (from c in ctx.catalog_category_product
    //          //                       where c.category_id == productCategory.entity_id && c.product_id == productEntity.entity_id
    //          //                       select c).FirstOrDefault();

    //          //ctx.SaveChanges();
    //          //if (categoryproduct == null)
    //          //{

    //          //  categoryproduct = new catalog_category_product()
    //          //               {
    //          //                 catalog_category_entity = tmpCat,
    //          //                 catalog_product_entity = tmpProduct,
    //          //                 position = maxPos,

    //          //                };

    //          //  ctx.AddTocatalog_category_product(categoryproduct);
    //          //}
    //        }
    //        else
    //        {
    //          //insert category??
    //        }
    //        //productEntity.updated_at = DateTime.Now;

    //        //ctx.SaveChanges();

    //        // stock item
    //        //TODO:
    //        var stockItem = (from s in ctx.cataloginventory_stock_item
    //                         where s.catalog_product_entity.entity_id == productEntity.entity_id
    //                         select s).FirstOrDefault();

    //        if (stockItem == null)
    //        {
    //          stockItem = new cataloginventory_stock_item();
    //          stockItem.catalog_product_entity = productEntity;
    //          stockItem.use_config_backorders = true;
    //          stockItem.use_config_manage_stock = false;
    //          stockItem.use_config_max_sale_qty = true;
    //          stockItem.use_config_min_qty = true;
    //          stockItem.use_config_min_sale_qty = true;
    //          stockItem.stock_id = 1;
    //          ctx.AddTocataloginventory_stock_item(stockItem);

    //        }

    //        ctx.SaveChanges();



    //        //var productWebsite = (from pw in ctx.catalog_product_website
    //        //                      where pw.catalog_product_entity.entity_id == productEntity.entity_id
    //        //                            && pw.website_id == WEBSITE_ID
    //        //                      select pw).FirstOrDefault();

    //        //if (productWebsite == null)
    //        //{
    //        //  ctx.AddTocatalog_product_website(new catalog_product_website()
    //        //                                     {
    //        //                                       website_id = WEBSITE_ID,
    //        //                                       catalog_product_entity = productEntity
    //        //                                     });

    //        //  //productWebsite = new catalog_product_website();
    //        //  //productWebsite.website_id = WEBSITE_ID;
    //        //  //productWebsite.catalog_product_entity = productEntity;

    //        //  //ctx.AddTocatalog_product_website(productWebsite);
    //        //  ctx.SaveChanges();
    //        //}


    //        if (productCategory != null)
    //        {
    //          var categoryMapping = (from pm in ctx.catalog_category_product
    //                                 where pm.category_id == productCategory.entity_id
    //                                       && pm.product_id == productEntity.entity_id
    //                                 select pm).FirstOrDefault();

    //          if (categoryMapping == null)
    //          {

    //            categoryMapping = new catalog_category_product();

    //            categoryMapping.catalog_category_entity = productCategory;
    //            categoryMapping.catalog_product_entity = productEntity;
    //            categoryMapping.position = 1;
    //            ctx.AddTocatalog_category_product(categoryMapping);
    //          }
    //          else
    //          {
    //            categoryMapping.catalog_category_entity = productCategory;
    //          }


    //          ctx.SaveChanges();

    //          // index

    //          string[] catIDs = productCategory.path.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

    //          for (int recIndex = 0; recIndex < catIDs.Length; recIndex++)
    //          {

    //            int catID = Int32.Parse(catIDs[recIndex]);

    //            if (catID == 1)
    //              continue;

    //            catalog_category_entity additionalCat = (from c in ctx.catalog_category_entity
    //                                                     where c.entity_id == catID
    //                                                     select c).FirstOrDefault();


    //            catalog_category_product_index indexEntry = (from i in ctx.catalog_category_product_index
    //                                                         where i.category_id == catID
    //                                                               && i.catalog_product_entity.entity_id == productEntity.entity_id
    //                                                         select i).FirstOrDefault();
    //            //TODO: Fix index insertions
    //            if (indexEntry == null)
    //            {

    //              using (MySqlConnection connection = new MySqlConnection(connectionString))
    //              {
    //                connection.Open();
    //                string commandQ = "Insert into `magento_demo`.`catalog_category_product_index` values (@category_id,@product_id,1, @isParent, @store_id, 4)";
    //                MySqlCommand command = connection.CreateCommand();
    //                command.CommandText = commandQ;
    //                command.Parameters.Add("@category_id",catID);
    //                command.Parameters.Add("@product_id", productEntity.entity_id);
    //                command.Parameters.Add("@isParent", additionalCat == null ? true : additionalCat.level >= 1 && recIndex == catIDs.Length - 1);
    //                command.Parameters.Add("@store_id", STORE_ID);

    //                command.ExecuteNonQuery();

    //              }
    //            }
    //          }
    //        }

    //        GetOrCreateProductAttribute(ctx, productEntity, productNameAttribute, prod.Element("Content").Attribute("ShortDescription").Value);
    //        GetOrCreateProductAttribute(ctx, productEntity, productDescriptionAttribute, prod.Element("Content").Attribute("LongContentDescription").Value);
    //        GetOrCreateProductAttribute(ctx, productEntity, productShortDescriptionAttribute, prod.Element("Content").Attribute("ShortContentDescription").Value);

    //        GetOrCreateProductAttribute(ctx, productEntity, taxClassAttribute, 2);

    //        GetOrCreateProductAttribute(ctx, productEntity, productStatusAttribute, 1); // 1= enabled, 2=disabled

    //        GetOrCreateProductAttribute(ctx, productEntity, productVisibilityAttribute, 4);//prod.DESCRIPTION1.StartsWith("*") ? 3 : 4); //search&catalog

    //        GetOrCreateProductAttribute(ctx, productEntity, productWeightAttribute, 100);

    //        GetOrCreateProductAttribute(ctx, productEntity, productPriceAttribute, decimal.Parse(prod.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture) * ((decimal.Parse(prod.Element("Price").Attribute("TaxRate").Value.Replace("\"", string.Empty).Trim()) / 100) + 1));

    //        GetOrCreateProductAttribute(ctx, productEntity, manufacturerAttribute, prod.Element("Brand").Element("Name").Value.Replace("\"", string.Empty).Trim());

    //        ctx.SaveChanges();
    //        // ICECAT Product_ID => icecatProduct.Product_ID

    //        //var icecatAttributes = icectx.Concentrator_GetIceCatAttribute(prod.BKPRODUCTID, null, null);
    //        #region TODO: check services
    //        XDocument icecatAttributes = XDocument.Parse(
    //            soap.GetAttributesAssortment(int.Parse(ConfigurationManager.AppSettings["ConnectorID"].ToString()),
    //                                          prod.Attribute("CustomProductID").Value).OuterXml);

    //        var groups = icecatAttributes.Root.Elements("ProductAttribute").Elements("AttributeGroups");

    //        if (groups.Where(x => x.IsEmpty).ToList().Count == 0)
    //        {

    //          List<AttributeResult> list = (from a in icecatAttributes.Root.Elements("ProductAttribute").Elements("Attributes").Elements("Attribute")
    //                                        join g in groups on a.Attribute("AttributeGroupID").Value equals g.Element("AttributeGroup").Attribute("AttributeGroupID").Value
    //                                        select new AttributeResult
    //                                        {
    //                                          AttributeGroupID = int.Parse(a.Attribute("AttributeGroupID").Value.Replace("\"", string.Empty).Trim()),
    //                                          AttributeGroupIndex = int.Parse(g.Element("AttributeGroup").Attribute("AttributeGroupIndex").Value.Replace("\"", string.Empty).Trim()),
    //                                          AttributeGroupName = g.Element("AttributeGroup").Element("Name").Value.Replace("\"", string.Empty).Trim(),
    //                                          AttributeName = a.Element("Name").Value.Replace("\"", string.Empty).Trim(),
    //                                          Feature_ID = double.Parse(a.Attribute("AttributeID").Value.Replace("\"", string.Empty).Trim()),
    //                                          FeatureIndex = int.Parse(a.Attribute("Index").Value.Replace("\"", string.Empty).Trim()),
    //                                          IsSearchable = int.Parse(a.Attribute("IsSearchable").Value.Replace("\"", string.Empty).Trim()),
    //                                          Presentation_value = a.Element("Value").Value + " " + a.Element("Sign").Value,
    //                                          Product_ID = int.Parse(prod.Attribute("CustomProductID").Value),
    //                                          Sign = a.Element("Sign").Value,
    //                                          Value = a.Element("Value").Value
    //                                        }).ToList();

    //          foreach (var icAttribute in list)
    //          {
    //            GetOrCreateProductAttribute(ctx, productEntity, icAttribute);
    //          }
    //        }
    //        #region Images

    //        string exportPath = ConfigurationManager.AppSettings["ImageDir"];


    //        string brandCode = prod.Element("Brand").Element("Code").Value;


    //        XmlElement customImages = (XmlElement)soap.GetCustomImages(sku, int.Parse(ConfigurationManager.AppSettings["ConnectorID"]),
    //                                                     brandCode, prod.Attribute("CustomProductID").Value);


    //        if (customImages != null)
    //        {


    //          foreach (var image in icecatAttributes.Root.Elements("Products").Elements("ProuductImage"))
    //          {
    //            int position = int.Parse(image.Attribute("Sequence").Value) + 1;
    //            Uri u = new Uri(image.Value);
    //            string linuxPath = RetrieveFile(u, exportPath);
    //            UpdateMediaGalleryItem(ctx, productEntity, mediaGalleryAttribute, linuxPath, position);
    //            GetOrCreateProductAttribute(ctx, productEntity, mediaGalleryImageAttribute, linuxPath);
    //            GetOrCreateProductAttribute(ctx, productEntity, mediaGallerySmallImageAttribute, linuxPath);
    //            GetOrCreateProductAttribute(ctx, productEntity, mediaGalleryThumbnailAttribute, linuxPath);

    //            ctx.SaveChanges();
    //          }
    //        }
    //        #endregion
    //        //if (!String.IsNullOrEmpty(icecatProduct.LowPic))
    //        //{

    //        //  Uri u = new Uri(icecatProduct.LowPic);
    //        //  string linuxPath = RetrieveFile(u, exportPath);
    //        //  UpdateMediaGalleryItem(ctx, productEntity, mediaGalleryAttribute, linuxPath, mediaGallerySmallImageAttribute);
    //        //}

    //        //if (!String.IsNullOrEmpty(icecatProduct.HighPic))
    //        //{
    //        //  Uri u = new Uri(icecatProduct.ThumbPic);
    //        //  string linuxPath = RetrieveFile(u, exportPath);
    //        //  UpdateMediaGalleryItem(ctx, productEntity, mediaGalleryAttribute, linuxPath, mediaGalleryThumbnailAttribute);
    //        //}

    //        #endregion

    //        ctx.SaveChanges();
    //        count++;
    //      }
    //      //});
    //      #endregion

    //    }
    //  }
    //}

    //private static void UpdateMediaGalleryItem(MagentoWebEntities ctx, catalog_product_entity productEntity, eav_attribute mediaGalleryAttribute, string linuxPath, int position)
    //{
    //  var currentAttribute = (from c in ctx.catalog_product_entity_media_gallery
    //                          where c.catalog_product_entity.entity_id == productEntity.entity_id
    //                                && c.eav_attribute.attribute_id == mediaGalleryAttribute.attribute_id
    //                                && c.value == linuxPath
    //                          select c).FirstOrDefault();

    //  if (currentAttribute == null)
    //  {
    //    currentAttribute = new catalog_product_entity_media_gallery();
    //    currentAttribute.eav_attribute = mediaGalleryAttribute;
    //    currentAttribute.catalog_product_entity = productEntity;
    //    currentAttribute.value = linuxPath;
    //    ctx.AddTocatalog_product_entity_media_gallery(currentAttribute);
    //    ctx.SaveChanges();
    //  }

    //  var valueAttribute = (from v in ctx.catalog_product_entity_media_gallery_value
    //                        where v.catalog_product_entity_media_gallery.value_id == currentAttribute.value_id
    //                        select v).FirstOrDefault();


    //  if (valueAttribute == null)
    //  {
    //    valueAttribute = new catalog_product_entity_media_gallery_value();
    //    valueAttribute.catalog_product_entity_media_gallery = currentAttribute;
    //    valueAttribute.position = position;
    //    valueAttribute.label = String.Empty;
    //    valueAttribute.store_id = STORE_ID_UNSPECIFIED;
    //    valueAttribute.disabled = false;
    //    ctx.AddTocatalog_product_entity_media_gallery_value(valueAttribute);
    //    ctx.SaveChanges();
    //  }


    //}

    //private static string RetrieveFile(Uri sourceUri, string exportPath)
    //{

    //  string fileName = sourceUri.Segments.Last();
    //  string exportFilePath = String.Format(@"{0}\{1}\{2}", fileName.Substring(0, 1),
    //                                        fileName.Substring(1, 1),
    //                                        fileName);
    //  string linuxPath = String.Format(@"/{0}/{1}/{2}", fileName.Substring(0, 1),
    //                                     fileName.Substring(1, 1),
    //                                     fileName);

    //  string outputPath = Path.Combine(exportPath, exportFilePath);

    //  #region retrieve file
    //  if (!File.Exists(outputPath))
    //  {

    //    using (Image img = MagentoUtility.DownloadImage(sourceUri.ToString()))
    //    {
    //      FileInfo info = new FileInfo(outputPath);
    //      if (!Directory.Exists(info.Directory.FullName))
    //      {
    //        Directory.CreateDirectory(info.Directory.FullName);
    //      }

    //      img.Save(outputPath);

    //    }
    //  }

    //  #endregion

    //  return linuxPath;
    //}

    //private static int STORE_ID = 1;
    //private static int STORE_ID_UNSPECIFIED = 0;


    //private static void ProcessProductAttribute(MagentoWebEntities context, catalog_product_entity entity, AttributeResult icecatAttribute)
    //{
    //  string attributeCode = String.Format("ice_{0}", (int)icecatAttribute.Feature_ID);

    //  bool searchable = (icecatAttribute.IsSearchable ?? 0) == 1;


    //  //group 



    //  eav_attribute currentAttribute = (from a in context.eav_attribute
    //                                    where attributeCode == attributeCode
    //                                          && a.entity_type_id == PRODUCT_ENTITY_TYPE_ID
    //                                    select a).FirstOrDefault();








    //}



    //private static void GetOrCreateProductAttribute(MagentoWebEntities ctx, catalog_product_entity productEntity, AttributeResult icecatAttribute)
    //{

    //  string attr_code = String.Format("ice_{0}", (int)icecatAttribute.Feature_ID);
    //  bool searchableAttribute = ((icecatAttribute.IsSearchable ?? 0) == 1);




    //  var currentAttribute = (from a in ctx.eav_attribute
    //                          where a.attribute_code == attr_code
    //                          && a.entity_type_id == PRODUCT_ENTITY_TYPE_ID
    //                          select a).FirstOrDefault();

    //  if (currentAttribute == null)
    //  {
    //    currentAttribute = new eav_attribute();
    //    currentAttribute.entity_type_id = PRODUCT_ENTITY_TYPE_ID;
    //    currentAttribute.backend_type = "varchar";
    //    currentAttribute.frontend_label = icecatAttribute.AttributeName;
    //    currentAttribute.note = icecatAttribute.AttributeName;
    //    currentAttribute.is_user_defined = true;


    //    if (searchableAttribute)
    //    {
    //      currentAttribute.frontend_input = "select";
    //      currentAttribute.source_model = "eav/entity_attribute_source_table";
    //    }
    //    else
    //      currentAttribute.frontend_input = "text";

    //    currentAttribute.attribute_code = attr_code;
    //    ctx.AddToeav_attribute(currentAttribute);


    //  }
    //  ctx.SaveChanges();

    //  eav_attribute_set set = (from s in ctx.eav_attribute_set
    //                           where s.attribute_set_id == 4
    //                           select s).FirstOrDefault();//default

    //  eav_attribute_group group = (from c in ctx.eav_attribute_group
    //                               where c.attribute_group_name == icecatAttribute.AttributeGroupName
    //                               select c).FirstOrDefault();
    //  if (group == null)
    //  {
    //    group = new eav_attribute_group();
    //    group.eav_attribute_set = set;
    //    group.attribute_group_name = icecatAttribute.AttributeGroupName;
    //    ctx.AddToeav_attribute_group(group);
    //    ctx.SaveChanges();
    //  }


    //  catalog_eav_attribute catalogAttr = (from cea in ctx.catalog_eav_attribute
    //                                       where cea.attribute_id == currentAttribute.attribute_id
    //                                       select cea).FirstOrDefault();

    //  if (catalogAttr == null)
    //  {
    //    catalogAttr = new catalog_eav_attribute();
    //    catalogAttr.eav_attribute = currentAttribute;
    //    catalogAttr.is_searchable = true;
    //    catalogAttr.is_visible = true;
    //    catalogAttr.is_visible_on_front = true;
    //    catalogAttr.used_for_sort_by = false;
    //    catalogAttr.is_filterable = true;
    //    catalogAttr.is_comparable = true;
    //    catalogAttr.is_configurable = true;
    //    catalogAttr.is_global = false;
    //    catalogAttr.apply_to = "simple";


    //    ctx.AddTocatalog_eav_attribute(catalogAttr);
    //  }

    //  ctx.SaveChanges();
    //  if (searchableAttribute)
    //  {
    //    var result = (from eav in ctx.eav_attribute_option_value
    //                  where
    //                    eav.eav_attribute_option.eav_attribute.attribute_id ==
    //                    currentAttribute.attribute_id
    //                    && eav.store_id == STORE_ID_UNSPECIFIED
    //                    && eav.value == icecatAttribute.Presentation_value
    //                  select new
    //                  {
    //                    eav_attribute = eav,
    //                    eav_attribute_option = eav.eav_attribute_option
    //                  }
    //                               ).FirstOrDefault();

    //    var currentProductOption = result != null ? result.eav_attribute_option : null;
    //    var currentProductOptionValue = result != null ? result.eav_attribute : null;
    //    if (currentProductOptionValue == null)
    //    {

    //      eav_attribute_option newOption = new eav_attribute_option();
    //      newOption.sort_order = 1;
    //      newOption.eav_attribute = currentAttribute;
    //      ctx.AddToeav_attribute_option(newOption);

    //      ctx.SaveChanges();

    //      //create attribute
    //      currentProductOptionValue = new eav_attribute_option_value();
    //      currentProductOptionValue.store_id = STORE_ID_UNSPECIFIED;
    //      currentProductOptionValue.eav_attribute_option = newOption;
    //      currentProductOptionValue.value = icecatAttribute.Presentation_value;

    //      ctx.AddToeav_attribute_option_value(currentProductOptionValue);

    //      ctx.SaveChanges();

    //      currentProductOption = newOption;
    //    }


    //    // select attribute
    //    var currentProductAttribute = (from a in ctx.catalog_product_entity_int
    //                                   where a.entity_type_id == PRODUCT_ENTITY_TYPE_ID
    //                                         && a.catalog_product_entity.entity_id == productEntity.entity_id
    //                                         && a.eav_attribute.attribute_id == currentAttribute.attribute_id
    //                                         && a.store_id == STORE_ID_UNSPECIFIED
    //                                   select a
    //                                 ).FirstOrDefault();

    //    ctx.SaveChanges();
    //    if (currentProductAttribute == null)
    //    {

    //      currentProductAttribute = new catalog_product_entity_int();
    //      currentProductAttribute.entity_type_id = PRODUCT_ENTITY_TYPE_ID;
    //      currentProductAttribute.catalog_product_entity = productEntity;
    //      currentProductAttribute.store_id = STORE_ID_UNSPECIFIED;
    //      currentProductAttribute.eav_attribute = currentAttribute;
    //      ctx.AddTocatalog_product_entity_int(currentProductAttribute);
    //    }
    //    currentProductAttribute.value = (int)currentProductOption.option_id;

    //    ctx.SaveChanges();



    //  }
    //  else
    //  {
    //    // simple attribute

    //    var currentProductAttribute = (from a in ctx.catalog_product_entity_varchar
    //                                   where a.entity_type_id == PRODUCT_ENTITY_TYPE_ID
    //                                         && a.catalog_product_entity.entity_id == productEntity.entity_id
    //                                         && a.eav_attribute.attribute_id == currentAttribute.attribute_id

    //                                   select a
    //                                  ).FirstOrDefault();


    //    if (currentProductAttribute == null)
    //    {

    //      currentProductAttribute = new catalog_product_entity_varchar();
    //      currentProductAttribute.entity_type_id = PRODUCT_ENTITY_TYPE_ID;
    //      currentProductAttribute.catalog_product_entity = productEntity;
    //      currentProductAttribute.store_id = 0;
    //      currentProductAttribute.eav_attribute = currentAttribute;

    //      ctx.AddTocatalog_product_entity_varchar(currentProductAttribute);
    //    }
    //    currentProductAttribute.value = icecatAttribute.Presentation_value;

    //    ctx.SaveChanges();




    //    eav_entity_attribute entityAttribute = (from ea in ctx.eav_entity_attribute
    //                                            where ea.entity_type_id == PRODUCT_ENTITY_TYPE_ID
    //                                                  && ea.eav_attribute.attribute_id == currentAttribute.attribute_id
    //                                            select ea).FirstOrDefault();

    //    if (entityAttribute == null)
    //    {
    //      entityAttribute = new eav_entity_attribute();
    //      entityAttribute.entity_type_id = PRODUCT_ENTITY_TYPE_ID;
    //      entityAttribute.eav_attribute = currentAttribute;
    //      entityAttribute.sort_order = 1;

    //      ctx.AddToeav_entity_attribute(entityAttribute);
    //    }

    //    entityAttribute.eav_attribute_group = group;
    //    entityAttribute.attribute_set_id = 4;

    //    ctx.SaveChanges();


    //  }
    //}


    //#region Attributes

    //private static void GetOrCreateProductAttribute(MagentoWebEntities ctx, catalog_product_entity productEntity, eav_attribute currentAttribute, string value)
    //{
    //  bool searchableAttribute = (currentAttribute.frontend_input == "select");


    //  if (searchableAttribute)
    //  {
    //    var result = (from eav in ctx.eav_attribute_option_value
    //                  where eav.eav_attribute_option.eav_attribute.attribute_id ==
    //                    currentAttribute.attribute_id
    //                    && eav.store_id == STORE_ID_UNSPECIFIED
    //                    && eav.value == value
    //                  select new
    //                  {
    //                    eav_attribute = eav,
    //                    eav_attribute_option = eav.eav_attribute_option
    //                  }
    //                               ).FirstOrDefault();

    //    var currentProductOption = result != null ? result.eav_attribute_option : null;
    //    var currentProductOptionValue = result != null ? result.eav_attribute : null;
    //    if (currentProductOptionValue == null)
    //    {

    //      eav_attribute_option newOption = new eav_attribute_option();
    //      newOption.sort_order = 1;
    //      newOption.eav_attribute = currentAttribute;
    //      ctx.AddToeav_attribute_option(newOption);

    //      ctx.SaveChanges();

    //      //create attribute
    //      currentProductOptionValue = new eav_attribute_option_value();
    //      currentProductOptionValue.store_id = STORE_ID_UNSPECIFIED;
    //      currentProductOptionValue.eav_attribute_option = newOption;
    //      currentProductOptionValue.value = value;

    //      ctx.AddToeav_attribute_option_value(currentProductOptionValue);

    //      ctx.SaveChanges();

    //      currentProductOption = newOption;
    //    }


    //    // select attribute
    //    var currentProductAttribute = (from a in ctx.catalog_product_entity_int
    //                                   where a.entity_type_id == PRODUCT_ENTITY_TYPE_ID
    //                                         && a.catalog_product_entity.entity_id == productEntity.entity_id
    //                                         && a.eav_attribute.attribute_id == currentAttribute.attribute_id
    //                                         && a.store_id == STORE_ID_UNSPECIFIED
    //                                   select a
    //                                 ).FirstOrDefault();

    //    ctx.SaveChanges();
    //    if (currentProductAttribute == null)
    //    {

    //      currentProductAttribute = new catalog_product_entity_int();
    //      currentProductAttribute.entity_type_id = PRODUCT_ENTITY_TYPE_ID;
    //      currentProductAttribute.catalog_product_entity = productEntity;
    //      currentProductAttribute.store_id = STORE_ID_UNSPECIFIED;
    //      currentProductAttribute.eav_attribute = currentAttribute;
    //      ctx.AddTocatalog_product_entity_int(currentProductAttribute);
    //    }
    //    currentProductAttribute.value = (int)currentProductOption.option_id;

    //    ctx.SaveChanges();


    //  }
    //  else
    //  {
    //    var prodNameAttribute = (from a in ctx.catalog_product_entity_varchar
    //                             where a.entity_type_id == PRODUCT_ENTITY_TYPE_ID
    //                                   && a.catalog_product_entity.entity_id == productEntity.entity_id
    //                                   && a.eav_attribute.attribute_id == currentAttribute.attribute_id
    //                             select a
    //                            ).FirstOrDefault();

    //    if (prodNameAttribute == null)
    //    {
    //      prodNameAttribute = new catalog_product_entity_varchar();
    //      prodNameAttribute.entity_type_id = PRODUCT_ENTITY_TYPE_ID;
    //      prodNameAttribute.catalog_product_entity = productEntity;
    //      prodNameAttribute.eav_attribute = currentAttribute;


    //      ctx.AddTocatalog_product_entity_varchar(prodNameAttribute);

    //    }

    //    prodNameAttribute.value = value;


    //  }

    //}


    //private static catalog_product_entity_int GetOrCreateProductAttribute(MagentoWebEntities ctx, catalog_product_entity productEntity, eav_attribute productNameAttribute, int value)
    //{
    //  var prodNameAttribute = (from a in ctx.catalog_product_entity_int
    //                           where a.entity_type_id == PRODUCT_ENTITY_TYPE_ID
    //                                 && a.catalog_product_entity.entity_id == productEntity.entity_id
    //                                 && a.eav_attribute.attribute_id == productNameAttribute.attribute_id
    //                           select a
    //                          ).FirstOrDefault();

    //  if (prodNameAttribute == null)
    //  {
    //    prodNameAttribute = new catalog_product_entity_int();
    //    prodNameAttribute.entity_type_id = PRODUCT_ENTITY_TYPE_ID;
    //    prodNameAttribute.catalog_product_entity = productEntity;
    //    prodNameAttribute.eav_attribute = productNameAttribute;

    //    ctx.AddTocatalog_product_entity_int(prodNameAttribute);
    //  }

    //  prodNameAttribute.value = value;

    //  return prodNameAttribute;
    //}


    //private static catalog_product_entity_decimal GetOrCreateProductAttribute(MagentoWebEntities ctx, catalog_product_entity productEntity, eav_attribute productNameAttribute, decimal value)
    //{
    //  var prodNameAttribute = (from a in ctx.catalog_product_entity_decimal
    //                           where a.entity_type_id == PRODUCT_ENTITY_TYPE_ID
    //                                 && a.catalog_product_entity.entity_id == productEntity.entity_id
    //                                 && a.eav_attribute.attribute_id == productNameAttribute.attribute_id
    //                           select a
    //                          ).FirstOrDefault();

    //  if (prodNameAttribute == null)
    //  {
    //    prodNameAttribute = new catalog_product_entity_decimal();
    //    prodNameAttribute.entity_type_id = PRODUCT_ENTITY_TYPE_ID;
    //    prodNameAttribute.catalog_product_entity = productEntity;
    //    prodNameAttribute.eav_attribute = productNameAttribute;

    //    ctx.AddTocatalog_product_entity_decimal(prodNameAttribute);

    //  }

    //  prodNameAttribute.value = value;

    //  return prodNameAttribute;
    //}


    //#endregion

    //private static eav_attribute GetAttribute(MagentoWebEntities ctx, int entity_type_id, string attribute_code)
    //{
    //  var attribute = ctx.eav_attribute.FirstOrDefault(x => x.attribute_code == attribute_code && x.entity_type_id == entity_type_id);

    //  if (attribute == null)
    //  {
    //    attribute = new eav_attribute()
    //              {
    //                attribute_code = attribute_code,
    //                backend_type = "varchar",
    //                note = string.Empty,
    //                entity_type_id = entity_type_id,
    //              };
    //    ctx.AddToeav_attribute(attribute);
    //    ctx.SaveChanges();
    //    return attribute;

    //  }
    //  else
    //    return attribute;


    //}
  }
}
