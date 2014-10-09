using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using Concentrator.Objects.ConcentratorService.Scheduler.Provider;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Logic;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Services.DTO;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Web;
using Quartz;
using Quartz.Impl;
using System.Drawing;
using Concentrator.Objects.Models.Media;
using Concentrator.Objects.Models.WebToPrint;
using Concentrator.Objects.Models.Complex;

namespace Concentrator.Objects.Services
{
  public class ProductService : Service<Product>, IProductService
  {
    public IQueryable<Models.Products.ProductResult> GetForIpad(int vendorID)
    {
      return ((IFunctionScope)Scope).Repository().GetCatalogResult(vendorID).AsQueryable();
    }

    public void CreateProductAttribute(ProductAttributeMetaData metadata, Dictionary<string, string> names, Stream imageStream = null, string imagePath = null)
    {
      var attributeRepository = Repository<ProductAttributeName>();

      if (imageStream != null && imageStream.Length > 0) //if image is provided save it to the specified location
      {
        var length = (int)imageStream.Length;
        byte[] buffer = new byte[length];
        imageStream.Read(buffer, 0, length);
        var attributeImagePath = Path.Combine(ConfigurationManager.AppSettings["FTPMediaDirectory"], ConfigurationManager.AppSettings["AttributeImageDirectory"], imagePath);
        File.WriteAllBytes(attributeImagePath, buffer);


        metadata.AttributePath = imagePath;
      }

      metadata.AttributeCode = metadata.AttributeCode.IfNullOrEmpty(names.FirstOrDefault().Try(c => c.Value, string.Empty));

      Repository<ProductAttributeMetaData>().Add(metadata);

      names.ForEach((languageSpec, idx) =>
      {

        var lang = Repository<Language>().GetSingle(c => c.Name == languageSpec.Key);
        ProductAttributeName pan = null;
        if (lang != null)
        {
          pan = new ProductAttributeName()
          {
            Language = lang,
            ProductAttributeMetaData = metadata
          };
          attributeRepository.Add(pan);
        }

        pan.Name = languageSpec.Value;
      });
    }

    public void CreateProductAttributeGroup(ProductAttributeGroupMetaData metadata, Dictionary<string, string> names)
    {
      var groupRepository = Repository<ProductAttributeGroupName>();

      names.ForEach((languageSpec, idx) =>
      {
        var lang = Repository<Language>().GetSingle(c => c.Name == languageSpec.Key);
        if (lang != null)
        {
          groupRepository.Add(new ProductAttributeGroupName()
          {
            Name = languageSpec.Value,
            Language = lang,
            ProductAttributeGroupMetaData = metadata,

          });
        }
      });

      Repository<ProductAttributeGroupMetaData>().Add(metadata);
    }

    public IQueryable<Models.Complex.ProductSearchResult> SearchProducts(int languageID, string query, bool? includeDescriptions = false, bool? includeBrands = false, bool? includeIds = false, bool? includeProductGroups = false)
    {
      return ((IFunctionScope)Scope).Repository().SearchProducts(languageID, query, includeDescriptions, includeBrands, includeProductGroups, includeIds).AsQueryable();
    }

    public List<Models.Complex.ProductSearchResult> SearchProducts(int languageID, int connectorID, string query, bool? includeDescriptions = false, bool? includeBrands = false, bool? includeIds = false, bool? includeProductGroups = false)
    {
      var assortiment = (from p in Scope.Repository<AssortmentContentView>().GetAll(x => x.ConnectorID == connectorID)
                         select p.ProductID).ToList();

      return (from s in ((IFunctionScope)Scope).Repository().SearchProducts(languageID, query, includeDescriptions, includeBrands, includeProductGroups, includeIds).ToList()
              join a in assortiment on s.ProductID equals a
              select s).ToList();
    }

    public IQueryable<Vendor> GetContentVendors(int productID, bool? includeConcentratorVendor = true)
    {
      var res = Repository<VendorAssortment>().GetAll(c => c.ProductID == productID).Select(c => c.Vendor).Distinct().ToList();

      if (includeConcentratorVendor.HasValue && includeConcentratorVendor.Value)
      {
        res.Add(Repository<Vendor>().GetSingle(c => c.Name == "Concentrator"));
      }
      //base 
      //overlay

      var baseVendor = Repository<Vendor>().GetSingle(c => c.Name == "Base");
      if (baseVendor != null) res.Add(baseVendor);

      var overlayVendor = Repository<Vendor>().GetSingle(c => c.Name == "Overlay");
      if (overlayVendor != null) res.Add(overlayVendor);

      return res.AsQueryable();
    }

    public void PushProducts()
    {
      string jobName = ConfigurationManager.AppSettings["PushProductsJobName"];
      string groupName = ConfigurationManager.AppSettings["PushProductsJobGroupName"];

      ISchedulerProvider provider = new RemoteSchedulerProvider { SchedulerHost = "tcp://localhost:555/QuartzScheduler" };
      provider.Init();

      var properties = new NameValueCollection();
      properties["quartz.scheduler.exporter.type"] = ConfigurationManager.AppSettings["QuartzType"];
      properties["quartz.scheduler.exporter.port"] = ConfigurationManager.AppSettings["QuartzPort"];
      properties["quartz.scheduler.exporter.bindName"] = ConfigurationManager.AppSettings["QuartzBindName"];
      properties["quartz.scheduler.exporter.channelType"] = ConfigurationManager.AppSettings["QuartzChannelType"];

      ISchedulerFactory fact = new StdSchedulerFactory(properties);
      var scheduler = fact.GetScheduler();

      scheduler.TriggerJob(jobName, groupName);
    }

