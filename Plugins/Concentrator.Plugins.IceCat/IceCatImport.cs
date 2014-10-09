using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using Concentrator.Objects.Service;
using Concentrator.Objects;
using Concentrator.Objects.Product;
using System.Diagnostics;
using System.Configuration;
using Concentrator.Objects.Enumaration;
using Concentrator.Objects.ProductAttribute;
using Concentrator.Objects.Product.ProductAttributes;
using System.Threading;
using Concentrator.Objects.Utility.General;
using Concentrator.Vendors.IceCat;

namespace Concentrator.Plugins.ContentVendor
{
  public class IceCatImport
  {
    private readonly static int contentVendorID = (int)ContentVendorTypes.IceCat;

    private readonly static List<int> importLanguages = new List<int> { 1, 2 };

    public void ProcessContent(bool update, log4net.ILog log)
    {
      try
      {
        log.Debug("Start Update brand information");
        UpdateBrands();
        log.Debug("Finish Update brand information");
        log.Debug("Start Insert/Update Product information");
        ProcessProductInformation(update, log);
        log.Debug("Finish Insert/Update Product information");
        //log.Debug("Start Insert/Update Cross sell information");
        //ProcessCrossSellItems();
        //log.Debug("Start Insert/Update Cross sell information");
      }
      catch (Exception ex)
      {
        log.Error("Error import ICEcat content", ex);
      }

    }


    private void ProcessCrossSellItems()
    {
      using (ConcentratorDataContext contextConcentrator = new ConcentratorDataContext())
      {
        using (ICEcatDataContext contextIceCat = new ICEcatDataContext())
        {

          //var productVendorItemNumbers = (from p in contextConcentrator.Products select p.VendorItemNumber).ToList();

          var relatedProducts = (from rp in contextIceCat.dt_ProductRelateds
                                 select rp).ToList();

          foreach (var relatedProduct in relatedProducts)
          {
            //get identifier to check on with concentrator for the source product. VendorItemNuber ?
            var productIDICECatLeft = (from ic in contextIceCat.dt_Products
                                       where ic.Product_ID == relatedProduct.Product_ID
                                       select ic.Prod_ID).FirstOrDefault();

            //get identifier to check on with concentrator for the destination product. VendorItemNuber ?
            var productIDICECatRight = (from ic in contextIceCat.dt_Products
                                        where ic.Product_ID == relatedProduct.ProductRelated_ID
                                        select ic.Prod_ID).FirstOrDefault();


            var product =
              (from p in contextConcentrator.Products where p.VendorItemNumber == productIDICECatLeft select p).FirstOrDefault();

            var relatedProductItem = (from p in contextConcentrator.Products where p.VendorItemNumber == productIDICECatRight select p).FirstOrDefault();

            //if any of the two products does not exist in the concentrator --- continue
            if (product == null || relatedProductItem == null) continue;


            //grab the related item from the concentrator if any
            var relatedItem = (from p in contextConcentrator.RelatedProducts
                               where p.Product.VendorItemNumber == productIDICECatLeft
                                     &&
                                     p.RelatedProduct_Product1.VendorItemNumber == productIDICECatRight
                                     &&
                                     p.VendorID == contentVendorID
                               select p).FirstOrDefault();

            //if there is isnt such an item in the related products of the concentrator
            if (relatedItem == null)
            {
              //create related item
              RelatedProduct rp = new RelatedProduct()
              {
                Product = product,
                RelatedProduct_Product1 = relatedProductItem,
                VendorID = contentVendorID

              };

              contextConcentrator.RelatedProducts.InsertOnSubmit(rp);
            }
          }

          contextConcentrator.SubmitChanges();
        }
      }
    }

