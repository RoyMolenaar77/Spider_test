using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Concentrator.Objects;
using log4net;
using System.Data.Linq;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Utility;
using Concentrator.Objects.Models.Attributes;

namespace Concentrator.Plugins.BAS
{
  //public enum VendorStockType
  //{
  //  VendorStockRegularID = 1,
  //  VendorStockBSCID = 2,
  //  VendorStockBSCOEMID = 3,
  //  VendorStockBSCDSID = 4,
  //  VendorStockBSCPRID = 5
  //}

  public class VendorDataParser
  {
    private DataRow _record;

    private Product _product;
    private int _brandID;

    private Vendor _vendor;
    private ILog _log;
    private IUnitOfWork _unit;
    private IEnumerable<VendorAssortment> _vendorAssortment = null;
    private List<VendorProductStatus> _vProductStatus = null;
    private List<ProductGroupVendor> _productGroupVendorRecords = null;
    private List<ProductGroupVendor> _vendorProductGroups = null;
    private List<VendorPrice> _vendorPrices = null;
    private List<VendorStock> _vendorStock = null;
    private VendorStockTypes _vendorStockTypes;
    private Dictionary<string, BrandVendor> _brandVendorList = new Dictionary<string, BrandVendor>();
    private Dictionary<int, List<int>> _vendorProductGroupDictionary;

    public VendorDataParser(IUnitOfWork unit, Vendor vendor, ILog log)
    {
      this._unit = unit;
      _log = log;
      _vendor = vendor;

      _vendorAssortment = unit.Scope.Repository<VendorAssortment>().Include(x => x.Product).GetAll(c => c.VendorID == _vendor.VendorID).ToList();

      _vProductStatus = unit.Scope.Repository<VendorProductStatus>().GetAll(vs => vs.VendorID == _vendor.VendorID ||
                           _vendor.ParentVendorID.HasValue && vs.VendorID == _vendor.ParentVendorID).ToList();

      _vendorPrices = unit.Scope.Repository<VendorPrice>().GetAll(vp => vp.VendorAssortment.VendorID == _vendor.VendorID).ToList();

      _vendorStock = unit.Scope.Repository<VendorStock>().GetAll(vs => vs.VendorID == _vendor.VendorID).ToList();

      _vendorStockTypes = new VendorStockTypes(unit.Scope.Repository<VendorStockType>());

      //_vendorProductGroups = _unit.Scope.Repository<ProductGroupVendor>().GetAll(pc => pc.VendorID == vendor.VendorID || vendor.ParentVendorID.HasValue && pc.VendorID == vendor.ParentVendorID).ToList();

      //_vendorProductGroupDictionary = unit.Scope.Repository<VendorAssortment>().Include(x => x.ProductGroupVendors)
      //  .GetAll(c => c.VendorID == _vendor.VendorID).Select(x => new
      //{
      //  x.VendorAssortmentID,
      //  list = x.ProductGroupVendors.Where(y => y.VendorID == _vendor.VendorID || (_vendor.ParentVendorID.HasValue && y.VendorID == _vendor.ParentVendorID.Value)).Select(y => y.ProductGroupVendorID)
      //}).ToDictionary(x => x.VendorAssortmentID, y => y.list.ToList());
    }

    #region Private Helper methods

    public bool LoadProduct(DataRow record, log4net.ILog log)
    {
      _product = null;
      _brandID = -1;

      this._record = record;

      //Parse brand vendor
      ParseBrandVendor();

      int vendorBrandID = this._vendor.VendorSettings.GetValueByKey("BrandID", 0);
      if (vendorBrandID > 0 && this._brandID != vendorBrandID)
        return false;

      string vendorItemNumber = _record.Field<string>("VendorItemNumber").Trim().ToUpper();
      var products = _unit.Scope.Repository<Product>().GetAll(p => p.VendorItemNumber.Trim().ToUpper() == vendorItemNumber && p.SourceVendorID == _vendor.VendorID).ToList();

      if (products.Count > 0 || (products.Count == 1 && products.FirstOrDefault().BrandID != this._brandID))
      {
        _product = products.SingleOrDefault(x => x.BrandID == this._brandID);
        if (_product == null)
        {
          foreach (var p in products)
          {
            if (p.VendorAssortments.Count < 1 && this._brandID > 0)
            {
              p.BrandID = this._brandID;
              p.SourceVendorID = _vendor.VendorID;
              _product = p;

              log.InfoFormat("Try delete productinformation for productID {0}", _product.ProductID);

              _unit.Scope.Repository<ProductDescription>().Delete(p.ProductDescriptions);
              _unit.Scope.Repository<ProductBarcode>().Delete(p.ProductBarcodes);

              _unit.Scope.Repository<ProductAttributeValue>().Delete(p.ProductAttributeValues);
              _unit.Scope.Repository<ProductMedia>().Delete(p.ProductMedias);

              _unit.Save();
              break;
            }
          }
        }
      }

      if (_product == null)
      {
        string customItemNumber = _record.Field<double>("ShortItemNumber").ToString();
        var va = _unit.Scope.Repository<VendorAssortment>().GetSingle(x => x.CustomItemNumber == customItemNumber && x.VendorID == _vendor.VendorID);

        if (va != null)
        {
          if (!va.Product.VendorAssortments.Any(x => x.CustomItemNumber != customItemNumber))
          {
            _product = va.Product;
            _product.VendorItemNumber = vendorItemNumber;
            _product.BrandID = _brandID;
            return true;
          }
        }

        log.DebugFormat("NEW product: {0}", vendorItemNumber);
      }
      return true;
    }