    public void CreateProductMedia(Stream file, string filename, string mediaurl, int productID, int typeID, int vendorID, string description, List<int> updateForMultipleIDs)
    {
      var path = ConfigurationManager.AppSettings["FTPMediaDirectory"];
      var prodPath = ConfigurationManager.AppSettings["FTPMediaProductDirectory"];
      var innerPath = Path.Combine(path, prodPath);
      var filePath = Path.Combine(prodPath, filename);
      var fullPath = Path.Combine(path, filePath);

      ProductMedia media = new ProductMedia();

      var extension = Path.GetExtension(filename);
      //no file
      if (string.IsNullOrEmpty(mediaurl))
      {
        if (!Directory.Exists(innerPath))
          Directory.CreateDirectory(innerPath);


        //copy it locally
        byte[] buff = new byte[file.Length];
        file.Read(buff, 0, (int)file.Length);
        System.IO.File.WriteAllBytes(fullPath, buff);

        //set the path
        media.MediaPath = filePath;
        file.Close();

        if (extension == "jpg" | extension == "png" | extension == "gif")
        {

          Image img = Image.FromFile(fullPath);
          media.Resolution = string.Format("{0}x{1}", img.Width, img.Height);
          media.Size = (int)Math.Round(new FileInfo(fullPath).Length / 1024d, 0);
        }
      }
      else
      {
        media.MediaUrl = mediaurl;

      }

      var MediaType = Repository<MediaType>().GetSingle(x => x.TypeID == typeID);

      media.MediaType = MediaType;
      media.ProductID = productID;
      media.VendorID = vendorID;
      media.TypeID = typeID;
      media.Description = description;

      var seq = Repository<ProductMedia>().GetAll(c => c.VendorID
                                               ==
                                               vendorID &&
                                               c.ProductID ==

                                               productID);
      int se = 0;
      if (seq.Count() > 0)
        se = seq.Try(c => c.Max(d => d.Sequence));

      //int se = Repository<ProductMedia>().GetAll(c => c.VendorID
      //                                         ==
      //                                         vendorID &&
      //                                         c.ProductID ==
      //                                         productID).Try(c => c.Max(d => d.Sequence), 0);

      media.Sequence = se;
      media.LastChanged = DateTime.Now;

      Repository<ProductMedia>().Add(media);

      if (updateForMultipleIDs != null)
      {
        foreach (var id in updateForMultipleIDs)
        {
          ProductMedia m = new ProductMedia();
          m.ProductID = id;
          m.Sequence = media.Sequence;
          m.VendorID = media.VendorID;
          m.TypeID = media.TypeID;
          m.MediaUrl = media.MediaUrl;
          m.MediaPath = media.MediaPath;
          m.Resolution = media.Resolution;
          m.Size = media.Size;
          m.Description = media.Description;
          m.FileName = media.FileName;
          m.LastChanged = DateTime.Now;
          Repository<ProductMedia>().Add(m);

        }
      }

    }

    public void CreateProductMediaByUrl(string mediaurl, int productID, int typeID, int vendorID, string description, List<int> updateForMultipleIDs = null)
    {
      ProductMedia media = new ProductMedia();

      media.MediaUrl = mediaurl;

      var MediaType = Repository<MediaType>().GetSingle(x => x.TypeID == typeID);

      media.MediaType = MediaType;
      media.ProductID = productID;
      media.VendorID = vendorID;
      media.TypeID = typeID;
      media.Description = description;

      int se = Repository<ProductMedia>().GetAll(c => c.VendorID
                                               ==
                                               vendorID &&
                                               c.ProductID ==
                                               productID).Try(c => c.Max(d => d.Sequence), 0);

      media.Sequence = se;

      Repository<ProductMedia>().Add(media);

      if (updateForMultipleIDs != null)
      {
        foreach (var id in updateForMultipleIDs)
        {
          ProductMedia m = new ProductMedia();
          m = media;
          m.ProductID = id;
          Repository<ProductMedia>().Add(m);
        }
      }

    }

    public void UpdateProductMedia(int MediaID, int ProductID, int VendorID, int sequence_new, int sequence_old, int TypeID, string MediaPath, string MediaUrl, string Description, int TypeID_Old)
    {
      var media = Repository<ProductMedia>().GetSingle(m => m.MediaID == MediaID);

      var sharedMedia = Repository<ProductMedia>().GetAll(m => m.ProductID == ProductID && m.TypeID == TypeID_Old && m.VendorID == VendorID);
      var upgradeSequenceList = sharedMedia.Where(m => m.Sequence >= sequence_new && m.Sequence < sequence_old);

      foreach (var item in upgradeSequenceList)
      {
        item.Sequence += 1;
      }

      media.Sequence = sequence_new;
      media.MediaPath = MediaPath;
      media.MediaUrl = MediaUrl;
      media.Description = Description;
      media.TypeID = TypeID;
    }

    public void DeleteProductMedia(int mediaID)
    {
      var media = Repository<ProductMedia>().GetSingle(x => x.MediaID == mediaID);
      string mediaPath = Path.Combine(ConfigurationManager.AppSettings["FTPMediaDirectory"], media.MediaPath);

      if (System.IO.File.Exists(mediaPath))
      {
        System.IO.File.Delete(mediaPath);
      }
    }

    public void CreateMatchForProduct(int productID, int correspondingProductID, int productMatchID)
    {
      var existingMatch = (Repository<ProductMatch>().GetSingle(x => x.ProductID == productID && x.ProductMatchID == productMatchID));

      existingMatch.isMatched = true;
      existingMatch.MatchStatus = 2;
    }

    public void RemoveMatchForProduct(int correspondingProductID, int productMatchID)
    {
      var existingMatch = (Repository<ProductMatch>().GetSingle(x => x.ProductID == correspondingProductID && x.ProductMatchID == productMatchID));

      existingMatch.isMatched = false;
      existingMatch.MatchStatus = 3;
    }

