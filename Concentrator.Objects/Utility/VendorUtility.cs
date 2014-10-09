using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.DataAccess.Repository;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Products;
using Microsoft.Practices.ServiceLocation;
using Concentrator.Objects.DataAccess.UnitOfWork;


namespace Concentrator.Objects.Utility
{
  public static class VendorUtility
  {
    /// <summary>
    /// Retrieves all vendor settings
    /// </summary>
    /// <param name="context"></param>
    /// <param name="vendorID"></param>
    /// <returns></returns>
    public static List<VendorSetting> GetVendorSettings(IRepository<VendorSetting> repo, int vendorID)
    {
      return repo.GetAll(c => c.VendorID == vendorID).ToList();
    }

    /// <summary>
    /// inserts or modifies a brand
    /// </summary>
    /// <param name="context"></param>
    /// <param name="vendorID"></param>
    public static BrandVendor MergeBrand(IRepository<BrandVendor> repo, Vendor vendor, string vendorBrandCode, Dictionary<string, BrandVendor> brandVendorList)
    {
      BrandVendor brandVendor = null;

      if (brandVendorList.ContainsKey(vendorBrandCode.Trim()))
        brandVendor = brandVendorList[vendorBrandCode.Trim()];

      if (brandVendor == null)
      {
        //brandVendor = repo.GetSingle(b => b.VendorBrandCode == vendorBrandCode.Trim() && (b.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && b.VendorID == vendor.ParentVendorID)));

        brandVendor = (from b in repo.GetAllAsQueryable()
                     where b.VendorBrandCode == vendorBrandCode.Trim()
                     && (b.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && b.VendorID == vendor.ParentVendorID))
                     select b).OrderByDescending(x => x.BrandID).FirstOrDefault();

        if (brandVendor == null)
        {
          brandVendor = new BrandVendor()
                          {
                            BrandID = -1,
                            VendorID = vendor.ParentVendorID.HasValue ? vendor.ParentVendorID.Value : vendor.VendorID,
                            VendorBrandCode = vendorBrandCode.Trim()
                          };

          repo.Add(brandVendor);
        }

        brandVendorList.Add(vendorBrandCode.Trim(), brandVendor);
      }

      return brandVendor;
    }


