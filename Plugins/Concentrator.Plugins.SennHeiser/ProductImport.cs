using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web;
using System.Xml.Linq;
using Concentrator.Objects;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.Product;
using Concentrator.Objects.Product.ProductAttributes;
using Concentrator.Objects.Service;
using Concentrator.Objects.Vendors;


namespace Concentrator.Plugins.SennHeiser
{
  class ProductImport : ConcentratorPlugin
  {

    private int vendorID_NL;
    private int vendorID_BE;
    private const int unmappedID = -1;
    private const int languageID = 1;
    private string[] AttributeMapping = new[] { "Category", "UPC_Master_Carton", "Warranty", "Pkg_Gram", "Unit_Gram", "Color", "MasterCartonQty", "BC_euro_cost", "MIP_euro_sell", "Bullet1", "Bullet2", "Bullet3", "Bullet4", "Bullet5" };

    public override string Name
    {
      get { return "Sennheiser Product Import Plugin"; }
    }

    protected override void Process()
    {

      var config = GetConfiguration();

      vendorID_NL = int.Parse(config.AppSettings.Settings["VendorID_NL"].Value);
      vendorID_BE = int.Parse(config.AppSettings.Settings["VendorID_BE"].Value);

      var productXmlFilePath = config.AppSettings.Settings["SennheiserBasePath"].Value + "products.xml";
      var productXmlCatPath = config.AppSettings.Settings["SennheiserBasePath"].Value + "categories.xml";

      log.InfoFormat("Processing file: {0}", "products.xml");

      var xmlText = File.ReadAllText(productXmlFilePath);

      var doc2 = HttpUtility.HtmlDecode(xmlText);

      XDocument doc = null;
      XDocument cat = null;

      try
      {
        xmlText = xmlText.Replace("<br>", " ");
        xmlText = xmlText.Replace("&M", "&amp;M");

        doc = XDocument.Parse(xmlText);
        cat = XDocument.Load(productXmlCatPath);
      }
      catch (Exception ex)
      {
        log.Error(String.Format("No new files to process or file not available"), ex);
      }

      if (!Running)
        return;

      using (ConcentratorDataContext context = new ConcentratorDataContext())
      {
        bool success = ParseDocument(context, doc, cat);

        if (success)
        {
          log.DebugFormat("File successfully processed");
        }
      }
      {
        log.DebugFormat("No new files to process");
      }
    }