    private void LoadAssortmentProduct(DataRow record, ILog log, bool retailStock)
    {
      _product = null;
      _brandID = -1;

      this._record = record;
      string customItemNumber = string.Empty;

      if (retailStock)
        customItemNumber = _record.Field<int>("ProductID").ToString();
      else
        customItemNumber = _record.Field<double>("ShortItemNumber").ToString();

      string vendorItemNumber = _record.Field<string>("VendorItemNumber").Trim().ToUpper();

      if (_vendorAssortment != null)
      {
        try
        {
          _product = _vendorAssortment.Where(x => x.CustomItemNumber == customItemNumber
            && x.Product.VendorItemNumber.Trim().ToUpper() == vendorItemNumber
            && x.VendorID == _vendor.VendorID
            ).Select(x => x.Product).SingleOrDefault();
          if (_product != null)
            _brandID = _product.BrandID;
        }
        catch (Exception ex)
        {
          var items = _vendorAssortment.Where(x => x.CustomItemNumber == customItemNumber
            && x.Product.VendorItemNumber.Trim().ToUpper() == vendorItemNumber
            );

          log.ErrorFormat("{0} Duplicate vendor customitemnummer {1}, remove items for next assortment import", items.Count(), customItemNumber);
          var assortmentRepo = _unit.Scope.Repository<VendorAssortment>();
          _unit.Save();
          assortmentRepo.Delete(items);
          _unit.Save();

          _vendorAssortment = assortmentRepo.GetAll(a => a.VendorID == _vendor.VendorID);
          _vendorProductGroupDictionary = _unit.Scope.Repository<VendorAssortment>().Include(x => x.ProductGroupVendors)
        .GetAll(c => c.VendorID == _vendor.VendorID).Select(x => new
      {
        x.VendorAssortmentID,
        list = x.ProductGroupVendors.Where(y => y.VendorID == _vendor.VendorID || (_vendor.ParentVendorID.HasValue && y.VendorID == _vendor.ParentVendorID.Value)).Select(y => y.ProductGroupVendorID)
      }).ToDictionary(x => x.VendorAssortmentID, y => y.list.ToList());
        }
      }
    }

    //private void ParseProduct(log4net.ILog log)
    //{
    //  string vendorItemNumber = _record.Field<string>("VendorItemNumber").Trim().ToUpper();
    //  bool doStock = false;
    //  if (this._product == null)
    //  {
    //    doStock = true;
    //    _product = new Product()
    //    {
    //      VendorItemNumber = vendorItemNumber,
    //      BrandID = this._brandID,
    //      SourceVendorID = _vendor.VendorID
    //    };
    //    if (_product.ProductBarcodes == null) _product.ProductBarcodes = new List<ProductBarcode>();
    //    var existing = (from p in _product.ProductBarcodes
    //                    where p.Barcode == _record.Try(c => c.Field<string>("Barcode").Try(f => f.Trim(), string.Empty), string.Empty)
    //                    select p).FirstOrDefault();

    //    if (existing == null && !string.IsNullOrEmpty(_record.Try(c => c.Field<string>("Barcode").Try(f => f.Trim(), string.Empty), string.Empty)))
    //    {
    //      var bar = new ProductBarcode
    //      {
    //        Product = _product,
    //        Barcode = _record.Try(c => c.Field<string>("Barcode").Trim(), string.Empty),
    //        VendorID = _vendor.ParentVendorID.HasValue ? _vendor.ParentVendorID.Value : _vendor.VendorID,
    //        BarcodeType = (int)BarcodeTypes.Default
    //      };
    //      _unit.Scope.Repository<ProductBarcode>().Add(bar);
    //    }

    //    log.DebugFormat("new product found {0} brand {1}", vendorItemNumber, this._brandID);

    //    _unit.Scope.Repository<Product>().Add(_product);
    //    _unit.Save();
    //  }
    //  else
    //  {
    //    if (this._brandID > 0)
    //    {
    //      if (_product.BrandID < 0)
    //      {
    //        _product.BrandID = this._brandID;
    //        _unit.Save();
    //      }
    //    }

    //  }