    /*
     * Hierarchy :
     * Brand
     * Level 1 = GroupCode1
     * Level 2 = GroupCode2
     * Level 3 = GroupCode3
    
     */
    /// <summary>
    /// insert or update productgroupvendor
    /// </summary>
    public static List<ProductGroupVendor> MergeProductGroupVendor(List<ProductGroupVendor> productGroupVendorList, Vendor vendor, string brandCode, string groupCode1, string groupCode2, string groupCode3, IUnitOfWork unit)
    {
      brandCode = brandCode != null ? brandCode.Trim() : null;
      groupCode1 = groupCode1 != null ? groupCode1.Trim() : null;
      groupCode2 = groupCode2 != null ? groupCode2.Trim() : null;
      groupCode3 = groupCode3 != null ? groupCode3.Trim() : null;

      var sourceRecords = productGroupVendorList;

      #region Product group

      var group =
        sourceRecords.FirstOrDefault(x => x.VendorProductGroupCode1 == groupCode1);
      if (group == null)
      {
        group = new ProductGroupVendor()
        {
          VendorID = vendor.ParentVendorID.HasValue ? vendor.ParentVendorID.Value : vendor.VendorID,
          VendorProductGroupCode1 = groupCode1,
          VendorName = string.Empty,
          ProductGroupID = -1
        };
        productGroupVendorList.Add(group);
        unit.Scope.Repository<ProductGroupVendor>().Add(group);
        unit.Save();
      }

      group =
        sourceRecords.FirstOrDefault(x => x.VendorProductGroupCode2 == groupCode2);
      if (group == null)
      {
        group = new ProductGroupVendor()
        {
          VendorID = vendor.ParentVendorID.HasValue ? vendor.ParentVendorID.Value : vendor.VendorID,
          VendorProductGroupCode2 = groupCode2,
          VendorName = string.Empty,
          ProductGroupID = -1
        };
        productGroupVendorList.Add(group);
        unit.Scope.Repository<ProductGroupVendor>().Add(group);
        unit.Save();
      }

      group = sourceRecords.FirstOrDefault(x => x.VendorProductGroupCode3 == groupCode3);
      if (group == null)
      {
        group = new ProductGroupVendor()
        {
          VendorID = vendor.ParentVendorID.HasValue ? vendor.ParentVendorID.Value : vendor.VendorID,
          VendorProductGroupCode3 = groupCode3,
          VendorName = string.Empty,
          ProductGroupID = -1
        };
        productGroupVendorList.Add(group);
        unit.Scope.Repository<ProductGroupVendor>().Add(group);
        unit.Save();
      }

      group = sourceRecords.FirstOrDefault(x => x.BrandCode == brandCode);
      if (group == null)
      {
        group = new ProductGroupVendor()
        {
          VendorID = vendor.ParentVendorID.HasValue ? vendor.ParentVendorID.Value : vendor.VendorID,
          BrandCode = brandCode,
          VendorName = string.Empty,
          ProductGroupID = -1
        };
        productGroupVendorList.Add(group);
        unit.Scope.Repository<ProductGroupVendor>().Add(group);
        unit.Save();
      }

      #endregion Product group

      sourceRecords = productGroupVendorList;

      var records = (from l in sourceRecords
                     where
                         (l.BrandCode == null || (brandCode != null && l.BrandCode.Trim() == brandCode))
                         && (l.VendorProductGroupCode1 == null || (groupCode1 != null && l.VendorProductGroupCode1.Trim() == groupCode1))
                              && (l.VendorProductGroupCode2 == null || (groupCode2 != null && l.VendorProductGroupCode2.Trim() == groupCode2))
                                   && (l.VendorProductGroupCode3 == null || (groupCode3 != null && l.VendorProductGroupCode3.Trim() == groupCode3))
                     select l).ToList();

      if (records.Count == 0)
      {
        var productGroup = new ProductGroupVendor
                {
                  VendorID = vendor.VendorID,
                  VendorProductGroupCode1 = groupCode1,
                  VendorProductGroupCode2 = groupCode2,
                  VendorProductGroupCode3 = groupCode3,
                  BrandCode = brandCode,
                  ProductGroupID = -1,
                };
        productGroupVendorList.Add(productGroup);
        unit.Scope.Repository<ProductGroupVendor>().Add(productGroup);
        unit.Save();
      }
      return records;
    }

    public static List<ProductGroupVendor> GetProductGroupVendors(IRepository<ProductGroupVendor> repo, int vendorID, int? parentVendorID)
    {
      return repo.GetAll(p => p.VendorID == vendorID || (parentVendorID.HasValue && p.VendorID == parentVendorID)).ToList();
    }

    public static VendorAssortment GetMatchedVendorAssortment(IRepository<VendorAssortment> repo, int vendorID, int productID, bool onlyActive = true)
    {
      VendorAssortment va = null;

      using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
      {


        var funcRepo = ((IFunctionScope)unit.Scope).Repository();
        if (onlyActive)
          va = repo.GetSingle(x => (x.VendorID == vendorID || (x.Vendor.ParentVendorID.HasValue && x.Vendor.ParentVendorID.Value == vendorID)) && x.ProductID == productID && x.IsActive);
        else
          va = repo.GetSingle(x => (x.VendorID == vendorID || (x.Vendor.ParentVendorID.HasValue && x.Vendor.ParentVendorID.Value == vendorID)) && x.ProductID == productID);

        if (va == null)
        {
          var matches = funcRepo.FetchProductMatches(productID, vendorID).ToList();

          if (matches.Count > 0)
          {
            int matchProductID =matches.FirstOrDefault().ProductID;

            Product prod = unit.Scope.Repository<Product>().GetSingle(c => c.ProductID == matchProductID);
            va = prod.VendorAssortments.FirstOrDefault(x => x.VendorID == vendorID || (x.Vendor.ParentVendorID.HasValue && x.Vendor.ParentVendorID.Value == vendorID));
          }
        }
        return va;
      }
    }
  }
}