    public void CopyProductDescription(int vendorID, int productID, int languageID)
    {
      var desc = Repository<ProductDescription>().GetSingle(c => c.ProductID == productID && c.LanguageID == languageID && c.VendorID == vendorID);


      if (desc != null)
        Repository<ProductDescription>().Delete(desc);

      var origDesc = Repository<ProductDescription>().GetSingle(c => c.ProductID == productID && c.LanguageID == languageID && c.VendorID == vendorID);

      if (origDesc == null)
      {
        throw new InvalidOperationException("Product description cannot be null");
      }

      var newDesc = new ProductDescription();

      //TryMap(newDesc, origDesc);

      newDesc.LongContentDescription = origDesc.LongContentDescription;
      newDesc.LongSummaryDescription = origDesc.LongSummaryDescription;
      newDesc.ModelName = origDesc.ModelName;
      newDesc.PDFSize = origDesc.PDFSize;
      newDesc.PDFUrl = origDesc.PDFUrl;
      newDesc.ProductName = origDesc.ProductName;
      newDesc.Quality = origDesc.Quality;
      newDesc.ShortContentDescription = origDesc.ShortContentDescription;
      newDesc.ShortSummaryDescription = origDesc.ShortSummaryDescription;
      newDesc.Url = origDesc.Url;
      newDesc.WarrantyInfo = origDesc.WarrantyInfo;

      newDesc.VendorID = vendorID;
      newDesc.LanguageID = languageID;
      newDesc.ProductID = productID;

      Repository<ProductDescription>().Add(newDesc);
    }

    public Dictionary<Language, int> GetMissingLongDescriptionsCount(int[] connectors = null, int[] vendors = null, DateTime? beforeDate = null, DateTime? afterDate = null, DateTime? onDate = null, bool? isActive = null, int[] productGroups = null, int[] brands = null, int? lowerStockQuantity = null, int? greaterStockQuantity = null, int? equalStockQuantity = null, int[] statuses = null, int? descriptionVendorID = null)
    {
      var missingContent = Repository<MissingContent>().GetAll();

      if (connectors != null && connectors.Count() > 0)
        missingContent = missingContent.Where(c => connectors.Contains(c.ConnectorID));

      else
        missingContent = missingContent.Where(c => c.ConnectorID == Client.User.ConnectorID);

      if (isActive.HasValue)
        missingContent = missingContent.Where(c => c.isActive == isActive);

      if (vendors != null && vendors.Count() > 0)
        missingContent = missingContent.Where(c => c.ContentVendorID.HasValue && vendors.Contains(c.ContentVendorID.Value));
      else
      {
        //if (Client.User.ConnectorID.HasValue)
        //{
        //  vendors = Repository<ContentVendorSetting>().GetAll(x => x.ConnectorID == Client.User.ConnectorID.Value).Select(x => x.VendorID).ToArray();
        //  missingContent = missingContent.Where(c => c.ContentVendorID.HasValue && vendors.Contains(c.ContentVendorID.Value));
        //}
      }

      if (brands != null && brands.Count() > 0)
      {
        var br = Repository<Brand>().GetAll().Where(c => brands.Any(m => m == c.BrandID)).Select(c => c.Name);
        missingContent = missingContent.Where(c => brands.Any(m => m == c.BrandID));
      }

      if (beforeDate.HasValue && afterDate.HasValue)
      {
        missingContent = missingContent.Where(x => x.CreationTime < beforeDate.Value && x.CreationTime > afterDate.Value);
      }

      if (beforeDate.HasValue)
      {
        DateTime beginOfDay = new DateTime(beforeDate.Value.Year, beforeDate.Value.Month, beforeDate.Value.Day, 0, 0, 0);
        missingContent = missingContent.Where(x => x.CreationTime < beginOfDay);
      }

      if (afterDate.HasValue)
      {
        DateTime beginOfDay = new DateTime(afterDate.Value.Year, afterDate.Value.Month, afterDate.Value.Day, 0, 0, 0).AddDays(1);
        missingContent = missingContent.Where(x => x.CreationTime > beginOfDay);
      }

      if (onDate.HasValue)
      {
        DateTime beginOfDay = new DateTime(onDate.Value.Year, onDate.Value.Month, onDate.Value.Day, 0, 0, 0);
        DateTime endOfDay = beginOfDay.AddDays(1);
        missingContent = missingContent.Where(x => x.CreationTime > beginOfDay && x.CreationTime < endOfDay);
      }

      if (productGroups != null && productGroups.Count() > 0)
      {
        missingContent = missingContent.Where(c => productGroups.Any(m => ((c.ProductGroupID == m))));
      }

      if (statuses != null && statuses.Count() != 0)
      {
        missingContent = missingContent.Where(c => c.Vendor.VendorStocks.Any(s => (s.ProductID == c.Product.ProductID) && ((s.ConcentratorStatusID.HasValue) ? statuses.Contains(s.ConcentratorStatusID.Value) : false)));
      }

      var assortmentContentView = Repository<AssortmentContentView>().GetAll();

      if (lowerStockQuantity.HasValue)
      {
        var tempMissingContent = missingContent.Join(
          assortmentContentView,
          mc => mc.Product.ProductID,
          ac => ac.ProductID,
          (mc, ac) => new { MissingContent = mc, AssortmentContentView = ac });

        missingContent = tempMissingContent.Where(
          c => (
            c.MissingContent.ConnectorID == c.AssortmentContentView.ConnectorID)
            && (c.AssortmentContentView.QuantityOnHand < lowerStockQuantity.Value))
            .Select(c => c.MissingContent);
        //missingContent.Where(x => x.QuantityOnHand < lowerStockQuantity.Value);
      }

      if (greaterStockQuantity.HasValue)
      {
        var tempMissingContent = missingContent.Join(
          assortmentContentView,
          mc => mc.Product.ProductID,
          ac => ac.ProductID,
          (mc, ac) => new { MissingContent = mc, AssortmentContentView = ac });

        missingContent = tempMissingContent.Where(
          c => (c.MissingContent.ConnectorID == c.AssortmentContentView.ConnectorID)
            && (c.AssortmentContentView.QuantityOnHand > greaterStockQuantity.Value))
            .Select(c => c.MissingContent);
        //missingContent = missingContent.Where(x => x.QuantityOnHand > greaterStockQuantity.Value);
      }

      if (equalStockQuantity.HasValue)
      {
        var tempMissingContent = missingContent.Join(
          assortmentContentView,
          mc => mc.Product.ProductID,
          ac => ac.ProductID,
          (mc, ac) => new { MissingContent = mc, AssortmentContentView = ac });

        missingContent = tempMissingContent.Where(
          c => (c.MissingContent.ConnectorID == c.AssortmentContentView.ConnectorID)
            && (c.AssortmentContentView.QuantityOnHand == equalStockQuantity.Value))
            .Select(c => c.MissingContent);
        //missingContent = missingContent.Where(x => x.QuantityOnHand == equalStockQuantity.Value);
      }

      var filtered = (from m in missingContent
                      select m.ConcentratorProductID).ToList();

      filtered.Sort();


      Dictionary<Language, int> languageCounter = new Dictionary<Models.Localization.Language, int>();
      var langRepo = Repository<Language>();
      if (filtered.Count > 0)
      {
        if ((connectors != null && connectors.Count() > 0)
          || (isActive.HasValue)
          || (vendors != null && vendors.Count() > 0)
          || (brands != null && brands.Count() > 0)
          || (beforeDate.HasValue)
          || (afterDate.HasValue)
          || (onDate.HasValue)
          || (productGroups != null && productGroups.Count() > 0)
          || (statuses != null && statuses.Count() != 0)
          || (lowerStockQuantity.HasValue)
          || (greaterStockQuantity.HasValue)
          || (equalStockQuantity.HasValue))
        {
          //var descriptions = Repository<ProductDescription>().GetAll(c => (c.LongContentDescription == null || c.LongContentDescription == "")).Select(x => new { x.LanguageID, x.ProductID }).ToList();



          //var languageCount = (from l in descriptions
          //                     join f in filtered on l.ProductID equals f
          //                     group l by l.LanguageID into grouped
          //                     select new
          //                     {
          //                       LanguageID = grouped.Key,
          //                       Counter = grouped.Count()
          //                     }).ToList();

          var descriptions = Repository<ProductDescription>().GetAll().Select(x => new { x.LanguageID, x.ProductID, x.LongContentDescription }).ToList();


          var languageCount = (from c in filtered
                               join j in descriptions on c equals j.ProductID into descJoin
                               from b in descJoin.DefaultIfEmpty()
                               where (b.LongContentDescription == null || b.LongContentDescription == "")
                               group b by b.LanguageID into grouped
                               select new
                                       {
                                         LanguageID = grouped.Key,
                                         Counter = grouped.Count()
                                       }).ToList();


          langRepo.GetAll().ToList().ForEach(m =>
         {
           var lc = languageCount.FirstOrDefault(x => x.LanguageID == m.LanguageID);

           var count = (lc != null ? lc.Counter : 0);
           languageCounter.Add(m, count);
         });
        }
        else
        {
          ((IFunctionScope)Scope).Repository().GetLanguageDescriptionCount(Client.User.ConnectorID.Value, descriptionVendorID).ForEach(m =>
                    {
                      languageCounter.Add(langRepo.GetSingle(c => c.LanguageID == m.LanguageID), m.Counter);
                    });
        }
      }

      return languageCounter;
    }