    //  VendorAssortment assortment = (from a in _vendorAssortment
    //                                 where a.ProductID == _product.ProductID
    //                                 select a).FirstOrDefault();
    //  if (assortment == null)
    //  {
    //    assortment = CreateAssortment();
    //    doStock = true;
    //  }
    //  else
    //  {
    //    assortment.ShortDescription = _record.Field<string>("Description1").Trim();
    //    assortment.LongDescription = _record.Field<string>("Description2").Trim();
    //    assortment.LineType = _record.Field<string>("LineType").Trim();
    //    assortment.LedgerClass = _record.Table.Columns.Contains("LedgerClass") ? _record.Field<string>("LedgerClass") : null;
    //    assortment.ProductDesk = _record.Table.Columns.Contains("ProductDesk") ? _record.Field<string>("ProductDesk") : null;
    //    assortment.CustomItemNumber = _record.Field<double>("ShortItemNumber").ToString();

    //    if (_record.Table.Columns.Contains("Extendedcatalog") && _record.Field<bool?>("Extendedcatalog").HasValue)
    //      assortment.ExtendedCatalog = _record.Field<bool?>("Extendedcatalog").Value;

    //    assortment.IsActive = true;
    //  }

    //  #region Vendor Product Group Assortment


    //  List<int> existingProductGroupVendors = new List<int>();

    //  foreach (ProductGroupVendor prodGroupVendor in _productGroupVendorRecords)
    //  {
    //    existingProductGroupVendors.Add(prodGroupVendor.ProductGroupVendorID);

    //    if (!_vendorProductGroupDictionary.ContainsKey(assortment.VendorAssortmentID) || prodGroupVendor.VendorAssortments == null)
    //      prodGroupVendor.VendorAssortments = new List<VendorAssortment>();
    //    else
    //    {
    //      if (_vendorProductGroupDictionary[assortment.VendorAssortmentID].Contains(prodGroupVendor.ProductGroupVendorID))
    //        continue;
    //    }

    //    //if (prodGroupVendor.VendorAssortments == null) prodGroupVendor.VendorAssortments = new List<VendorAssortment>();

    //    //if (prodGroupVendor.VendorAssortments.Any(x => x.VendorAssortmentID == assortment.VendorAssortmentID))
    //    //{ // only add new rows
    //    //  continue;
    //    //}

    //    prodGroupVendor.VendorAssortments.Add(assortment);

    //    if (_vendorProductGroupDictionary.ContainsKey(assortment.VendorAssortmentID))
    //      _vendorProductGroupDictionary[assortment.VendorAssortmentID].Add(prodGroupVendor.ProductGroupVendorID);
    //    else
    //    {
    //      _vendorProductGroupDictionary.Add(assortment.VendorAssortmentID, new List<int>() { prodGroupVendor.ProductGroupVendorID });
    //    }
    //  }

    //  //if (assortment.ProductGroupVendors == null) assortment.ProductGroupVendors = new List<ProductGroupVendor>();

    //  if (_vendorProductGroupDictionary.ContainsKey(assortment.VendorAssortmentID))
    //  {
    //    var unusedProductGroups = (from a in _vendorProductGroupDictionary[assortment.VendorAssortmentID]
    //                               where !existingProductGroupVendors.Contains(a)
    //                               select a).ToList();

    //    unusedProductGroups.ForEach(x =>
    //    {
    //      var unusedProductGroup = _unit.Scope.Repository<ProductGroupVendor>().GetSingle(y => y.ProductGroupVendorID == x);
    //      _unit.Scope.Repository<ProductGroupVendor>().Delete(unusedProductGroup);
    //    });
    //  }


    //  #endregion

    //  var barcodeRepo = _unit.Scope.Repository<ProductBarcode>();

    //  #region ProductBarcode
    //  if (!string.IsNullOrEmpty(_record.Field<string>("Barcode")))
    //  {
    //    var existingBarcode = (from p in _product.ProductBarcodes
    //                           where p.Barcode == _record.Try(c => c.Field<string>("Barcode").Trim(), string.Empty)
    //                           select p).ToList();

    //    if (existingBarcode.Count < 1 && !string.IsNullOrEmpty(_record.Try(c => c.Field<string>("Barcode"), string.Empty)))
    //    {
    //      var bar = new ProductBarcode
    //      {
    //        Product = _product,
    //        Barcode = _record.Try(c => c.Field<string>("Barcode").Trim(), string.Empty),
    //        VendorID = _vendor.VendorID,
    //        BarcodeType = (int)BarcodeTypes.Default
    //      };
    //      _unit.Scope.Repository<ProductBarcode>().Add(bar);
    //      barcodeRepo.Add(bar);
    //    }

    //    foreach (var bar in existingBarcode)
    //    {
    //      if (!bar.VendorID.HasValue)
    //        bar.VendorID = _vendor.ParentVendorID.HasValue ? _vendor.ParentVendorID.Value : _vendor.VendorID;
    //    }