    private void ProcessProductInformation(bool update, log4net.ILog log)
    {
      log.Debug("Start process product Information");
      using (var concentratorContext = new ConcentratorDataContext())
      {
        var vendorContent = (from p in concentratorContext.Products
                             join cv in concentratorContext.BrandVendors on p.BrandID equals cv.BrandID
                             where cv.VendorID == contentVendorID && cv.BrandID > 0
                             select new
                             {
                               ProductID = p.ProductID,
                               VendorItemNumber = p.VendorItemNumber.Trim(),
                               BrandID = p.BrandID,
                               IceCatSupplierID = int.Parse(cv.VendorBrandCode),
                               ShortDescription = string.Empty,
                               LongDescription = string.Empty,
                               p.CreationTime
                             }).Distinct().OrderBy(x => x.ProductID).ToList();


        if (update)
          vendorContent = vendorContent.Where(x => x.CreationTime > DateTime.Now.AddDays(-1)).Select(x => x).ToList();


        using (ICEcatDataContext ctx = new ICEcatDataContext(ConfigurationManager.ConnectionStrings["ICEcat"].ConnectionString))
        {
          //ctx.Log = new DebuggerWriter();

          List<ProductDescription> newAddedProducts = new List<ProductDescription>();
          int counter = 0;
          int logstep = 0;


          //List<ManualResetEvent> manualEvents = new List<ManualResetEvent>();

          //ThreadPool.SetMaxThreads(40, 40);

          //List<BatchResult> _batchResults = new List<BatchResult>();
          int totalNumberOfProductsToProcess = vendorContent.Count();

          int processed = 0;

          for (int i = vendorContent.Count - 1; i >= 0; i--)
          {
            StateInfo info = new StateInfo
            {
              VendorItemNumber = vendorContent[i].VendorItemNumber,
              IceCatSupplierID = vendorContent[i].IceCatSupplierID,
              BrandID = vendorContent[i].BrandID,
              ProductID = vendorContent[i].ProductID
            };

//#if DEBUG
//            if (info.ProductID != 2335976)
//              continue;
//#endif

            Callback(info, log);

            processed++;
            if (processed >= 1000)
            {

              //int numberOfProducts = manualEvents.Count;
              //log.Info("=================================================");
              //log.InfoFormat("Start processing icecat product information , batch of {0} products.", numberOfProducts);
              //WaitHandle.WaitAll(manualEvents.ToArray());
              //double executionTime = (DateTime.Now - startTime).TotalSeconds;
              //log.InfoFormat("Processed {0} products in : {1} seconds", numberOfProducts, executionTime);
              //_batchResults.Add(new BatchResult(numberOfProducts, executionTime));

              log.InfoFormat("Processed {0} of {1} total records", vendorContent.Count - i, vendorContent.Count);

              //log.InfoFormat("Still need to process {0} of {1}; {2} done; Estimated completiontime: {3}; Average product time : {4}s",
              //    vendorContent.Count, totalNumberOfProductsToProcess, totalNumberOfProductsToProcess - vendorContent.Count, DateTime.Now.AddSeconds(secondsNeeded), averageSecondsPerProduct);
              log.Info("=================================================");
              processed = 0;
              DateTime startTime = DateTime.Now;

            }
          }
        }
      }
      log.Debug("Finish process product Information");
    }

    private void UpdateBrands()
    {
           
      
      using (ConcentratorDataContext concentratorContext = new ConcentratorDataContext())
      {
        var brands = (from b in concentratorContext.BrandVendors
                                   where b.VendorID == contentVendorID
                                   select b).ToList();


        using (ICEcatDataContext ctx = new ICEcatDataContext(ConfigurationManager.ConnectionStrings["ICEcat"].ConnectionString))
        {
          var iceCatContentVendorBrands = (from c in ctx.Brands                                           
                                           select new
                                           {
                                             ContentVendorBrandID = c.Supplier_ID,
                                             ContentVendorBrandCode = c.Supplier_Name
                                           }).Distinct().ToList();

          foreach (var pair in iceCatContentVendorBrands)
          {
            var vbrand = brands.FirstOrDefault(vb => vb.VendorBrandCode == pair.ContentVendorBrandID.ToString());

            if (vbrand == null)
            {
              vbrand = new BrandVendor
              {
                VendorID = contentVendorID,
                VendorBrandCode = pair.ContentVendorBrandID.ToString(),
                Name = pair.ContentVendorBrandCode,
                BrandID = -1
              };

              brands.Add(vbrand);

              concentratorContext.BrandVendors.InsertOnSubmit(vbrand);
            }
          }
          concentratorContext.SubmitChanges();
        }
      }
    }

