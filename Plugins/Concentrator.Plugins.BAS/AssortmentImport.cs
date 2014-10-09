using System;
using System.Linq;
using System.Data;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Plugins.BAS.Vendors.BAS.WebService;
using Concentrator.Objects.Models.Vendors;
using Microsoft.Practices.ServiceLocation;
using Ninject;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Vendors.Bulk;
using System.Collections.Generic;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Vendors;

namespace Concentrator.Plugins.BAS
{
  public class AssortmentImport : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "BAS Assortment Bulk Import Plugin"; }
    }

    private bool _retailStock;
    private bool _auctionStock;
    private bool _shopAssortment;


    private DataSet GetContent(Vendor vendor)
    {

      DataSet content = null;

      JdeAssortmentSoapClient cl = new JdeAssortmentSoapClient();
      if (_retailStock)
      {
        content = cl.GenerateFullProductListWithRetailStock(
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AssortmentImportID").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "InternetAssortment").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10OItems").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10ObyStatus").FirstOrDefault().Value));
      }
      else if (_auctionStock)
      {

        content = cl.GenerateFullProductListSpecialAssortment(
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AssortmentImportID").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "InternetAssortment").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10OItems").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10ObyStatus").FirstOrDefault().Value),
          0,
          vendor.VendorSettings.Where(c => c.SettingKey == "BSCStock").FirstOrDefault().Value,
          vendor.VendorSettings.Where(c => c.SettingKey == "CostPriceDC10").FirstOrDefault().Value,
          vendor.VendorSettings.Where(c => c.SettingKey == "CostPriceBSC").FirstOrDefault().Value,
          true
          );
      }
      else if (_shopAssortment)
      {
        content = cl.GenerateFullProductListWithNonStock(
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AssortmentImportID").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "InternetAssortment").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10OItems").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10ObyStatus").FirstOrDefault().Value),
          false,
          true
          );


      }
      else
      {

        content = cl.GenerateFullProductList(
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AssortmentImportID").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "InternetAssortment").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10OItems").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10ObyStatus").FirstOrDefault().Value),
          false,
          false
          );
      }

      if (vendor.VendorSettings.Where(c => c.SettingKey == "CostPriceImportID").FirstOrDefault() != null)
      {

        DataSet contentCostPrice = cl.GenerateFullProductList(
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "CostPriceImportID").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "InternetAssortment").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10OItems").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10ObyStatus").FirstOrDefault().Value),
          false,
          false
          );
        content.Tables[0].Columns.Add("CostPrice", typeof(decimal));
        content.Tables[0].Columns["CostPrice"].AllowDBNull = true;

        if (contentCostPrice != null)
        {
          foreach (DataRow row in content.Tables[0].AsEnumerable())
          {
            row["CostPrice"] = (from ccp in contentCostPrice.Tables[0].AsEnumerable()
                                where
                                  ccp.Field<double>("ShortItemNumber") == row.Field<double>("ShortItemNumber")
                                  && ccp.Field<int>("MinimumQuantity") == row.Field<int>("MinimumQuantity")
                                select ccp.Field<decimal>("UnitPrice")).FirstOrDefault();
          }
        }
        else
          log.Error("Cosprices list empty");
      }

      return content;
    }


    protected override void Process()
    {
        foreach (Vendor vendor in Vendors.Where(x => ((VendorType)x.VendorType).Has(VendorType.Assortment) && x.IsActive))
      {
        try
        {
          if (vendor.VendorSettings.GetValueByKey<int>("AssortmentImportID", 0) < 1)
            continue;

          log.DebugFormat("Start Assortment Import for Vendor '{0} ({1})'", vendor.Name, vendor.VendorID);

          _retailStock = false;//vendor.VendorSettings.GetValueByKey<bool>("RetailStock", false);
          _auctionStock = vendor.VendorSettings.GetValueByKey<bool>("AuctionStock", false);
          _shopAssortment = vendor.VendorSettings.GetValueByKey<bool>("ShopAssortment", false);

          using (var content = GetContent(vendor))
          {
            // DataSet content = new DataSet();

            //try
            //{
            //  //content.WriteXml(@"F:\temp\ass.xml");


            //  content.ReadXml(@"F:\temp\ass.xml");
            //}
            //catch (Exception ex)
            //{

            //}
            int totalProducts = content.Tables[0].AsEnumerable().Count();
            log.DebugFormat("Start Assortment processing for {0}, {1} Products", vendor.Name, totalProducts);

            if (totalProducts > 0)
              BulkImport(vendor.VendorID, content, vendor.ParentVendorID, vendor);
            else
              log.Debug("Stop processing with empty dataset");
          }

          //if (_auctionStock)
          //{
          //  var bsv = new ProcessBSCStockAssortment();
          //  bsv.Process(content, vendor, log, false);
          //}
          //else
          //{
          //  var ass = new ProcessAssortment();
          //  ass.Process(content, vendor, log);
          //}

          //if (vendor.RetailStock)
          //{
          //  var rs = new ProcessRetailStock();
          //  rs.Process(Content, VendorID, log);
          //}

          log.DebugFormat("Finished Assortment Import for vendor '{0} ({1})'", vendor.Name, vendor.VendorID);
        }
        catch (Exception ex)
        {
          log.Error("Error import BAS assortment", ex);
        }

      }
    }

    private void BulkImport(int vendorID, DataSet content, int? parentVendorID, Vendor vendor)
    {
      using (var unit = GetUnitOfWork())
      {
        if (content.Tables[0].AsEnumerable().Count() < 1000)
          return;

        var dataList = (from d in content.Tables[0].AsEnumerable()
                        let DefaultVendorID = parentVendorID.HasValue ? parentVendorID.Value : vendorID
                        select new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem
                          {
                            #region BrandVendor
                            BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>()
                          {
                            new VendorAssortmentBulk.VendorImportBrand(){
                              VendorID = DefaultVendorID,
                              VendorBrandCode = VendorImportUtility.SetDataSetValue("Brand", d) ?? string.Empty,
                              ParentBrandCode = null,
                              Name =  VendorImportUtility.SetDataSetValue("Brand", d) ?? string.Empty,
                            }
                          },
                            #endregion
                            #region GeneralProductInfo
                            VendorProduct = new VendorAssortmentBulk.VendorProduct
                            {
                              VendorItemNumber = VendorImportUtility.SetDataSetValue("VendorItemNumber", d),
                              CustomItemNumber = VendorImportUtility.SetDataSetValue("ShortItemNumber", d),
                              ShortDescription = VendorImportUtility.SetDataSetValue("Description1", d),
                              LongDescription = VendorImportUtility.SetDataSetValue("Description2", d),
                              LineType = VendorImportUtility.SetDataSetValue("LineType", d),
                              LedgerClass = d.Table.Columns.Contains("LedgerClass") ? VendorImportUtility.SetDataSetValue("LedgerClass", d) : null,
                              ProductDesk = d.Table.Columns.Contains("ProductDesk") ? VendorImportUtility.SetDataSetValue("ProductDesk", d) : null,
                              ExtendedCatalog = d.Table.Columns.Contains("Extendedcatalog") ? VendorImportUtility.SetDataSetValue("Extendedcatalog", d) : null,
                              VendorID = vendorID,
                              DefaultVendorID = DefaultVendorID,
                              VendorBrandCode = VendorImportUtility.SetDataSetValue("Brand", d),
                              Barcode = VendorImportUtility.SetDataSetValue("Barcode", d),
                              VendorProductGroupCode1 = VendorImportUtility.SetDataSetValue("ProductGroup", d),
                              VendorProductGroupCodeName1 = string.Empty,
                              VendorProductGroupCode2 = VendorImportUtility.SetDataSetValue("ProductSubGroup", d),
                              VendorProductGroupCodeName2 = string.Empty,
                              VendorProductGroupCode3 = VendorImportUtility.SetDataSetValue("ProductSubSubGroup", d),
                              VendorProductGroupCodeName3 = string.Empty,
                              VendorProductGroupCode4 = null,
                              VendorProductGroupCodeName4 = null,
                              VendorProductGroupCode5 = null,
                              VendorProductGroupCodeName5 = null,
                              VendorProductGroupCode6 = null,
                              VendorProductGroupCodeName6 = null,
                              VendorProductGroupCode7 = null,
                              VendorProductGroupCodeName7 = null,
                              VendorProductGroupCode8 = null,
                              VendorProductGroupCodeName8 = null,
                              VendorProductGroupCode9 = null,
                              VendorProductGroupCodeName9 = null,
                              VendorProductGroupCode10 = null,
                              VendorProductGroupCodeName10 = null
                            },
                            #endregion
                            #region RelatedProducts
                            RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>(),
                            #endregion
                            #region Attribures
                            VendorImportAttributeValues = new List<VendorAssortmentBulk.VendorImportAttributeValue>(),
                            #endregion
                            #region Prices
                            VendorImportPrices = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
                          {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice(){
                              VendorID = vendorID,
                              DefaultVendorID = DefaultVendorID,
                              CustomItemNumber = VendorImportUtility.SetDataSetValue("ShortItemNumber", d),
                              Price = VendorImportUtility.SetDataSetValue("UnitPrice", d, "0"),
                              CostPrice = VendorImportUtility.SetDataSetValue("CostPrice", d, "0"),
                              TaxRate = VendorImportUtility.SetDataSetValue("TaxRate", d, "0"),
                              MinimumQuantity = VendorImportUtility.SetDataSetValue("MinimumQuantity", d).Try(x => int.Parse(x) < 0 ? 0 : int.Parse(x),0),
                              CommercialStatus = VendorImportUtility.SetDataSetValue("CommercialStatus",d, "S")
                            }
                          },

                            #endregion
                            #region Stock
                            VendorImportStocks = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock>()
                          {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock(){
                              VendorID = vendorID,
                              DefaultVendorID = DefaultVendorID,
                              CustomItemNumber = VendorImportUtility.SetDataSetValue("ShortItemNumber", d),
                              QuantityOnHand = VendorImportUtility.SetDataSetValue("QuantityOnHand",d).Try(x => int.Parse(x),0),
                              StockType = "Assortment",
                              StockStatus = VendorImportUtility.SetDataSetValue("StockStatus",d)
                            }
                          },

                            #endregion
                          });

        //var dataList = (from d in content.Tables[0].AsEnumerable()
        //                let defaultVendorID = parentVendorID.HasValue ? parentVendorID.Value : vendorID
        //                select new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem
        //                {
        //                  #region BrandVendor
        //                  BrandVendors = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportBrand>()
        //                  {
        //                    new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportBrand(){
        //                      Name = VendorImportUtility.SetDataSetValue("Brand", d),
        //                      VendorBrandCode = VendorImportUtility.SetDataSetValue("Brand", d),
        //                      VendorID = defaultVendorID
        //                    }
        //                  },
        //                  #endregion
        //                  #region GeneralProductInfo
        //                  VendorProduct = new VendorAssortmentBulk.VendorProduct
        //                  {
        //                    VendorItemNumber = VendorImportUtility.SetDataSetValue("VendorItemNumber", d),
        //                    CustomItemNumber = VendorImportUtility.SetDataSetValue("ShortItemNumber", d),
        //                    ShortDescription = VendorImportUtility.SetDataSetValue("Description1", d),
        //                    LongDescription = VendorImportUtility.SetDataSetValue("Description2", d),
        //                    LineType = VendorImportUtility.SetDataSetValue("LineType", d),
        //                    LedgerClass = d.Table.Columns.Contains("LedgerClass") ? VendorImportUtility.SetDataSetValue("LedgerClass", d) : null,
        //                    ProductDesk = d.Table.Columns.Contains("ProductDesk") ? VendorImportUtility.SetDataSetValue("ProductDesk", d) : null,
        //                    ExtendedCatalog = d.Table.Columns.Contains("Extendedcatalog") ? VendorImportUtility.SetDataSetValue("Extendedcatalog", d) : null,
        //                    VendorID = vendorID,
        //                    DefaultVendorID = defaultVendorID,
        //                    VendorBrandCode = VendorImportUtility.SetDataSetValue("Brand", d),
        //                    Barcode = VendorImportUtility.SetDataSetValue("Barcode", d),
        //                    VendorProductGroupCode1 = VendorImportUtility.SetDataSetValue("ProductGroup", d),
        //                    VendorProductGroupCode2 = VendorImportUtility.SetDataSetValue("ProductSubGroup", d),
        //                    VendorProductGroupCode3 = VendorImportUtility.SetDataSetValue("ProductSubSubGroup", d)
        //                  },
        //                  //                     VendorImportPrices = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
        //                  //                     {
        //                  //                       new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice(){
        //                  //                         VendorID = vendorID,
        //                  //                         DefaultVendorID = defaultVendorID,
        //                  //                         CustomItemNumber = VendorImportUtility.SetDataSetValue("ShortItemNumber", d),
        //                  //                          Price = d.Field<decimal>("UnitPrice"),
        //                  //   CostPrice = (d.Table.Columns.Contains("CostPrice") ? d.Field<decimal?>("CostPrice") : 0) ?? 0,
        //                  //        TaxRate = (decimal)(d.Field<double>("TaxRate")),
        //                  //                         MinimumQuantity = (d.Field<int>("MinimumQuantity") > 0 ? d.Field<int>("MinimumQuantity") : 0),
        //                  //CommercialStatus = d.Field<string>("CommercialStatus")
        //                  //                       }
        //                  //                     }
        //                  #endregion
        //                });

        log.Debug("dataset to object");

        string vendorBrandID = vendor.VendorSettings.GetValueByKey("BrandID", string.Empty);
        if (!string.IsNullOrEmpty(vendorBrandID))
          dataList = dataList.Where(x => x.VendorProduct.VendorBrandCode == vendorBrandID);

        using (var vendorAssortmentBulk = new VendorAssortmentBulk(dataList, vendorID, parentVendorID))
        {
          vendorAssortmentBulk.Init(unit.Context);
          vendorAssortmentBulk.Sync(unit.Context);
        }
      }
    }
  }
}