    //    if (!string.IsNullOrEmpty(_record.Try(c => c.Field<string>("Barcode").Trim(), string.Empty)))
    //    {
    //      var unusedBarCodes = _product.ProductBarcodes.
    //        Where(x => (x.VendorID == _vendor.VendorID || (_vendor.ParentVendorID.HasValue && x.VendorID == _vendor.ParentVendorID.Value))
    //        && x.Barcode != _record.Try(c => c.Field<string>("Barcode").Trim(), string.Empty)).ToList();

    //      if (unusedBarCodes.Count() > 0)
    //      {
    //        log.DebugFormat("Delete {0} barcodes for productID {1}", unusedBarCodes.Count(), _product.ProductID);

    //        foreach (var b in unusedBarCodes)
    //        {
    //          var bar = barcodeRepo.GetSingle(x => x.Barcode == b.Barcode && (b.BarcodeType == null ? x.BarcodeType == null : x.BarcodeType == b.BarcodeType) && x.ProductID == b.ProductID);
    //          if (bar != null)
    //          {
    //            _unit.Scope.Repository<ProductBarcode>().Delete(bar);
    //          }
    //        }
    //      }
    //    }
    //  }
    //  #endregion
    //}

    private VendorAssortment CreateAssortment()
    {
      VendorAssortment vendorAssortment = new VendorAssortment()
      {
        Product = this._product,
        CustomItemNumber = _record.Field<double>("ShortItemNumber").ToString(),
        VendorID = _vendor.VendorID,
        ShortDescription = _record.Field<string>("Description1"),
        LongDescription = _record.Field<string>("Description2"),
        LineType = _record.Field<string>("LineType"),
        LedgerClass = _record.Table.Columns.Contains("LedgerClass") ? _record.Field<string>("LedgerClass") : null,
        ProductDesk = _record.Table.Columns.Contains("ProductDesk") ? _record.Field<string>("ProductDesk") : null,
        IsActive = true
      };

      if (_record.Table.Columns.Contains("Extendedcatalog") && _record.Field<bool?>("Extendedcatalog").HasValue)
        vendorAssortment.ExtendedCatalog = _record.Field<bool?>("Extendedcatalog").Value;

      _unit.Scope.Repository<VendorAssortment>().Add(vendorAssortment);

      _unit.Save();

      return vendorAssortment;
    }

    private void ParseBrandVendor()
    {
      BrandVendor bv = VendorUtility.MergeBrand(_unit.Scope.Repository<BrandVendor>(), _vendor, _record.Field<string>("Brand").Trim(), _brandVendorList);
      //submit changes
      this._brandID = bv.BrandID;
    }

    //private void ParseProductGroupVendor()
    //{
    //  string productGroupCode = _record.Field<string>("ProductGroup");
    //  string productSubGroupCode = _record.Field<string>("ProductSubGroup");
    //  string productSubSubGroupCode = _record.Field<string>("ProductSubSubGroup");
    //  string brandcode = _record.Field<string>("Brand").Trim();

    //  _productGroupVendorRecords = VendorUtility.MergeProductGroupVendor(_vendorProductGroups, _vendor, brandcode, productGroupCode, productSubGroupCode, productSubSubGroupCode, _unit);
    //}

