using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Vendors;
using Concentrator.Objects.Vendors.Bulk;

namespace Concentrator.Plugins.ACSystems
{
  public class ProductImport : VendorBase
  {
    protected override int VendorID
    {
      get { return Int32.Parse(Config.AppSettings.Settings["VendorID"].Value); }
    }

    protected override int DefaultVendorID
    {
      get { return Int32.Parse(Config.AppSettings.Settings["VendorID"].Value); }
    }

    protected override System.Configuration.Configuration Config
    {
      get { return GetConfiguration(); }
    }

    public override string Name
    {
      get { return "ACSystems Product Import"; }
    }

    protected override void SyncProducts()
    {
      var UserName = Config.AppSettings.Settings["Username"].Value;
      var Password = Config.AppSettings.Settings["Password"].Value;

      SessionService.SessionServiceClient session = new SessionService.SessionServiceClient();
      var key = session.Try(x=> x.GetSessionKey(UserName, Password), string.Empty);

      //If session failed
      if (!key.Equals(string.Empty))
      {
        log.AuditFatal("Failed to create session");
        return;
      }

      ProductInfoService.ProductInfoServiceClient client = new ProductInfoService.ProductInfoServiceClient();

      var prodsList = client.GetFullProductListExtended(key).ItemInfos;
      var prodsNames = client.GetFullDeviceList(key);

      var itemProducts = (from p in prodsList
                          join prod in prodsNames on p.ItemReference equals prod.ItemReference
                          select new
                          {
                            ProductName = prod.Name,
                            ShortDescription = p.Description,
                            //OtherDescription = p.DescriptionFR,
                            LongDescription = p.DescriptionFR, //checken of dit klopt!!
                            Brand = p.Category2,
                            VendorProductGroupCodeName3 = p.Category1,
                            VendorProductGroupCodeName2 = p.Category3,
                            VendorProductGroupCodeName1 = p.Category4,
                            VendorProductGroupCode3 = p.CategoryCode.Substring(0, 3),
                            VendorProductGroupCode2 = p.CategoryCode.Substring(6, 3),
                            VendorProductGroupCode1 = p.CategoryCode.Substring(9, 3),
                            Currency = p.Currency,
                            RelatedProduct = p.Devices,
                            p.Images,
                            p.InStock,
                            CostPrice = p.Price1Piece,
                            Price = p.PriceEndUser,
                            CustomItemNumber = p.ItemReference.OurItemNo,
                            EAN = p.ItemReference.EANCode,
                            VendorItemNumber = p.ItemReference.VendorItemNo
                          });
      List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem> assortmentList = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem>();

        int counter = 0;
        int total = itemProducts.Count();
        int totalNumberOfProductsToProcess = total;
        log.InfoFormat("Start processing {0} products", total);

        using (var unit = GetUnitOfWork())
        {
          foreach (var product in itemProducts)
          {
            var languageID = 2;

            //switch (product.Language)
            //{
            //  case "nl-NL":
            //    languageID = 2;
            //    break;
            //  case "en-GB":
            //    languageID = 1;
            //    break;
            //  default:
            //    continue;

            //}

            if (counter == 50)
            {
              counter = 0;
              log.InfoFormat("Still need to process {0} of {1}; {2} done;", totalNumberOfProductsToProcess, total, total - totalNumberOfProductsToProcess);
            }
            totalNumberOfProductsToProcess--;
            counter++;

            var assortment = new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem
            {
              #region BrandVendor
              BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>()
                        {
                            new VendorAssortmentBulk.VendorImportBrand()
                            {
                                VendorID = VendorID,
                                VendorBrandCode = product.Brand.Trim(),
                                ParentBrandCode = null,
                                Name = product.Brand.Trim() 
                            }
                        },
              #endregion

              #region GeneralProductInfo
              VendorProduct = new VendorAssortmentBulk.VendorProduct
              {
                VendorItemNumber = product.VendorItemNumber.Trim(), //EAN
                CustomItemNumber = product.CustomItemNumber.Trim(), //EAN
                ShortDescription = product.ShortDescription != null ? product.ShortDescription.Length > 150
                                        ? product.ShortDescription.Substring(0, 150)
                                        : product.ShortDescription : string.Empty,
                LongDescription = product.ShortDescription != null ? product.ShortDescription.Length > 1000
                                        ? product.ShortDescription.Substring(0, 1000)
                                        : product.ShortDescription : string.Empty,
                LineType = null,
                LedgerClass = null,
                ProductDesk = null,
                ExtendedCatalog = null,
                VendorID = VendorID,
                DefaultVendorID = DefaultVendorID,
                VendorBrandCode = product.Brand.Trim(), //UITGEVER_ID
                Barcode = product.Try(x => x.EAN, string.Empty),//EAN
                VendorProductGroupCode1 = product.Try(x => x.VendorProductGroupCode1, string.Empty),
                VendorProductGroupCodeName1 = product.Try(x => x.VendorProductGroupCodeName1, string.Empty),
                VendorProductGroupCode2 = product.Try(x => x.VendorProductGroupCode2, string.Empty),
                VendorProductGroupCodeName2 = product.Try(x => x.VendorProductGroupCodeName2, string.Empty),
                VendorProductGroupCode3 = product.Try(x => x.VendorProductGroupCode3, string.Empty),
                VendorProductGroupCodeName3 = product.Try(x => x.VendorProductGroupCodeName3, string.Empty),

              },
              #endregion

              #region RelatedProducts
              RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>()
                          {
                            //new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportRelatedProduct
                            //{
                            //    VendorID = VendorID,
                            //    DefaultVendorID = DefaultVendorID,
                            //    CustomItemNumber = product.ProductID.Trim(), //EAN
                            //    RelatedProductType = string.Empty,
                            //    RelatedCustomItemNumber = string.Empty
                            //}
                          },
              #endregion

              #region Attributes

              VendorImportAttributeValues = new List<VendorAssortmentBulk.VendorImportAttributeValue>(),

              #endregion

              #region Prices
              VendorImportPrices = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice()
                            {
                                VendorID = VendorID,
                                DefaultVendorID = DefaultVendorID,
                                CustomItemNumber = product.CustomItemNumber.Trim(), //EAN
                                Price = product.Price.ToString(),//.ToString("0.00", CultureInfo.InvariantCulture) ,
                                CostPrice =  product.CostPrice.ToString(),//.ToString("0.00", CultureInfo.InvariantCulture), //NETTOPRIJS
                                TaxRate = "19", //TODO: Calculate this!
                                MinimumQuantity = 0,
                                CommercialStatus = product.InStock > 0 ? "InStock" : "OutOfStock"
                            }
                        },
              #endregion

              #region Stock
              VendorImportStocks = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock()
                            {
                                VendorID = VendorID,
                                DefaultVendorID = DefaultVendorID,
                                CustomItemNumber = product.CustomItemNumber.Trim(), //EAN
                                QuantityOnHand = product.InStock,
                                StockType = "Assortment",
                                StockStatus =  product.InStock > 0 ? "InStock" : "OutOfStock"
                            }
                        },
              #endregion

              #region Descriptions
              VendorProductDescriptions = new List<VendorAssortmentBulk.VendorProductDescription>()
            {
               new VendorAssortmentBulk.VendorProductDescription(){
                VendorID = VendorID,
                LanguageID =languageID,
                DefaultVendorID = VendorID,
                CustomItemNumber = product.CustomItemNumber.Trim(), //EAN
                LongContentDescription = product.LongDescription,
                ShortContentDescription = product.LongDescription != string.Empty ? product.LongDescription.Length > 1000
                                     ? product.LongDescription.Substring(0, 1000)
                                      : product.LongDescription : product.ShortDescription,
                ProductName = product.ProductName
                            }
            }
              #endregion
            };

            // assortment will be added to the list defined outside of this loop
            assortmentList.Add(assortment);
          }

          // Creates a new instance of VendorAssortmentBulk(Passes in the AssortmentList defined above, vendorID and DefaultVendorID)
          using (var vendorAssortmentBulk = new VendorAssortmentBulk(assortmentList, VendorID, DefaultVendorID))
          {
            //vendorAssortmentBulk.IncludeBrandMapping = true;
            vendorAssortmentBulk.Init(unit.Context);
            vendorAssortmentBulk.Sync(unit.Context);
          }

        }
      throw new NotImplementedException();
    }
  }
}