    public void MergeProductDescription(Models.Products.ProductDescription pd)
    {
      var descriptionRepo = Repository<ProductDescription>();

      var productDescr = descriptionRepo.GetSingle(c => c.ProductID == pd.ProductID && c.VendorID == pd.VendorID && c.LanguageID == pd.LanguageID);
      if (productDescr == null)
      {
        productDescr = new ProductDescription()
        {
          ProductID = pd.ProductID,
          VendorID = pd.VendorID,
          LanguageID = pd.LanguageID
        };
        descriptionRepo.Add(productDescr);
      }

      productDescr.LongContentDescription = pd.LongContentDescription;
      productDescr.LongSummaryDescription = pd.LongSummaryDescription;
      productDescr.ModelName = pd.ModelName;
      productDescr.PDFSize = pd.PDFSize;
      productDescr.PDFUrl = pd.PDFUrl;
      productDescr.ProductName = pd.ProductName;
      productDescr.Quality = pd.Quality;
      productDescr.ShortContentDescription = pd.ShortContentDescription;
      productDescr.ShortSummaryDescription = pd.ShortSummaryDescription;
      productDescr.Url = pd.Url;
      productDescr.WarrantyInfo = pd.WarrantyInfo;

    }

    public void SetOrUpdateProductDescription(Product p, bool propagate, string longContentDescription, int? pdfSize, string pdfUrl, string productName, string quality, string shortContentDescription, string url, int vendorID, int languageID)
    {
      var descriptionRepo = Repository<ProductDescription>();

      var productDescr = descriptionRepo.GetSingle(c => c.ProductID == p.ProductID && c.VendorID == vendorID && c.LanguageID == languageID);
      if (productDescr == null)
      {
        productDescr = new ProductDescription()
        {
          ProductID = p.ProductID,
          VendorID = vendorID,
          LanguageID = languageID
        };
        descriptionRepo.Add(productDescr);
      }

      productDescr.LongContentDescription = longContentDescription;
      productDescr.PDFSize = pdfSize;
      productDescr.PDFUrl = pdfUrl;
      productDescr.ProductName = productName;
      productDescr.Quality = quality;
      productDescr.ShortContentDescription = shortContentDescription;
      productDescr.Url = url;

      p.LastModificationTime = DateTime.Now;
      p.LastModifiedBy = Client.User.UserID;

      if (propagate)
      {
        foreach (var product in p.ChildProducts)
        {
          SetOrUpdateProductDescription(product, propagate, longContentDescription, pdfSize, pdfUrl, productName, quality, shortContentDescription, url, vendorID, languageID);
        }
      }
    }