    private void ParseBSCStockLevels()
    {
      //update the regular stock levels
      ParseStockLevels();
      var repoVendorStock = _unit.Scope.Repository<VendorStock>();

      #region BSC OEM
      VendorStock bscoem = (from vs in _vendorStock
                            where vs.ProductID == _product.ProductID
                                  &&
                                  vs.VendorID == _vendor.VendorID
                                  &&
                                  vs.VendorStockType == _vendorStockTypes.SyncVendorStockTypes("BSCOEM")
                            select vs).FirstOrDefault();

      if (_record.Field<int?>("QuantityBSCOEM").HasValue && _record.Field<decimal?>("CostPriceBSC").HasValue)
      {
        if (bscoem == null)
        {
          bscoem = new VendorStock()
          {
            VendorID = _vendor.VendorID,
            ProductID = _product.ProductID,
            VendorStockType = _vendorStockTypes.SyncVendorStockTypes("BSCOEM")
          };


          repoVendorStock.Add(bscoem);
          _vendorStock.Add(bscoem);
        }
        bscoem.QuantityOnHand = _record.Field<int>("QuantityBSCOEM");
        bscoem.UnitCost = _record.Field<decimal>("CostPriceBSC");
        bscoem.ConcentratorStatusID = UpdateVendorStatuses(_record.Field<string>("StockStatus"));
        bscoem.StockStatus = _record.Field<string>("StockStatus");//TODO: To remove
        bscoem.VendorStatus = _record.Field<string>("StockStatus");
      }
      else
      {
        if (bscoem != null)
        {
          repoVendorStock.Delete(bscoem);
          _vendorStock.Remove(bscoem);
        }
      }
      #endregion

      #region BSC MYVEIL
      VendorStock bsc = (from vs in _vendorStock
                         where vs.ProductID == _product.ProductID
                               &&
                               vs.VendorID == _vendor.VendorID
                               &&
                               vs.VendorStockType == _vendorStockTypes.SyncVendorStockTypes("BSC")
                         select vs).FirstOrDefault();

      if (_record.Field<int?>("QuantityBSC").HasValue && _record.Field<decimal?>("CostPriceBSC").HasValue)
      {
        if (bsc == null)
        {
          bsc = new VendorStock()
          {
            VendorID = _vendor.VendorID,
            ProductID = _product.ProductID,
            VendorStockType = _vendorStockTypes.SyncVendorStockTypes("BSC")
          };
          repoVendorStock.Add(bsc);
          _vendorStock.Add(bsc);
        }
        bsc.QuantityOnHand = _record.Field<int>("QuantityBSC");
        bsc.UnitCost = _record.Field<decimal>("CostPriceBSC");

        bsc.ConcentratorStatusID = UpdateVendorStatuses(_record.Field<string>("StockStatus"));
        bsc.VendorStatus = _record.Field<string>("StockStatus");
        bsc.StockStatus = _record.Field<string>("StockStatus");
      }
      else
      {
        if (bsc != null)
        {
          repoVendorStock.Delete(bsc);
          _vendorStock.Remove(bsc);
        }
      }
      #endregion

      #region BSC DEMO
      VendorStock bscDS = (from vs in _vendorStock
                           where vs.ProductID == _product.ProductID
                                 &&
                                 vs.VendorID == _vendor.VendorID
                                 &&
                                 vs.VendorStockType == _vendorStockTypes.SyncVendorStockTypes("BSCDEMO")
                           select vs).FirstOrDefault();

      if (_record.Field<int?>("QuantityBSCDEMO").HasValue && _record.Field<decimal?>("CostPriceBSC").HasValue)
      {
        if (bscDS == null)
        {
          bscDS = new VendorStock()
          {
            VendorID = _vendor.VendorID,
            ProductID = _product.ProductID,
            VendorStockType = _vendorStockTypes.SyncVendorStockTypes("BSCDEMO")
          };
          repoVendorStock.Add(bscDS);
          _vendorStock.Add(bscDS);
        }
        bscDS.QuantityOnHand = _record.Field<int>("QuantityBSCDEMO");
        bscDS.UnitCost = _record.Field<decimal>("CostPriceBSC");

        bscDS.ConcentratorStatusID = UpdateVendorStatuses(_record.Field<string>("StockStatus"));
        bscDS.VendorStatus = _record.Field<string>("StockStatus");
        bscDS.StockStatus = _record.Field<string>("StockStatus");
      }
      else
      {
        if (bscDS != null)
        {
          repoVendorStock.Delete(bscDS);
          _vendorStock.Remove(bscDS);
        }
      }
      #endregion

      #region BSC DMGBOX
      VendorStock bscDMGBOX = (from vs in _vendorStock
                               where vs.ProductID == _product.ProductID
                                     &&
                                     vs.VendorID == _vendor.VendorID
                                     &&
                                     vs.VendorStockType == _vendorStockTypes.SyncVendorStockTypes("BSCDMGBOX")
                               select vs).FirstOrDefault();

      if (_record.Field<int?>("QuantityBSCDMGBOX").HasValue && _record.Field<decimal?>("CostPriceBSC").HasValue)
      {
        if (bscDMGBOX == null)
        {
          bscDMGBOX = new VendorStock()
          {
            VendorID = _vendor.VendorID,
            ProductID = _product.ProductID,
            VendorStockType = _vendorStockTypes.SyncVendorStockTypes("BSCDMGBOX")
          };
          repoVendorStock.Add(bscDMGBOX);
          _vendorStock.Add(bscDMGBOX);
        }
        bscDMGBOX.QuantityOnHand = _record.Field<int>("QuantityBSCDMGBOX");
        bscDMGBOX.UnitCost = _record.Field<decimal>("CostPriceBSC");

        bscDMGBOX.ConcentratorStatusID = UpdateVendorStatuses(_record.Field<string>("StockStatus"));
        bscDMGBOX.VendorStatus = _record.Field<string>("StockStatus");
        bscDMGBOX.StockStatus = _record.Field<string>("StockStatus");
      }
      else
      {
        if (bscDMGBOX != null)
        {
          repoVendorStock.Delete(bscDMGBOX);
          _vendorStock.Remove(bscDMGBOX);
        }
      }
      #endregion

      #region BSCDMGITEM
      VendorStock bscDMGItem = (from vs in _vendorStock
                                where vs.ProductID == _product.ProductID
                                      &&
                                      vs.VendorID == _vendor.VendorID
                                      &&
                                      vs.VendorStockType == _vendorStockTypes.SyncVendorStockTypes("BSCDMGITEM")
                                select vs).FirstOrDefault();

      if (_record.Field<int?>("QuantityBSCDMGITEM").HasValue && _record.Field<decimal?>("CostPricebsc").HasValue)
      {
        if (bscDMGItem == null)
        {
          bscDMGItem = new VendorStock()
          {
            VendorID = _vendor.VendorID,
            ProductID = _product.ProductID,
            VendorStockType = _vendorStockTypes.SyncVendorStockTypes("BSCDMGITEM")
          };
          repoVendorStock.Add(bscDMGItem);
          _vendorStock.Add(bscDMGItem);
        }
        bscDMGItem.QuantityOnHand = _record.Field<int>("QuantityBSCDMGITEM");
        bscDMGItem.UnitCost = _record.Field<decimal>("CostPricebsc");

        bscDMGItem.ConcentratorStatusID = UpdateVendorStatuses(_record.Field<string>("StockStatus"));
        bscDMGItem.VendorStatus = _record.Field<string>("StockStatus");
        bscDMGItem.StockStatus = _record.Field<string>("StockStatus");
      }
      else
      {
        if (bscDMGItem != null)
        {
          repoVendorStock.Delete(bscDMGItem);
          _vendorStock.Remove(bscDMGItem);
        }
      }
      #endregion

      #region BSC BSCIMCOMPL
      VendorStock bscBSCIMCOMPL = (from vs in _vendorStock
                                   where vs.ProductID == _product.ProductID
                                         &&
                                         vs.VendorID == _vendor.VendorID
                                         &&
                                         vs.VendorStockType == _vendorStockTypes.SyncVendorStockTypes("BSCINCOMPL")
                                   select vs).FirstOrDefault();

      if (_record.Field<int?>("QuantityBSCINCOMPL").HasValue && _record.Field<decimal?>("CostPriceBSC").HasValue)
      {
        if (bscBSCIMCOMPL == null)
        {
          bscBSCIMCOMPL = new VendorStock()
          {
            VendorID = _vendor.VendorID,
            ProductID = _product.ProductID,
            VendorStockType = _vendorStockTypes.SyncVendorStockTypes("BSCINCOMPL")
          };
          repoVendorStock.Add(bscBSCIMCOMPL);
          _vendorStock.Add(bscBSCIMCOMPL);
        }
        bscBSCIMCOMPL.QuantityOnHand = _record.Field<int>("QuantityBSCINCOMPL");
        bscBSCIMCOMPL.UnitCost = _record.Field<decimal>("CostPriceBSC");

        bscBSCIMCOMPL.ConcentratorStatusID = UpdateVendorStatuses(_record.Field<string>("StockStatus"));
        bscBSCIMCOMPL.VendorStatus = _record.Field<string>("StockStatus");
        bscBSCIMCOMPL.StockStatus = _record.Field<string>("StockStatus");
      }
      else
      {
        if (bsc != null)
        {
          repoVendorStock.Delete(bscBSCIMCOMPL);
          _vendorStock.Remove(bscBSCIMCOMPL);
        }
      }
      #endregion

      #region BSC BSCRETURN
      VendorStock bscBSCRETURN = (from vs in _vendorStock
                                  where vs.ProductID == _product.ProductID
                                        &&
                                        vs.VendorID == _vendor.VendorID
                                        &&
                                        vs.VendorStockType == _vendorStockTypes.SyncVendorStockTypes("BSCRETURN")
                                  select vs).FirstOrDefault();

      if (_record.Field<int?>("QuantityBSCRETURN").HasValue && _record.Field<decimal?>("CostPriceBSC").HasValue)
      {
        if (bscBSCRETURN == null)
        {
          bscBSCRETURN = new VendorStock()
          {
            VendorID = _vendor.VendorID,
            ProductID = _product.ProductID,
            VendorStockType = _vendorStockTypes.SyncVendorStockTypes("BSCRETURN")
          };
          repoVendorStock.Add(bscBSCRETURN);
          _vendorStock.Add(bscBSCRETURN);
        }
        bscBSCRETURN.QuantityOnHand = _record.Field<int>("QuantityBSCRETURN");
        bscBSCRETURN.UnitCost = _record.Field<decimal>("CostPriceBSC");

        bscBSCRETURN.ConcentratorStatusID = UpdateVendorStatuses(_record.Field<string>("StockStatus"));
        bscBSCRETURN.VendorStatus = _record.Field<string>("StockStatus");
        bscBSCRETURN.StockStatus = _record.Field<string>("StockStatus");
      }
      else
      {
        if (bscBSCRETURN != null)
        {
          repoVendorStock.Delete(bscBSCRETURN);
          _vendorStock.Remove(bscBSCRETURN);
        }
      }
      #endregion

      #region BSC BSCUSED
      VendorStock bscUsed = (from vs in _vendorStock
                             where vs.ProductID == _product.ProductID
                                   &&
                                   vs.VendorID == _vendor.VendorID
                                   &&
                                   vs.VendorStockType == _vendorStockTypes.SyncVendorStockTypes("BSCUSED")
                             select vs).FirstOrDefault();

      if (_record.Field<int?>("QuantityBSCUSED").HasValue && _record.Field<decimal?>("CostPriceBSC").HasValue)
      {
        if (bscUsed == null)
        {
          bscUsed = new VendorStock()
          {
            VendorID = _vendor.VendorID,
            ProductID = _product.ProductID,
            VendorStockType = _vendorStockTypes.SyncVendorStockTypes("BSCUSED")
          };
          repoVendorStock.Add(bscUsed);
          _vendorStock.Add(bscUsed);
        }
        bscUsed.QuantityOnHand = _record.Field<int>("QuantityBSCUSED");
        bscUsed.UnitCost = _record.Field<decimal>("CostPriceBSC");

        bscUsed.ConcentratorStatusID = UpdateVendorStatuses(_record.Field<string>("StockStatus"));
        bscUsed.VendorStatus = _record.Field<string>("StockStatus");
        bscUsed.StockStatus = _record.Field<string>("StockStatus");
      }
      else
      {
        if (bscUsed != null)
        {
          repoVendorStock.Delete(bscUsed);
          _vendorStock.Remove(bscUsed);
        }
      }
      #endregion
    }