    //private List<ProductAttributeGroupMetaData> _productAttributeGroupMetaDatas;
    //private List<ProductAttributeGroupMetaData> ProductAttributeGroupMetaDatas
    //{
    //  get
    //  {
    //    if (_productAttributeGroupMetaDatas == null)
    //    {
    //      _productAttributeGroupMetaDatas = (from pagmd in concentratorContext.ProductAttributeGroupMetaDatas
    //                                         select pagmd).ToList();
    //    }
    //    return _productAttributeGroupMetaDatas;
    //  }
    //  set
    //  {
    //    _productAttributeGroupMetaDatas = value;
    //  }
    //}

    //private List<ProductAttributeGroupName> _productAttributeGroupNames;
    //private List<ProductAttributeGroupName> ProductAttributeGroupNames
    //{
    //  get
    //  {
    //    if (_productAttributeGroupNames == null)
    //    {
    //      _productAttributeGroupNames = (from pagn in concentratorContext.ProductAttributeGroupNames
    //                                     select pagn).ToList();
    //    }
    //    return _productAttributeGroupNames;
    //  }
    //  set
    //  {
    //    _productAttributeGroupNames = value;
    //  }
    //}

    //private List<ProductAttributeMetaData> _productAttributeMetaDatas;
    //private List<ProductAttributeMetaData> ProductAttributeMetaDatas
    //{
    //  get
    //  {
    //    if (_productAttributeMetaDatas == null)
    //    {
    //      _productAttributeMetaDatas = (from pamd in concentratorContext.ProductAttributeMetaDatas
    //                                    select pamd).ToList();
    //    }
    //    return _productAttributeMetaDatas;
    //  }
    //  set
    //  {
    //    _productAttributeMetaDatas = value;
    //  }
    //}

    //private List<ProductAttributeValue> _productAttributeValues;
    //private List<ProductAttributeValue> ProductAttributeValues
    //{
    //  get
    //  {
    //    if (_productAttributeValues == null)
    //    {
    //      _productAttributeValues = (from pav in concentratorContext.ProductAttributeValues
    //                                 select pav).ToList();
    //    }
    //    return _productAttributeValues;
    //  }
    //  set
    //  {
    //    _productAttributeValues = value;
    //  }
    //}

    //private List<ProductAttributeName> _productAttributeNames;
    //private List<ProductAttributeName> ProductAttributeNames
    //{
    //  get
    //  {
    //    if (_productAttributeNames == null)
    //    {
    //      _productAttributeNames = (from pan in concentratorContext.ProductAttributeNames
    //                                select pan).ToList();
    //    }
    //    return _productAttributeNames;
    //  }
    //  set
    //  {
    //    _productAttributeNames = value;
    //  }
    //}

    //private List<ProductImage> _productImages;
    //private List<ProductImage> ProductImages
    //{
    //  get
    //  {
    //    if (_productImages == null)
    //    {
    //      _productImages = (from pi in concentratorContext.ProductImages
    //                        select pi).ToList();
    //    }
    //    return _productImages;
    //  }
    //  set
    //  {
    //    _productImages = value;
    //  }
    //}
    private ConcentratorDataContext concentratorContext = new ConcentratorDataContext();
    private List<ProductAttributeGroupMetaData> _productAttributeGroupMetaDatas;
    public List<ProductAttributeGroupMetaData> ProductAttributeGroupMetaDatas
    {
      get
      {

        if (_productAttributeGroupMetaDatas == null)
        {
          _productAttributeGroupMetaDatas = (from b in concentratorContext.ProductAttributeGroupMetaDatas
                                             select b).ToList();
        }
        return _productAttributeGroupMetaDatas;
      }
      set { _productAttributeGroupMetaDatas = value; }
    }

    private List<ProductAttributeMetaData> _productAttributeMetaDatas;
    public List<ProductAttributeMetaData> ProductAttributeMetaDatas
    {
      get
      {

        if (_productAttributeMetaDatas == null)
        {
          _productAttributeMetaDatas = (from b in concentratorContext.ProductAttributeMetaDatas
                                        select b).ToList();
        }
        return _productAttributeMetaDatas;
      }
      set { _productAttributeMetaDatas = value; }
    }

    private List<ProductAttributeGroupName> _productAttributeGroupNames;
    public List<ProductAttributeGroupName> ProductAttributeGroupNames
    {
      get
      {

        if (_productAttributeGroupNames == null)
        {
          _productAttributeGroupNames = (from b in concentratorContext.ProductAttributeGroupNames
                                         select b).ToList();
        }
        return _productAttributeGroupNames;
      }
      set { _productAttributeGroupNames = value; }
    }