    public void SetOrUpdateAttributeOption(Product p, int AttributeID, int? AttributeOptionID, bool propagate)
    {
      var productAttributeValueRepo = Repository<ProductAttributeValue>();

      var productAttributeValueMaterial = Repository<ProductAttributeValue>().GetSingle(x => x.ProductID == p.ProductID && x.AttributeID == AttributeID);

      if (AttributeOptionID.HasValue)
      {
        if (productAttributeValueMaterial == null)
        {
          ProductAttributeValue newMaterialAttributeValue = new ProductAttributeValue()
          {
            AttributeID = AttributeID,
            ProductID = p.ProductID,
            Value = AttributeOptionID.ToString()
          };

          productAttributeValueRepo.Add(newMaterialAttributeValue);
        }
        else
        {
          productAttributeValueMaterial.Value = AttributeOptionID.ToString();
        }
      }
      else
      {
        if (productAttributeValueMaterial != null)
          productAttributeValueRepo.Delete(productAttributeValueMaterial);
      }


      if (propagate)
      {
        foreach (var product in p.ChildProducts.Where(c => c.IsConfigurable))
        {
          SetOrUpdateAttributeOption(product, AttributeID, AttributeOptionID, propagate);
        }
      }
    }



    public void SetOrUpdateAttributeValue(Product p, int AttributeID, string AttributeValue, bool propagate, int? languageID = null)
    {
      var productAttributeValueRepo = Repository<ProductAttributeValue>();

      var productAttributeValueMaterial = Repository<ProductAttributeValue>().GetSingle(x => x.ProductID == p.ProductID && x.AttributeID == AttributeID && (languageID.HasValue ? (x.LanguageID == languageID.Value) : !x.LanguageID.HasValue));
      if (productAttributeValueMaterial == null)
      {
        ProductAttributeValue newMaterialAttributeValue = new ProductAttributeValue()
        {
          AttributeID = AttributeID,
          ProductID = p.ProductID,
          Value = AttributeValue
        };
        if (languageID.HasValue)
          newMaterialAttributeValue.LanguageID = languageID.Value;

        productAttributeValueRepo.Add(newMaterialAttributeValue);
      }
      else
      {
        productAttributeValueMaterial.Value = AttributeValue;
      }

      if (propagate)
      {
        foreach (var product in p.ChildProducts.Where(x => x.IsConfigurable))
        {
          SetOrUpdateAttributeValue(product, AttributeID, AttributeValue, propagate, languageID);
        }
      }
    }


    public void MergeProductDescriptionByID(Models.Products.ProductDescription pd, int productID)
    {
      var descriptionRepo = Repository<ProductDescription>();

      var productDescr = descriptionRepo.GetSingle(c => c.ProductID == productID && c.VendorID == pd.VendorID && c.LanguageID == pd.LanguageID);
      if (productDescr == null)
      {
        productDescr = new ProductDescription()
        {
          ProductID = productID,
          VendorID = pd.VendorID,
          LanguageID = pd.LanguageID
        };
        descriptionRepo.Add(productDescr);
      }

      productDescr.LongContentDescription = pd.LongContentDescription;
      productDescr.LongSummaryDescription = pd.LongSummaryDescription;
      productDescr.ModelName = pd.ModelName;
      productDescr.PDFSize = pd.PDFSize;
      productDescr.PDFUrl = pd.PDFUrl;
      productDescr.ProductName = pd.ProductName;
      productDescr.Quality = pd.Quality;
      productDescr.ShortContentDescription = pd.ShortContentDescription;
      productDescr.ShortSummaryDescription = pd.ShortSummaryDescription;
      productDescr.Url = pd.Url;
      productDescr.WarrantyInfo = pd.WarrantyInfo;

    }

    public void CreateProductGroup(Concentrator.Objects.Models.Products.ProductGroup pg, Dictionary<string, string> names)
    {
      var groupRepository = Repository<ProductGroupLanguage>();

      names.ForEach((languageSpec, idx) =>
      {
        var lang = Repository<Language>().GetSingle(c => c.Name == languageSpec.Key);

        if (groupRepository.GetSingle(c => c.Name == languageSpec.Value && c.LanguageID == lang.LanguageID) != null)
          throw new InvalidOperationException("Can't create a product group. Name already exists");

        if (lang != null)
        {
          groupRepository.Add(new ProductGroupLanguage()
          {
            Language = lang,
            ProductGroup = pg,
            Name = languageSpec.Value
          });
        }
      });

      Repository<ProductGroup>().Add(pg);
    }