    private void ParseProductPrices(VendorAssortment assortment)
    {
      #region VendorPrice
      VendorPrice vendorPrice = (from vp in _vendorPrices
                                 where vp.VendorAssortmentID == assortment.VendorAssortmentID
&& vp.MinimumQuantity == (_record.Field<int>("MinimumQuantity") > 0 ? _record.Field<int>("MinimumQuantity") : 0)
                                 select vp).FirstOrDefault();
      if (vendorPrice == null)
      {
        vendorPrice = new VendorPrice()
        {
          VendorAssortment = assortment,
          MinimumQuantity = (_record.Field<int>("MinimumQuantity") > 0 ? _record.Field<int>("MinimumQuantity") : 0)
        };
        _vendorPrices.Add(vendorPrice);
        _unit.Scope.Repository<VendorPrice>().Add(vendorPrice);
      }

      vendorPrice.Price = _record.Field<decimal>("UnitPrice");
      vendorPrice.CostPrice = _record.Table.Columns.Contains("CostPrice") ? _record.Field<decimal?>("CostPrice") : null;
      if (!vendorPrice.CostPrice.HasValue)
        vendorPrice.CostPrice = _record.Table.Columns.Contains("CostPriceDC10") ? _record.Field<decimal?>("CostPriceDC10") : null;

      vendorPrice.TaxRate = (decimal)(_record.Field<double>("TaxRate"));
      vendorPrice.CommercialStatus = _record.Field<string>("CommercialStatus");
      vendorPrice.ConcentratorStatusID = UpdateVendorStatuses(_record.Field<string>("CommercialStatus"));
      #endregion
    }