    private List<ProductAttributeName> _productAttributeNames;
    public List<ProductAttributeName> ProductAttributeNames
    {
      get
      {

        if (_productAttributeNames == null)
        {
          _productAttributeNames = (from b in concentratorContext.ProductAttributeNames
                                    select b).ToList();
        }
        return _productAttributeNames;
      }
      set { _productAttributeNames = value; }
    }

    //private List<ProductAttributeValue> _productAttributeValues;
    //public List<ProductAttributeValue> ProductAttributeValues
    //{
    //  get
    //  {

    //    if (_productAttributeValues == null)
    //    {
    //      _productAttributeValues = (from b in concentratorContext.ProductAttributeValues
    //                                 select b).ToList();
    //    }
    //    return _productAttributeValues;
    //  }
    //  set { _productAttributeValues = value; }
    //}



    private void ProcessProductAttributes(ICEcatDataContext ctx, string vendorItemNumber, int? supplierID, int productID, log4net.ILog log)
    {
      log.Info(
        string.Format("Started importing product attributes for product with id: {0} and Vendor Item Number : {1}", productID, vendorItemNumber));
      try
      {
        //DataLoadOptions options = new DataLoadOptions();
        //options.LoadWith<ProductAttributeGroupMetaData>(x => x.ProductAttributeGroupLabels);
        //options.LoadWith<ProductAttributeGroupMetaData>(x => x.ProductAttributes);
        //options.LoadWith<ProductAttributeMetaData>(x => x.ProductAttributeLabels);
        ////options.LoadWith<ProductAttributeMetaData>(x=>x.ProductAttributeValues);

        //_concentratorDataContext.LoadOptions = options;

         

        foreach (int languageKey in importLanguages)
        {
          var attributes = ctx.Concentrator_GetIceProductAttributebyManufactID(vendorItemNumber, supplierID,
                                                                               languageKey).ToList();

          foreach (var at in attributes)
          {
            if (!string.IsNullOrEmpty(at.Value))
            {

              #region Product Attribute Group MetaData

              ProductAttributeGroupMetaData attributeGroup = ProductAttributeGroupMetaDatas.FirstOrDefault(
                  x => x.GroupCode == at.AttributeGroupID.ToString() && x.VendorID == contentVendorID);
              //where pr.GroupCode == at.AttributeGroupID.ToString()

              if (attributeGroup == null)
              {
                attributeGroup = new ProductAttributeGroupMetaData();
                concentratorContext.ProductAttributeGroupMetaDatas.InsertOnSubmit(attributeGroup);
                ProductAttributeGroupMetaDatas.Add(attributeGroup);
              }
              attributeGroup.Index = at.AttributeGroupIndex;
              attributeGroup.GroupCode = at.AttributeGroupID.ToString();
              attributeGroup.VendorID = contentVendorID;

              #endregion

              #region Product Attribute Group Name


              ProductAttributeGroupName attributeGroupName = ProductAttributeGroupNames.FirstOrDefault(
                pag => pag.LanguageID == languageKey
                       && pag.ProductAttributeGroupID == attributeGroup.ProductAttributeGroupID
                );


              if (attributeGroupName == null)
              {
                attributeGroupName = new ProductAttributeGroupName
                                       {
                                         LanguageID = languageKey,
                                         ProductAttributeGroup = attributeGroup
                                       };
                concentratorContext.ProductAttributeGroupNames.InsertOnSubmit(attributeGroupName);
                ProductAttributeGroupNames.Add(attributeGroupName);
              }
              attributeGroupName.Name = at.AttributeGroupName;

              #endregion

              #region Product Attribute MetaData

              ProductAttributeMetaData productAttribute =
                ProductAttributeMetaDatas.FirstOrDefault(x => x.AttributeCode == at.Feature_ID.ToString() && x.ProductAttributeGroupID == attributeGroup.ProductAttributeGroupID && x.VendorID == contentVendorID);

              if (productAttribute == null)
              {
                productAttribute = new ProductAttributeMetaData()
                                     {
                                       VendorID = contentVendorID,
                                       NeedsUpdate = true,
                                       AttributeCode = at.Feature_ID.ToString(),
                                       ProductAttributeGroup = attributeGroup
                                     };

                concentratorContext.ProductAttributeMetaDatas.InsertOnSubmit(productAttribute);
                ProductAttributeMetaDatas.Add(productAttribute);

              }
              productAttribute.Sign = string.IsNullOrEmpty(at.LanguageSign) ? at.Sign : at.LanguageSign;
              productAttribute.Index = at.FeatureIndex;
              productAttribute.AttributeCode = at.Feature_ID.ToString();
              productAttribute.IsVisible = at.KeyFeature.HasValue ? at.KeyFeature.Value == 0 : false;
              productAttribute.IsSearchable = Convert.ToBoolean(at.IsSearchable.Try(c => c.Value, 0));
              productAttribute.VendorID = contentVendorID;

              #endregion

              #region Product Attribute Value

              ProductAttributeValue value =
                concentratorContext.ProductAttributeValues.FirstOrDefault(x => x.LanguageID == languageKey
                                                                            && x.ProductID == productID
                                                                            && x.AttributeID == productAttribute.AttributeID
                  );

              if (value == null)
              {
                value = new ProductAttributeValue
                          {
                            ProductID = productID,
                            ProductAttribute = productAttribute,
                            LanguageID = languageKey
                          };
                concentratorContext.ProductAttributeValues.InsertOnSubmit(value);
              }

              if (at.Value.Length > 3000)
                log.DebugFormat("ProductAttribute value length > 3000 for product {0}, attritbuteID {1}", productID, productAttribute.AttributeID);


              value.Value = at.Value.Cap(3000);

              #endregion

              #region Product Attribute Name

              ProductAttributeName attributeName =
                ProductAttributeNames.FirstOrDefault(x => x.LanguageID == languageKey
                                                                            && x.AttributeID == productAttribute.AttributeID);
              if (attributeName == null)
              {
                attributeName = new ProductAttributeName()
                                  {
                                    ProductAttribute = productAttribute,
                                    LanguageID = languageKey
                                  };
                concentratorContext.ProductAttributeNames.InsertOnSubmit(attributeName);
                ProductAttributeNames.Add(attributeName);
              }

              attributeName.Name = at.AttributeName;

              #endregion

              concentratorContext.SubmitChanges();
            }
          }
        }
      }
      catch (Exception e)
      {
        log.Error(string.Format("Importing Product Attributes failed for product with id: {0} and Vendor Item Number : {1}", productID, vendorItemNumber));
        log.Error("Import failed", e);

      }
    }

