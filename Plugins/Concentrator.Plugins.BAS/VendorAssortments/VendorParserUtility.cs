using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Concentrator.Objects;
using log4net;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Brands;

namespace Concentrator.Plugins.BAS
{
  public class VendorParserUtility
  {
    private IUnitOfWork _unit;
    private int _importVendorID;
    private int? _parentVendorID;
    private ILog _log;

    public VendorParserUtility(IUnitOfWork unit, int vendorID, ILog log)
    {
      _unit = unit;
      this._importVendorID = vendorID;
      this._log = log;

      this._parentVendorID = _unit.Scope.Repository<Vendor>().GetSingle(v => v.VendorID == vendorID).ParentVendorID;
    }

    private List<BrandVendor> _brandVendors;
    public List<BrandVendor> BrandVendors
    {
      get
      {

        if (_brandVendors == null)
        {
          _brandVendors = _unit.Scope.Repository<BrandVendor>().GetAll().ToList();

        }
        return _brandVendors;
      }
      set { _brandVendors = value; }
    }

    /// <summary>
    /// Cleares all records in the db which are not in the received dataset
    /// </summary>
    /// <param name="context"></param>
    /// <param name="content"></param>
    /// <param name="ImportVendorID"></param>
    public void SyncUneeded(DataSet content, int ImportVendorID)
    {
      #region Vendor Assortment

      var itemNumbers = (from r in content.Tables[0].AsEnumerable()
                         select new
                         {
                           VendorItemNumber = r.Field<string>("VendorItemNumber").Trim().ToUpper(),
                           BrandID = GetBrandVendor(r.Field<string>("Brand").Trim())
                         }).ToList();


      List<VendorAssortment> unused = _unit.Scope.Repository<VendorAssortment>().GetAllAsQueryable(c => c.VendorID == ImportVendorID).ToList();


      foreach (var p in itemNumbers)
      {
        unused.RemoveAll(c => p.BrandID == c.Product.BrandID && p.VendorItemNumber == c.Product.VendorItemNumber);
      }

      //unused.RemoveAll(c => itemNumbers.Contains( (c.Product.VendorItemNumber.Trim()));

      #endregion
      if (unused != null && unused.Count > 0)
      {
        unused.ForEach(x => x.IsActive = false);

        //context.VendorAssortments.DeleteAllOnSubmit(unused);
        //context.VendorProductGroupAssortments.DeleteAllOnSubmit(unused.SelectMany(c => c.VendorProductGroupAssortments).ToList());
        #region Vendor Stock

        //context.VendorStock.DeleteAllOnSubmit(unused.SelectMany(vd => vd.VendorStock.AsEnumerable()));
        //var stockRepo = _unit.Scope.Repository<VendorStock>().GetAll();
        //unused.ForEach((assortment) =>
        //{
        //  var a = stockRepo.FirstOrDefault(c => c.VendorID == assortment.VendorID && c.ProductID == assortment.ProductID);
        //  if (a != null)
        //    _unit.Scope.Repository<VendorStock>().Delete(a);
        //});


        #endregion
      }
      _unit.Save();
    }

    private int? GetBrandVendor(string vendorBrandCode)
    {
      int? brandID = (from b in BrandVendors
                      where b.VendorBrandCode == vendorBrandCode.Trim()
                      && (b.VendorID == _importVendorID || (_parentVendorID.HasValue && b.VendorID == _parentVendorID.Value))
                      select b.BrandID).OrderByDescending(x => x).FirstOrDefault();

      return brandID;
    }
  }
}