    private void ParseStockLevels()
    {

      var promisedDeliveryDate = _record.Field<DateTime?>("PromisedDeliveryDate");
      var quantityToReceive = _record.Field<double?>("QuantityToReceive");

      VendorStock vendorStock = (from vs in _vendorStock
                                 where vs.ProductID == _product.ProductID
                                 && vs.VendorStockType == _vendorStockTypes.SyncVendorStockTypes("Assortment")
                                 && vs.VendorID == _vendor.VendorID
                                 select vs).FirstOrDefault();
      if (vendorStock == null)
      {
        vendorStock = new VendorStock()
        {
          ProductID = _product.ProductID,
          VendorID = _vendor.VendorID,
          VendorStockTypeID = 1

        };
        _unit.Scope.Repository<VendorStock>().Add(vendorStock);
        _vendorStock.Add(vendorStock);
      }


      vendorStock.ProductID = _product.ProductID;
      vendorStock.QuantityOnHand = (int)_record.Field<double>("QuantityOnHand");
      vendorStock.PromisedDeliveryDate = promisedDeliveryDate;
      vendorStock.VendorID = _vendor.VendorID;
      vendorStock.QuantityToReceive = quantityToReceive.Try<double?, int?>(c => int.Parse(c.Value.ToString()), null);
      vendorStock.ConcentratorStatusID = UpdateVendorStatuses(_record.Field<string>("StockStatus"));
      vendorStock.VendorStatus = _record.Field<string>("StockStatus");
      vendorStock.StockStatus = _record.Field<string>("StockStatus");
    }