    private void ProcessProductImages(ICEcatDataContext contextIceCat, int? supplierID, string vendorItemNumber, int productID, log4net.ILog log)
    {
      try
      {
        
        var images = contextIceCat.Concentrator_GetIceProductImageUrlbyICEcatID(vendorItemNumber, supplierID, null, 1);

        int dbSequence = 1;
        foreach (var img in images)
        {
          if (!string.IsNullOrEmpty(img.Picture))
          {
            int sequence = img.BaseImage;

            ProductImage productImage = (from pi in concentratorContext.ProductImages
                                         where pi.ProductID == productID && pi.VendorID == contentVendorID
                                          && pi.Sequence == (sequence == 0 ? sequence : dbSequence)
                                          && pi.ImageUrl == img.Picture
                                         select pi).FirstOrDefault();

            if (productImage == null)
            {
              productImage = new ProductImage
                               {
                                 VendorID = contentVendorID,
                                 ProductID = productID,
                               };
              concentratorContext.ProductImages.InsertOnSubmit(productImage);
            }
            productImage.ImageUrl = img.Picture;
            productImage.Sequence = (sequence == 0 ? sequence : dbSequence);

            if (sequence != 0)
              dbSequence++;
          }
        }
        concentratorContext.SubmitChanges();
      }
      catch (Exception ex)
      {
        log.Error(string.Format("Importing Product Image failed for product with id: {0} and Vendor Item Number : {1}", productID, vendorItemNumber));
      }
    }

    #region multithread