    public ProductDetailDto getProductDetailsBySingleID(int productID)
    {
      int connectorID = Client.User.ConnectorID.Value;
      int languageID = Client.User.LanguageID;
      ProductDetailDto dto = new ProductDetailDto();

      var productFromTable = Repository().Include(c => c.ProductDescriptions, c => c.VendorAssortments, c => c.VendorStocks).GetAllAsQueryable(c => c.ProductDescriptions.Any(l => l.ProductID == productID)).ToList();



      string shortContDes = "";
      string longcontDes = "";
      string shortSumDes = "";
      string longSummDes = "";
      string modName = "";

      productFromTable.ForEach((p, id) =>
        {

          //descriptions
          shortContDes = p.ProductDescriptions.FirstOrDefault(c => c.LanguageID == languageID).ShortContentDescription; //dit geeft soms ook een error
          //if (string.IsNullOrEmpty(shortContDes))
          //{
          //  shortContDes = p.ProductDescriptions.FirstOrDefault(c => c.LanguageID == 2).ShortContentDescription;
          //  if (string.IsNullOrEmpty(shortContDes))
          //    shortContDes = p.ProductDescriptions.FirstOrDefault(c => c.LanguageID == 1).ShortContentDescription;
          //}


          longcontDes = p.ProductDescriptions.FirstOrDefault(c => c.LanguageID == languageID).LongContentDescription;
          //if (string.IsNullOrEmpty(shortContDes))
          //{
          //  longcontDes = p.ProductDescriptions.FirstOrDefault(c => c.LanguageID == 2).LongContentDescription;
          //  if (string.IsNullOrEmpty(shortContDes))
          //    longcontDes = p.ProductDescriptions.FirstOrDefault(c => c.LanguageID == 1).LongContentDescription;
          //}



          shortSumDes = p.ProductDescriptions.FirstOrDefault(c => c.LanguageID == languageID).ShortContentDescription;
          //if (string.IsNullOrEmpty(shortContDes))
          //{
          //  shortSumDes = p.ProductDescriptions.FirstOrDefault(c => c.LanguageID == 2).ShortContentDescription;
          //  if (string.IsNullOrEmpty(shortContDes))
          //    shortSumDes = p.ProductDescriptions.FirstOrDefault(c => c.LanguageID == 1).ShortContentDescription;
          //}


          longSummDes = p.ProductDescriptions.FirstOrDefault(c => c.LanguageID == languageID).LongSummaryDescription;
          //if (string.IsNullOrEmpty(longSummDes))
          //{
          //  longSummDes = p.ProductDescriptions.FirstOrDefault(c => c.LanguageID == 2).LongSummaryDescription;
          //  if (string.IsNullOrEmpty(shortContDes))
          //    longSummDes = p.ProductDescriptions.FirstOrDefault(c => c.LanguageID == 1).LongSummaryDescription;
          //}


          //modelname
          modName = p.ProductDescriptions.FirstOrDefault(c => c.LanguageID == languageID).ModelName;

        });

      var productDescription = Repository<ProductDescription>().GetSingle(a => a.ProductID == productID && a.LanguageID == languageID);
      var assortment2 = Repository<AssortmentContentView>().GetAll(x => x.ProductID == productID);
      var assortment = assortment2.FirstOrDefault();


      dto.ProductID = productID;

      //descriptions
      if (productDescription == null)
      {
        //check for default language
        var productDescription2 = Repository<ProductDescription>().GetSingle(a => a.ProductID == productID);

        if (productDescription2 != null)
        {
          dto.ShortDescription = productDescription2.ShortContentDescription;
          dto.LongDescription = productDescription2.LongContentDescription;
        }
        else//lool in assortmentontentview
        {
          var productDescription3 = Repository<AssortmentContentView>().GetSingle(w => w.ProductID == productID);
          if (productDescription3 != null)
          {
            dto.ShortDescription = productDescription3.ShortDescription;
            dto.LongDescription = productDescription3.LongDescription;
          }

        }

      }
      else
      {
        //string longDes = productDescription.LongContentDescription;
        dto.ShortDescription = productDescription.ShortContentDescription != null ? productDescription.ShortContentDescription : productDescription.ShortSummaryDescription;
        dto.LongDescription = productDescription.LongContentDescription == null ? " " : productDescription.LongContentDescription.Replace("\\n", "<br />");
      }

      //
      //extra description
      var extr = Repository<VendorAssortment>().GetSingle(w => w.ProductID == productID);
      if (extr != null)
      {
        var longDes = (!string.IsNullOrEmpty(extr.LongDescription)) ? extr.LongDescription : " ";
        dto.ExtraDescription = (!string.IsNullOrEmpty(extr.ShortDescription)) ? extr.ShortDescription : longDes;
      }
      else
        dto.ExtraDescription = "";

      //
      //brandname and modelname
      if (assortment == null)
      {
        dto.BrandName = "";
        dto.ModelName = "";
      }
      else
      {
        dto.BrandName = assortment.BrandName;
        dto.ModelName = assortment.CustomItemNumber;
      }

      //productgroup
      //var tempProduct = Scope.Repository<ContentProductGroup>().GetSingle(t => t.ConnectorID == connectorID && t.ProductID == productID);
      //connectorid hoeft niet?
      var tempProduct = Scope.Repository<ContentProductGroup>().GetSingle(t => t.ProductID == productID);

      if (tempProduct == null)
        dto.ProductGroup = "";
      else
      {
        var tempProduct2 = Scope.Repository<ProductGroupMapping>().GetSingle(q => q.ProductGroupMappingID == tempProduct.ProductGroupMappingID);
        if (tempProduct2 == null)
          dto.ProductGroup = "";
        else
        {
          var productGroup = Scope.Repository<ProductGroupLanguage>().GetSingle(r => r.ProductGroupID == tempProduct2.ProductGroupID && r.LanguageID == languageID);
          var productGroup2 = Scope.Repository<ProductGroupLanguage>().GetSingle(r => r.ProductGroupID == tempProduct2.ProductGroupID).Name;

          if (productGroup == null) //supllied language doesnt excists
            dto.ProductGroup = productGroup2;
          else
            dto.ProductGroup = productGroup.Name;
        }
      }

      //barcode
      List<String> stringList = new List<String>();
      var barCodes = Repository<ProductBarcode>().GetAll(r => r.ProductID == productID);
      foreach (var item2 in barCodes)
        stringList.Add(item2.Barcode);
      dto.Barcode = string.Join(", ", stringList);

      //attributes
      dto.AttributeNameValueList = new List<AttributeNameAndValueDto>();
      //to do: language id is not set
      var attributesValues = Repository<ProductAttributeValue>().GetAll(d => d.ProductID == productID && d.LanguageID == languageID);

      foreach (var a in attributesValues)
      {
        //to do: change Issearchable == true
        var temp = Repository<ProductAttributeMetaData>().GetSingle(e => e.AttributeID == a.AttributeID && e.IsSearchable == false);
        if (temp != null)
        {
          AttributeNameAndValueDto av = new AttributeNameAndValueDto();
          //to do: language id is not set

          av.AttributeName = Repository<ProductAttributeName>().GetSingle(w => w.AttributeID == a.AttributeID && w.LanguageID == languageID).Name;
          av.AttributeValue = a.Value;
          av.AttributeValueID = a.AttributeValueID;
          av.ImagePath = a.ProductAttributeMetaData.AttributePath;
          dto.AttributeNameValueList.Add(av);

        }

      }
      return dto;
    }