    private bool ParseDocument(ConcentratorDataContext context, XDocument doc, XDocument categories)
    {

      

      var brands = (from bv in context.BrandVendors
                    where bv.VendorID == vendorID_NL
                    select bv).ToList();

      var productGroups_NL = (from g in context.ProductGroupVendors
                              where g.VendorID == vendorID_NL
                              select g).ToList();

      var productAttributes_NL = (from g in context.ProductAttributeMetaDatas
                                  where g.VendorID == vendorID_NL
                                  select g).ToList();

      var brands_BE = (from bv in context.BrandVendors
                       where bv.VendorID == vendorID_BE
                       select bv).ToList();

      var productGroups_BE = (from g in context.ProductGroupVendors
                              where g.VendorID == vendorID_BE
                              select g).ToList();

      var productAttributes_BE = (from g in context.ProductAttributeMetaDatas
                                  where g.VendorID == vendorID_BE
                                  select g).ToList();

      var productAttributeGroups = (from g in context.ProductAttributeGroupMetaDatas select g).ToList();

      int counter = 0;
      int total = itemProducts.Count();
      int totalNumberOfProductsToProcess = total;
      log.InfoFormat("Start inport {0} products", total);

      var Languages = new List<string>()
                        {
                          "NL",
                          "BE"
                        };
      foreach (var language in Languages)
      {
        foreach (var product in itemProducts)
        {
          int vendorID = (language == "NL" ? vendorID_NL : vendorID_BE);

          if (counter == 50)
          {
            counter = 0;
            log.InfoFormat("Still need to process {0} of {1}; {2} done;", totalNumberOfProductsToProcess, total, total - totalNumberOfProductsToProcess);
          }
          totalNumberOfProductsToProcess--;
          counter++;

          try
          {
            #region BrandVendor

            Product prod = null;
            //check if brandvendor exists in db
            var brandVendor = language == "NL" ? brands_NL.FirstOrDefault(vb => vb.VendorBrandCode == product.VendorBrandCode + "NL") : brands_BE.FirstOrDefault(vb => vb.VendorBrandCode == product.VendorBrandCode + "BE");

            if (brandVendor == null) //if brandvendor does not exist
            {
              //create new brandVendor
              brandVendor = new BrandVendor
                              {
                                BrandID = unmappedID,
                                VendorID = vendorID,
                                VendorBrandCode = language == "NL" ? product.VendorBrandCode + "NL" : product.VendorBrandCode + "BE",
                                Name = language == "NL" ? product.VendorBrandCode + "NL" : product.VendorBrandCode + "BE",
                              };
              context.BrandVendors.InsertOnSubmit(brandVendor);
              if (language == "NL")
              {
                brands_NL.Add(brandVendor);
              }
              else brands_BE.Add(brandVendor);
            }

            //use BrandID to create product and retreive ProductID

            var BrandID = brandVendor.BrandID;

            prod = context.Products.FirstOrDefault(p => p.VendorItemNumber == product.CustomItemNr);

            //if product does not exist (usually true)
            if (prod == null)
            {
              prod = new Product
                       {
                         VendorItemNumber = product.CustomItemNr.ToString(),
                         BrandID = BrandID,
                         SourceVendorID = vendorID
                       };
              context.Products.InsertOnSubmit(prod);
              //context.SubmitChanges();
            }

            #endregion

            #region ProductImage

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
              var productImage = context.ProductImages.FirstOrDefault(pi => pi.Product.ProductID == prod.ProductID);

              //if (productImage == null)
              //{
              //  productImage = new ProductImage
              //  {
              //    Product = prod,
              //    VendorID = language == "NL" ? vendorID_NL : vendorID_BE,
              //    ImageUrl = product.ImageUrl
              //  };
              //  context.ProductImages.InsertOnSubmit(productImage);
              //}

              if (productImage == null)
              {
                productImage = new ProductImage
                                 {

                                   Product = prod,
                                   VendorID = vendorID
                                 };
                context.ProductImages.InsertOnSubmit(productImage);
              }

              productImage.ImageUrl = product.ImageUrl;
            }

            #endregion

            #region VendorAssortMent

            var productID = prod.ProductID;

            var vendorAssortment = language == "NL" ? prod.VendorAssortment.FirstOrDefault(va => va.VendorID == vendorID_NL) : prod.VendorAssortment.FirstOrDefault(va => va.VendorID == vendorID_BE);

            //if vendorAssortMent does not exist
            if (vendorAssortment == null)
            {
              //create vendorAssortMent with productID
              vendorAssortment = new VendorAssortment
                                   {
                                     Product = prod,
                                     CustomItemNumber = product.CustomItemNr.ToString(),
                                     VendorID = vendorID,
                                     ShortDescription =
                                       product.ShortDescription.Length > 150
                                         ? product.ShortDescription.Substring(0, 150)
                                         : product.ShortDescription,
                                     LongDescription = ""
                                   };
              context.VendorAssortments.InsertOnSubmit(vendorAssortment);
              // context.SubmitChanges();
            }

            #endregion

            #region ProductGroupVendor

            ////create vendorGroup five times, each time with a different VendorProductGroupCode on a different level if not exist
            int productGroupCount = 1;

            foreach (var cat in product.ProductCategory.Split('_'))
            {
              var productGroupVendor = language == "NL"
                                          ? productGroups_NL.FirstOrDefault(
                                              pg => pg.GetType().GetProperty("VendorProductGroupCode" + productGroupCount).GetValue(pg, null) == cat)
                                          : productGroups_BE.FirstOrDefault(
                                              pg => pg.GetType().GetProperty("VendorProductGroupCode" + productGroupCount).GetValue(pg, null) == cat);


              if (productGroupVendor == null)
              {
                productGroupVendor = new ProductGroupVendor
                                        {
                                          ProductGroupID = unmappedID,
                                          VendorID = vendorID,
                                          VendorName = product.VendorName,
                                          VendorProductGroupCode1 = cat

                                        };
                context.ProductGroupVendors.InsertOnSubmit(productGroupVendor);
                if (language == "NL")
                {
                  productGroups_NL.Add(productGroupVendor);
                }
                else
                {
                  productGroups_BE.Add(productGroupVendor);
                }

              }

              var vendorProductGroupAssortments = (from c in context.VendorProductGroupAssortments
                                                   where
                                                     c.VendorAssortment == vendorAssortment
                                                   select c).ToList();

              var vendorProductGroupAssortment1 =
                vendorProductGroupAssortments.FirstOrDefault(
                  vPga => vPga.ProductGroupVendor == productGroupVendor);

              if (vendorProductGroupAssortment1 == null)
              {
                vendorProductGroupAssortment1 = new VendorProductGroupAssortment
                                                  {
                                                    VendorAssortment = vendorAssortment,
                                                    ProductGroupVendor = productGroupVendor
                                                  };
                context.VendorProductGroupAssortments.InsertOnSubmit(vendorProductGroupAssortment1);
              }

              productGroupCount++;
            }

            #endregion

            #region ProductDescription

            var productDescription =
              prod.ProductDescription.FirstOrDefault(pd => pd.LanguageID == languageID && pd.VendorID == vendorID);

            if (productDescription == null)
            {
              //create ProductDescription
              productDescription = new ProductDescription
                                     {
                                       Product = prod,
                                       LanguageID = languageID,
                                       VendorID = vendorID,
                                       ShortContentDescription = product.ShortDescription.Length > 1000
                                                                   ? product.ShortDescription.Substring(0, 1000)
                                                                   : product.ShortDescription,
                                       ProductName = product.ProductName.ToString()
                                     };

              context.ProductDescription.InsertOnSubmit(productDescription);
            }
            else
            {
              productDescription.ProductName = product.ProductName.ToString();
              productDescription.ShortContentDescription = product.ShortDescription.Length > 1000
                                                             ? product.ShortDescription.Substring(0, 1000)
                                                             : product.ShortDescription;

            }

            #endregion

            // first insert all the TechnicalData
            foreach (var content in product.TechnicalData)
            {
              #region ProductAttributeGroupMetaData

              var productAttributeGroupMetaData =
                productAttributeGroups.FirstOrDefault(c => c.GroupCode == "TechnicalData");
              //create ProductAttributeGroupMetaData if not exists
              if (productAttributeGroupMetaData == null)
              {
                productAttributeGroupMetaData = new ProductAttributeGroupMetaData
                                                  {
                                                    Index = 0,
                                                    GroupCode = "TechnicalData",
                                                    VendorID = vendorID
                                                  };
                context.ProductAttributeGroupMetaDatas.InsertOnSubmit(productAttributeGroupMetaData);
                productAttributeGroups.Add(productAttributeGroupMetaData);
              }
              #endregion
              //  context.SubmitChanges();

              #region ProductAttributeGroupName

              var productAttributeGroupName =
                productAttributeGroupMetaData.ProductAttributeGroupLabels.FirstOrDefault(c => c.LanguageID == languageID);
              //create ProductAttributeGroupName if not exists
              if (productAttributeGroupName == null)
              {
                productAttributeGroupName = new ProductAttributeGroupName
                                              {
                                                Name = "TechnicalData",
                                                ProductAttributeGroup = productAttributeGroupMetaData,
                                                LanguageID = languageID
                                              };
                context.ProductAttributeGroupNames.InsertOnSubmit(productAttributeGroupName);
              }
              //context.SubmitChanges();

              #endregion

              #region ProductAttributeMetaData

              //create ProductAttributeMetaData as many times that there are entrys in technicaldata

              var productAttributeMetaData = language == "NL" ? productAttributes_NL.FirstOrDefault(c => c.AttributeCode == content.AttributeCode) : productAttributes_BE.FirstOrDefault(c => c.AttributeCode == content.AttributeCode);
              //create ProductAttributeMetaData if not exists
              if (productAttributeMetaData == null)
              {
                productAttributeMetaData = new ProductAttributeMetaData
                                             {
                                               ProductAttributeGroup = productAttributeGroupMetaData,
                                               AttributeCode = content.AttributeCode.ToString(),
                                               Index = 0,
                                               IsVisible = true,
                                               NeedsUpdate = true,
                                               VendorID = vendorID,
                                               IsSearchable = false
                                             };
                context.ProductAttributeMetaDatas.InsertOnSubmit(productAttributeMetaData);
                if (language == "NL")
                {
                  productAttributes_NL.Add(productAttributeMetaData);
                }
                else productAttributes_BE.Add(productAttributeMetaData);
              }

              // context.SubmitChanges();
              #endregion

              #region ProductAttributeName

              var productAttributeName =
                productAttributeMetaData.ProductAttributeLabels.FirstOrDefault(c => c.LanguageID == languageID);
              //create ProductAttributeName if not exists
              if (productAttributeName == null)
              {
                productAttributeName = new ProductAttributeName
                                         {
                                           ProductAttribute = productAttributeMetaData,
                                           LanguageID = languageID,
                                           Name = content.AttributeCode.ToString()
                                         };
                context.ProductAttributeNames.InsertOnSubmit(productAttributeName);
              }
              else
              {
                productAttributeName.Name = content.AttributeCode.ToString();
              }
              // context.SubmitChanges();
              #endregion

              #region ProductAttributeValue

              var productAttributeValue =
                productAttributeMetaData.ProductAttributeValues.FirstOrDefault(c => c.ProductID == prod.ProductID);
              //create ProductAttributeValue 
              if (productAttributeValue == null)
              {
                productAttributeValue = new ProductAttributeValue
                                          {
                                            ProductAttribute = productAttributeMetaData,
                                            Product = prod,
                                            Value = content.Value,
                                            LanguageID = languageID
                                          };
                context.ProductAttributeValues.InsertOnSubmit(productAttributeValue);
              }
              else
              {
                productAttributeValue.Value = content.Value;
              }

              #endregion
            }
            
            //  insert all other Attributes
            foreach (var content in product.Attributes)
            {
              #region ProductAttributeGroupMetaData

              var productAttributeGroupMetaData =
                productAttributeGroups.FirstOrDefault(c => c.GroupCode == "General");
              //create ProductAttributeGroupMetaData if not exists
              if (productAttributeGroupMetaData == null)
              {
                productAttributeGroupMetaData = new ProductAttributeGroupMetaData
                {
                  Index = 0,
                  GroupCode = "General",
                  VendorID = vendorID
                };
                context.ProductAttributeGroupMetaDatas.InsertOnSubmit(productAttributeGroupMetaData);
                productAttributeGroups.Add(productAttributeGroupMetaData);
              }
              #endregion

              #region ProductAttributeGroupName

              var productAttributeGroupName =
                productAttributeGroupMetaData.ProductAttributeGroupLabels.FirstOrDefault(c => c.LanguageID == languageID);
              //create ProductAttributeGroupName if not exists
              if (productAttributeGroupName == null)
              {
                productAttributeGroupName = new ProductAttributeGroupName
                {
                  Name = "General",
                  ProductAttributeGroup = productAttributeGroupMetaData,
                  LanguageID = languageID
                };
                context.ProductAttributeGroupNames.InsertOnSubmit(productAttributeGroupName);
              }

              #endregion

              #region ProductAttributeMetaData

              //create ProductAttributeMetaData as many times that there are entrys in technicaldata

              var productAttributeMetaData = language == "NL" ?
                productAttributes_NL.FirstOrDefault(c => c.AttributeCode == content.AttributeCode) : productAttributes_BE.FirstOrDefault(c => c.AttributeCode == content.AttributeCode);
              //create ProductAttributeMetaData if not exists
              if (productAttributeMetaData == null)
              {
                productAttributeMetaData = new ProductAttributeMetaData
                {
                  ProductAttributeGroup = productAttributeGroupMetaData,
                  AttributeCode = content.AttributeCode,
                  Index = 0,
                  IsVisible = true,
                  NeedsUpdate = true,
                  VendorID = vendorID,
                  IsSearchable = false
                };
                context.ProductAttributeMetaDatas.InsertOnSubmit(productAttributeMetaData);
                if (language == "NL")
                {
                  productAttributes_NL.Add(productAttributeMetaData);
                }
                else productAttributes_BE.Add(productAttributeMetaData);
              }

              #endregion

              #region ProductAttributeName

              var productAttributeName =
                productAttributeMetaData.ProductAttributeLabels.FirstOrDefault(c => c.LanguageID == languageID);
              //create ProductAttributeName if not exists
              if (productAttributeName == null)
              {
                productAttributeName = new ProductAttributeName
                {
                  ProductAttribute = productAttributeMetaData,
                  LanguageID = languageID,
                  Name = content.AttributeCode
                };
                context.ProductAttributeNames.InsertOnSubmit(productAttributeName);
              }
              else
              {
                productAttributeName.Name = content.AttributeCode;
              }

              #endregion

              #region ProductAttributeValue

              var productAttributeValue =
                productAttributeMetaData.ProductAttributeValues.FirstOrDefault(c => c.ProductID == prod.ProductID);
              //create ProductAttributeValue 
              if (productAttributeValue == null)
              {
                productAttributeValue = new ProductAttributeValue
                {
                  ProductAttribute = productAttributeMetaData,
                  Product = prod,
                  Value = content.Value,
                  LanguageID = languageID
                };
                context.ProductAttributeValues.InsertOnSubmit(productAttributeValue);
              }
              else
              {
                productAttributeValue.Value = content.Value;
              }

              #endregion
            }

            context.SubmitChanges();
            //  insert Guarantee data
            #region ProductAttributeGroupMetaData

            var guaranteeProductAttributeGroupMetaData =
                productAttributeGroups.FirstOrDefault(c => c.GroupCode == "Guarantee");
            //create ProductAttributeGroupMetaData if not exists
            if (guaranteeProductAttributeGroupMetaData == null)
            {
              guaranteeProductAttributeGroupMetaData = new ProductAttributeGroupMetaData
              {
                Index = 0,
                GroupCode = "Guarantee",
                VendorID = vendorID
              };
              context.ProductAttributeGroupMetaDatas.InsertOnSubmit(guaranteeProductAttributeGroupMetaData);
              productAttributeGroups.Add(guaranteeProductAttributeGroupMetaData);
            }
            #endregion
            //  context.SubmitChanges();

            #region ProductAttributeGroupName

            var guaranteeProductAttributeGroupName =
              guaranteeProductAttributeGroupMetaData.ProductAttributeGroupLabels.FirstOrDefault(c => c.LanguageID == languageID);
            //create ProductAttributeGroupName if not exists
            if (guaranteeProductAttributeGroupName == null)
            {
              guaranteeProductAttributeGroupName = new ProductAttributeGroupName
              {
                Name = "Guarantee",
                ProductAttributeGroup = guaranteeProductAttributeGroupMetaData,
                LanguageID = languageID
              };
              context.ProductAttributeGroupNames.InsertOnSubmit(guaranteeProductAttributeGroupName);
            }
            //context.SubmitChanges();

            #endregion

            #region ProductAttributeMetaData

            //create ProductAttributeMetaData as many times that there are entrys in technicaldata

            var guaranteeProductAttributeMetaData = language == "NL" ?
              productAttributes_NL.FirstOrDefault(c => c.AttributeCode == "GuaranteeSupplier") : productAttributes_BE.FirstOrDefault(c => c.AttributeCode == "GuaranteeSupplier"); ;
            //create ProductAttributeMetaData if not exists
            if (guaranteeProductAttributeMetaData == null)
            {
              guaranteeProductAttributeMetaData = new ProductAttributeMetaData
              {
                ProductAttributeGroup = guaranteeProductAttributeGroupMetaData,
                AttributeCode = "GuaranteeSupplier",
                Index = 0,
                IsVisible = true,
                NeedsUpdate = true,
                VendorID = vendorID,
                IsSearchable = false
              };
              context.ProductAttributeMetaDatas.InsertOnSubmit(guaranteeProductAttributeMetaData);
              if (language == "NL")
              {
                productAttributes_NL.Add(guaranteeProductAttributeMetaData);
              }
              else productAttributes_BE.Add(guaranteeProductAttributeMetaData);
            }

            // context.SubmitChanges();
            #endregion

            #region ProductAttributeName

            var guaranteeProductAttributeName =
              guaranteeProductAttributeMetaData.ProductAttributeLabels.FirstOrDefault(c => c.LanguageID == languageID);
            //create ProductAttributeName if not exists
            if (guaranteeProductAttributeName == null)
            {
              guaranteeProductAttributeName = new ProductAttributeName
              {
                ProductAttribute = guaranteeProductAttributeMetaData,
                LanguageID = languageID,
                Name = "GuaranteeSupplier"
              };
              context.ProductAttributeNames.InsertOnSubmit(guaranteeProductAttributeName);
            }

            // context.SubmitChanges();
            #endregion

            #region ProductAttributeValue

            var guaranteeProductAttributeValue =
              guaranteeProductAttributeMetaData.ProductAttributeValues.FirstOrDefault(c => c.ProductID == prod.ProductID);
            //create ProductAttributeValue 
            if (guaranteeProductAttributeValue == null)
            {
              guaranteeProductAttributeValue = new ProductAttributeValue
              {
                ProductAttribute = guaranteeProductAttributeMetaData,
                Product = prod,
                Value = "Sennheiser",
                LanguageID = languageID
              };
              context.ProductAttributeValues.InsertOnSubmit(guaranteeProductAttributeValue);
            }

            #endregion


            context.SubmitChanges();
          }
          catch (Exception ex)
          {
            log.ErrorFormat("product: {0} error: {1}", product.CustomItemNr, ex.StackTrace);
            return false;
          }
        }
      }