    private int UpdateVendorStatuses(string stockStatus)
    {
      if (string.IsNullOrEmpty(stockStatus)) return -1;

      var vProductStatus = (from vs in _vProductStatus
                            where (vs.VendorID == _vendor.VendorID ||
                              _vendor.ParentVendorID.HasValue && vs.VendorID == _vendor.ParentVendorID) && vs.VendorStatus == stockStatus.ToUpper()
                            select vs).ToList();

      if (vProductStatus.Count < 1)
      {
        var svProductStatus = new VendorProductStatus()
                           {
                             VendorID = _vendor.VendorID,
                             VendorStatus = stockStatus,
                             //ProductStatus = ProductStatus.NeedsMatch,
                             ConcentratorStatusID = -1
                           };
        _unit.Scope.Repository<VendorProductStatus>().Add(svProductStatus);
        _unit.Save();

        _vProductStatus.Add(svProductStatus);
        return svProductStatus.ConcentratorStatusID;
      }
      if (vProductStatus.Count == 1)
        return vProductStatus.FirstOrDefault().ConcentratorStatusID;

      return vProductStatus.Where(x => x.VendorID == _vendor.VendorID ||
_vendor.ParentVendorID.HasValue && x.VendorID == _vendor.ParentVendorID).OrderByDescending(x => x.ConcentratorStatusID).FirstOrDefault().ConcentratorStatusID;
    }

    private VendorStock ParseRetailStockLevels(int retailVendorID, DataRow dr, List<VendorStock> vendorstockList)
    {

      var stockRepo = _unit.Scope.Repository<VendorStock>();

      var stockType = _vendorStockTypes.SyncVendorStockTypes("Assortment");

      VendorStock vendorStock = vendorstockList.FirstOrDefault(vs => vs.ProductID == _product.ProductID && vs.VendorStockTypeID == stockType.VendorStockTypeID);
      if (vendorStock == null)
      {
        vendorStock = new VendorStock()
        {
          ProductID = _product.ProductID,
          VendorID = retailVendorID,
          VendorStockType = _vendorStockTypes.SyncVendorStockTypes("Assortment")

        };
        stockRepo.Add(vendorStock);
      }

      //vendorStock.ProductID = _product.ProductID;
      vendorStock.QuantityOnHand = dr.Field<int>("InStock");
      //vendorStock.VendorID = retailVendorID;
      return vendorStock;
    }

    #endregion

    #region Public methods

    /// <summary>
    /// updates regular stock levels
    /// MyCom, BasWeb, BasBE
    /// </summary>
    public void UpdateStockLevels(DataRow dr, log4net.ILog log)
    {
      LoadAssortmentProduct(dr, log, false);

      if (this._product != null)
      {
        try
        {
          ParseStockLevels();
        }
        catch (Exception e)
        {
          _log.Debug("Updating stock levels failed: ", e);
        }
      }
    }

    /// <summary>
    /// updates retail stock levels
    /// MyCom shops
    /// </summary>
    public VendorStock UpdateRetailStockLevels(int retailVendorID, DataRow dr, log4net.ILog log, List<VendorStock> vendorstock)
    {
      LoadAssortmentProduct(dr, log, true);

      if (this._product != null)
      {
        try
        {
          return ParseRetailStockLevels(retailVendorID, dr, vendorstock);
        }
        catch (Exception e)
        {
          _log.Debug("Updating stock levels failed: ", e);
          //TODO: Change conflicts

          //foreach (var conflict in _context.ChangeConflicts)
          //{
          //  conflict.Resolve(System.Data.Linq.RefreshMode.KeepChanges);
          //}
          //_context.SubmitChanges();
        }
      }
      return null;
    }

    /// <summary>
    /// updates regular stock levels and vendor BSC stock levels
    /// MyComVeiling
    /// </summary>
    public void UpdateBSCStockLevels(DataRow dr, log4net.ILog log)
    {
      LoadAssortmentProduct(dr, log, false);
      if (this._product != null)
      {
        try
        {
          ParseBSCStockLevels();
        }
        catch (Exception e)
        {
          _log.Debug("Updating stock levels failed :", e);
        }
      }
    }

    /// <summary>
    /// updates prices
    /// </summary>
    public void UpdatePrices(DataRow dr, log4net.ILog log)
    {
      LoadAssortmentProduct(dr, log, false);

      if (this._product != null)
      {
        try
        {
          VendorAssortment assortment = (from a in _vendorAssortment
                                         where a.ProductID == _product.ProductID
                                         select a).FirstOrDefault();
          if (assortment != null)
          {
            ParseProductPrices(assortment);
          }
        }
        catch (Exception e)
        {
          _log.Debug("Updating stock levels failed :", e);
        }
      }
    }


    //public void ParseAssortment(log4net.ILog log)
    //{
    //  try
    //  {
    //    //parse productGroup
    //    ParseProductGroupVendor();
    //    ParseProduct(log);
    //  }
    //  catch (Exception e)
    //  {
    //    _log.Debug("Import of assortment failed:", e);
    //  }
    //}

    #endregion
  }
}