    public List<ProductDto> GetProductDetailsByIDs(int[] IDlist, int connectorID)
    {
      List<ProductDto> productDtos = new List<ProductDto>();

      List<AssortmentContentView> content = Repository<AssortmentContentView>().GetAll(c => c.ConnectorID == connectorID && IDlist.Contains(c.ProductID)).ToList();

      content.ForEach((p, id) =>
      {

        var d = new ProductDto()
                   {
                     Name = p.ShortDescription,
                     ProductID = p.ProductID,
                     Description = p.LongDescription,
                     Brand = new BrandDTO
                     {
                       BrandID = p.BrandID,
                       Name = p.BrandName
                     }
                   };
        productDtos.Add(d);

      });
      return productDtos;
    }
    public List<ProductDto> GetByProductGroupMapping(int? productGroupMappingID = null, string lineage = null, int? connectorID = null)
    //public List<ProductDto> GetByProductGroupMapping(int? productGroupMappingID = null, string lineage = null, int? connectorID = null, int ? itemsPerPage = null, int ? pageNumber =null)
    {
      if (!connectorID.HasValue) connectorID = Client.User.ConnectorID;
      if (!productGroupMappingID.HasValue && string.IsNullOrEmpty(lineage))
      {
        throw new InvalidOperationException("Lineage or product group mapping must be supplied");
      }

      List<Product> products = null;

      if (!productGroupMappingID.HasValue)
      {
        var lineageAr = lineage.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        productGroupMappingID = int.Parse(lineageAr.Last());
      }

      products = Repository().Include(c => c.VendorAssortments, c => c.ProductDescriptions).GetAllAsQueryable(c => c.ContentProductGroups.Any(l => l.ConnectorID == connectorID && l.ProductGroupMappingID == productGroupMappingID.Value)).ToList();

      //var count = products.Count;
      //if (itemsPerPage != null && pageNumber != null)
      //{
      //  products = products.GetRange(0, 10);
      //  products = Repository().Include(c => c.VendorAssortments, c => c.ProductDescriptions).GetAllAsQueryable(c => c.ContentProductGroups.Any(l => l.ConnectorID == connectorID && l.ProductGroupMappingID == productGroupMappingID.Value)).Skip(((int)pageNumber - 1)).Take(itemsPerPage).ToList();
      //}
      //else
      //  products = Repository().Include(c => c.VendorAssortments, c => c.ProductDescriptions).GetAllAsQueryable(c => c.ContentProductGroups.Any(l => l.ConnectorID == connectorID && l.ProductGroupMappingID == productGroupMappingID.Value)).ToList();

      var connector = Repository<Connector>().GetSingle(c => c.ConnectorID == Client.User.ConnectorID);

      ContentLogic logic = new ContentLogic(Scope, Client.User.ConnectorID.Value);

      List<ProductDto> productDtos = new List<ProductDto>();
      products.ForEach((p, id) =>
      {
        //zorg eerst de product naam, en daarna de product description


        var desc = p.ProductDescriptions.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID);
        var desc2 = p.ProductDescriptions.FirstOrDefault();

        string description = "";
        string productName = "";

        if (desc != null)
        {
          description = desc.ShortContentDescription != null ? desc.ShortContentDescription : desc.ShortSummaryDescription;
          productName = desc.ProductName;
        }
        else if (desc2 != null)
        {
          description = desc2.ShortContentDescription != null ? desc2.ShortContentDescription : desc2.ShortSummaryDescription;
          productName = desc2.ProductName;
        }

        var shortDescription = p.VendorAssortments.FirstOrDefault().ShortDescription;

        productName = string.IsNullOrEmpty(productName) ? shortDescription : productName;


        var stockk = Repository<VendorStock>().GetSingle(s => s.ProductID == p.ProductID);


        var d = new ProductDto()
        {
          Name = productName, //kijk eerst of het een naam heeft, en gebruik anders shortdescription
          ProductID = p.ProductID,
          //Description = desc.Try(c => c.Description, shortDescription),
          Description = description,
          Price = logic.CalculatePrice(p.ProductID, 1, connector, Enumerations.PriceRuleType.UnitPrice),
          Brand = new BrandDTO
          {
            BrandID = p.BrandID,
            Name = p.Brand.Name
          }
        };
        productDtos.Add(d);
      });

      return productDtos;
    }

    public void SetProductGroupTranslations(int languageID, int productGroupID, string name)
    {

      var langRepo = Repository<ProductGroupLanguage>();

      var productGroupLanguage = langRepo.GetSingle(c => c.LanguageID == languageID && c.ProductGroupID == productGroupID);

      if (productGroupLanguage == null)
      {
        productGroupLanguage = new Concentrator.Objects.Models.Products.ProductGroupLanguage()
        {
          LanguageID = languageID,
          ProductGroupID = productGroupID

        };
        langRepo.Add(productGroupLanguage);
      }
      productGroupLanguage.Name = name;
    }

    public void SetAttributeTranslations(int languageID, int attributeID, string name)
    {

      var langRepo = Repository<ProductAttributeName>();

      var attributeName = langRepo.GetSingle(c => c.LanguageID == languageID && c.AttributeID == attributeID);

      if (attributeName == null)
      {
        attributeName = new ProductAttributeName
        {
          LanguageID = languageID,
          AttributeID = attributeID
        };
        langRepo.Add(attributeName);
      }
      attributeName.Name = name;
    }

    public ContentLogic FillPriceInformation(int connectorID)
    {
      //if (HttpContext.Current != null)
      //{
      //  if (HttpContext.Current.Cache["CalculatedPriceView"] != null)
      //  {
      //    return ((ContentLogic)HttpContext.Current.Cache["CalculatedPriceView"]);
      //  }
      //}

      ContentLogic logic = new ContentLogic(this.Scope, connectorID);
      var funcRepo = ((IFunctionScope)this.Scope).Repository();

      logic.FillPriceInformation(funcRepo.GetCalculatedPriceView(connectorID));

      //try
      //{
      //  HttpContext.Current.Cache.Remove("CalculatedPriceView");

      //  HttpContext.Current.Cache.Insert("CalculatedPriceView", logic,
      //          null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 1, 0));
      //}
      //catch (Exception ex)
      //{

      //}

      return logic;
    }