    private void Callback(object state, log4net.ILog log)
    {
      StateInfo b = state as StateInfo;
      try
      {
        using (var ctx = new ICEcatDataContext())
        {

          log.DebugFormat("Start processing product : {0}", b.VendorItemNumber);
          var productDescriptions =
            (from pi in
               ctx.Concentrator_GetIceProductDescriptions(b.VendorItemNumber, b.IceCatSupplierID)
             select new
                      {
                        VendorItemNumber = b.VendorItemNumber,
                        BrandID = b.BrandID,
                        ProductID = b.ProductID,
                        Description = pi
                      }).ToList();



          if (productDescriptions.Count > 0)
          {
            var product = productDescriptions[0];
            ProcessProductAttributes(ctx, product.VendorItemNumber, b.IceCatSupplierID, product.ProductID, log);
            ProcessProductImages(ctx, b.IceCatSupplierID, product.VendorItemNumber, product.ProductID, log);
          }

          List<ProductDescription> descriptions = new List<ProductDescription>();

          if (productDescriptions.Count > 0)
            descriptions = concentratorContext.ProductDescription.Where(x => x.ProductID == productDescriptions.First().ProductID).
              ToList();

          foreach (var p in productDescriptions)
          {
            ProductDescription product = descriptions.FirstOrDefault(x => x.LanguageID == p.Description.LanguageID);

            try
            {


              if (product != null)
              {
                if (product.LongContentDescription != p.Description.LongDescription
                    || product.ShortContentDescription != p.Description.ShortDescription
                    || product.ShortSummaryDescription != p.Description.ShortSummaryDescription
                    || product.LongSummaryDescription != p.Description.LongSummaryDescription
                    || product.PDFUrl != p.Description.PDFUrl
                    || product.PdfSize != p.Description.PDFSize
                    || product.Url != p.Description.Url
                    || product.WarrantyInfo != p.Description.WarrentyInfo
                    || product.ModelName != p.Description.ModelName
                    || product.ProductName != p.Description.Name
                    || product.Quality != p.Description.Quality)
                {
                  product.LongContentDescription = p.Description.LongDescription;
                  product.ShortContentDescription = p.Description.ShortDescription;
                  product.ShortSummaryDescription = p.Description.ShortSummaryDescription;
                  product.LongSummaryDescription = p.Description.LongSummaryDescription;
                  product.PDFUrl = p.Description.PDFUrl;
                  product.PdfSize = p.Description.PDFSize.HasValue ? (int)p.Description.PDFSize.Value : 0;
                  product.Url = p.Description.Url;
                  product.WarrantyInfo = p.Description.WarrentyInfo;
                  product.ModelName = p.Description.ModelName;
                  product.ProductName = p.Description.Name;
                  product.Quality = p.Description.Quality;
                }
              }
              else
              {

                product = new ProductDescription
                            {
                              ProductID = p.ProductID,
                              VendorID = contentVendorID,
                              ShortContentDescription = p.Description.ShortDescription,
                              LongContentDescription = p.Description.LongDescription,
                              ShortSummaryDescription = p.Description.ShortSummaryDescription,
                              LongSummaryDescription = p.Description.LongSummaryDescription,
                              PDFUrl = p.Description.PDFUrl,
                              PdfSize = p.Description.PDFSize.HasValue ? (int)p.Description.PDFSize.Value : 0,
                              Url = p.Description.Url,
                              WarrantyInfo = p.Description.WarrentyInfo,
                              ModelName = p.Description.ModelName,
                              ProductName = p.Description.Name,
                              Quality = p.Description.Quality,
                              LanguageID = p.Description.LanguageID
                            };

                concentratorContext.ProductDescription.InsertOnSubmit(product);
                descriptions.Add(product);
              }


            }
            catch (Exception ex)
            {
              log.Error("Error import content for product " + p.VendorItemNumber, ex);
            }

            concentratorContext.SubmitChanges();
          }

        }
      }
      catch (Exception ex)
      {
        log.Error("Error importing product attribute for productID" + b.ProductID, ex);
      }
      finally
      {
        //b.ResetEvent.Set();
      }
    }

    public struct BatchResult
    {

      public BatchResult(int numberOfProducts, double executionTimeInSeconds)
      {
        NumberOfProducts = numberOfProducts;
        ExecutionTimeInSeconds = executionTimeInSeconds;
      }

      public int NumberOfProducts;
      public double ExecutionTimeInSeconds;

    }

    public class StateInfo
    {
      public string VendorItemNumber { get; set; }
      public int? IceCatSupplierID { get; set; }
      public int BrandID { get; set; }
      public int ProductID { get; set; }
      //public ICEcatDataContext ICEcatDataContext { get; set; }
      public ManualResetEvent ResetEvent { get; set; }
    }

    #endregion

  }
}