      foreach (var product in itemProducts)
      {
        #region Related Products


        var relatedProduct = (from va in context.VendorAssortments where va.CustomItemNumber == product.CustomItemNr && va.VendorID == vendorID_NL select va).FirstOrDefault().Try<VendorAssortment, int?>(c => c.ProductID, null);
        var relatedProducts = (from rp in context.RelatedProducts
                               where
                                 rp.ProductID == (from pro in context.Products
                                                  where pro.VendorItemNumber == product.CustomItemNr.ToString() && pro.SourceVendorID == vendorID_NL
                                                  select pro.ProductID).FirstOrDefault() &&
                                 rp.VendorID == vendorID_NL
                               select rp).ToList();

        foreach (var relatedProd in product.RelatedProducts)
        {
          if ((from pro in context.Products
               where pro.VendorItemNumber == relatedProd.RelatedProductID
               select pro.ProductID).FirstOrDefault() != 0)
          {
            if (relatedProduct.HasValue)
            {
              RelatedProduct relProd = (from rp in relatedProducts
                                        where
                                          rp.ProductID == (from pro in context.Products
                                                           where pro.VendorItemNumber == product.CustomItemNr.ToString() && pro.SourceVendorID == vendorID_NL
                                                           select pro.ProductID).FirstOrDefault() &&
                                          rp.RelatedProductID == (from pro in context.Products
                                                                  where pro.VendorItemNumber == relatedProd.RelatedProductID && pro.SourceVendorID == vendorID_NL
                                                                  select pro.ProductID).FirstOrDefault() &&
                                          rp.VendorID == vendorID_NL
                                        select rp).FirstOrDefault();

              if (relProd == null)
              {
                relProd = new RelatedProduct
                {
                  ProductID = (from pro in context.Products
                               where pro.VendorItemNumber == product.CustomItemNr.ToString() && pro.SourceVendorID == vendorID_NL
                               select pro.ProductID).FirstOrDefault(),
                  RelatedProductID = (from pro in context.Products
                                      where pro.VendorItemNumber == relatedProd.RelatedProductID && pro.SourceVendorID == vendorID_NL
                                      select pro.ProductID).FirstOrDefault(),
                  VendorID = vendorID_NL
                };
                context.RelatedProducts.InsertOnSubmit(relProd);
                relatedProducts.Add(relProd);
              }
            }
          }
        }
      }
        #endregion

      return true;
    }
  }

}

class SanitizeXml
{
  /// <summary>
  /// Remove illegal XML characters from a string.
  /// </summary>
  public string SanitizeXmlString(string xml)
  {
    if (xml == null)
    {
      throw new ArgumentNullException("xml");
    }

    StringBuilder buffer = new StringBuilder(xml.Length);

    foreach (char c in xml)
    {
      if (IsLegalXmlChar(c))
      {
        buffer.Append(c);
      }
    }

    return buffer.ToString();
  }

  /// <summary>
  /// Whether a given character is allowed by XML 1.0.
  /// </summary>
  public bool IsLegalXmlChar(int character)
  {
    return
    (
         character == 0x9 /* == '\t' == 9   */          ||
         character == 0xA /* == '\n' == 10  */          ||
         character == 0xD /* == '\r' == 13  */          ||
        (character >= 0x20 && character <= 0xD7FF) ||
        (character >= 0xE000 && character <= 0xFFFD) ||
        (character >= 0x10000 && character <= 0x10FFFF)
    );
  }
}