    public void CreateProductAttributeValueGroup(ProductAttributeValueGroup group, Dictionary<string, string> names)
    {
      Repository<ProductAttributeValueGroup>().Add(group);
      var groupNameRepo = Repository<ProductAttributeValueGroupName>();
      
      //if any group with this name exists stop
      foreach (var lan in names)
      {
        if (groupNameRepo.GetSingle(c => c.Name.ToLower() == lan.Value && c.ProductAttributeValueGroup.ConnectorID == Client.User.ConnectorID) != null)
          throw new InvalidOperationException("Group with the same name already exists");
      }

      if (!group.ConnectorID.HasValue && Client.User.ConnectorID.HasValue)
      {
        group.ConnectorID = Client.User.ConnectorID.Value;
      }
      names.ForEach((languageSpec, idx) =>
      {
        var lang = Repository<Language>().GetSingle(c => c.Name == languageSpec.Key);
        ProductAttributeValueGroupName pan = null;
        if (lang != null)
        {
          pan = new ProductAttributeValueGroupName()
          {
            Language = lang,
            ProductAttributeValueGroup = group
          };
          groupNameRepo.Add(pan);
        }
        pan.Name = languageSpec.Value;
      });
    }



    //public void SetAttributeValueTranslations(int languageID, int attributeValueID, string name)
    //{
    //  var langRepo = Repository<ProductAttributeValueTranslation>();

    //  var attributeName = langRepo.GetSingle(c => c.LanguageID == languageID && c.AttributeID == attributeID && c.AttributeValueID == attributeValueID);

    //  if (attributeName == null)
    //  {
    //    attributeName = new ProductAttributeValueTranslation
    //    {
    //      LanguageID = languageID,
    //      AttributeID = attributeID,
    //      AttributeValueID = attributeValueID
    //    };
    //    langRepo.Add(attributeName);
    //  }
    //  attributeName.Translation = name;
    //}

    public void SetAttributeValueTranslations(int languageID, int attributeID, int connectorID, string translation, string Value)
    {
      //todoattribute

      var langRepo = Repository<ProductAttributeValueLabel>();
      var attributeName = langRepo.GetSingle(c =>
        c.LanguageID == languageID
        && c.Value == Value
        && c.ConnectorID == connectorID
        && c.AttributeID == attributeID);

      if (attributeName == null)
      {
        attributeName = new ProductAttributeValueLabel
        {
          LanguageID = languageID,
          Value = Value,
          ConnectorID = connectorID,
          AttributeID = attributeID
        };
        langRepo.Add(attributeName);
      }

      if (string.IsNullOrEmpty(translation))
      {
        langRepo.Delete(attributeName);
      }
      else
      {
        attributeName.Label = translation;
      }
    }

    public void RemoveFromConnectorPublication(string vendorItemNumber, int connectorID, bool propagate = true)
    {
      var productRepo = Repository<Product>();
      List<Product> productsToAddToConnectorPublication = new List<Product>();

      var product = productRepo.GetSingle(c => c.VendorItemNumber == vendorItemNumber);

      if (product == null) throw new InvalidOperationException("Product can't be found");

      productsToAddToConnectorPublication.Add(productRepo.GetSingle(c => c.VendorItemNumber == vendorItemNumber));

      if (propagate)
      {
        productsToAddToConnectorPublication.AddRange(Repository<Product>().GetAll(c => c.ParentProductID == product.ProductID).ToList());
      }

      var baseContentProductRecord = Repository<ContentProduct>().GetAll(c => c.ConnectorID == connectorID).FirstOrDefault(); //get first vendorid for the specified connector
      if (baseContentProductRecord == null) throw new InvalidOperationException("Base content product can't be found. Be sure to specify at least one rule with a ConnectorID, VendorID combo");

      var connectorPublicationRepo = Repository<ConnectorPublication>();

      foreach (var p in productsToAddToConnectorPublication)
      {
        var contentPublication = connectorPublicationRepo.GetSingle(c => c.ConnectorID == connectorID && c.VendorID == baseContentProductRecord.VendorID && c.ProductID == p.ProductID);
        connectorPublicationRepo.Delete(contentPublication);
      }
    }


    public void AddToConnectorPublication(string vendorItemNumber, int connectorID, bool propagate = true)
    {
      var productRepo = Repository<Product>();
      List<Product> productsToAddToConnectorPublication = new List<Product>();

      var product = productRepo.GetSingle(c => c.VendorItemNumber == vendorItemNumber);

      if (product == null) throw new InvalidOperationException("Product can't be found");

      productsToAddToConnectorPublication.Add(productRepo.GetSingle(c => c.VendorItemNumber == vendorItemNumber));

      if (propagate)
      {
        productsToAddToConnectorPublication.AddRange(Repository<Product>().GetAll(c => c.ParentProductID == product.ProductID).ToList());
      }

      var baseContentProductRecord = Repository<ContentProduct>().GetAll(c => c.ConnectorID == connectorID).FirstOrDefault(); //get first vendorid for the specified connector
      if (baseContentProductRecord == null) throw new InvalidOperationException("Base content product can't be found. Be sure to specify at least one rule with a ConnectorID, VendorID combo");

      var connectorPublicationRepo = Repository<ConnectorPublication>();

      foreach (var p in productsToAddToConnectorPublication)
      {
        var contentPublication = connectorPublicationRepo.GetSingle(c => c.ConnectorID == connectorID && c.VendorID == baseContentProductRecord.VendorID && c.ProductID == p.ProductID);
        if (contentPublication == null)
        {
          contentPublication = new ConnectorPublication()
          {
            ProductID = p.ProductID,
            VendorID = baseContentProductRecord.VendorID,
            ConnectorID = connectorID,
            Publish = false,
            ProductContentIndex = 0
          };
          connectorPublicationRepo.Add(contentPublication);
        }
      }
    }
  }

  public class LanguageDescriptionModel
  {
    public int LanguageID { get; set; }
    public int Counter { get; set; }
  }
}
