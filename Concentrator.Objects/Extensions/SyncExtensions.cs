using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Products;
using System.Data.Objects;
using Concentrator.Objects.DataAccess.Repository;


namespace Concentrator.Objects.Extensions
{
  //TO DO: Change from ObjectSet to abstraction
  public class ProductGroupSyncer
  {
    private Type _pgvType = typeof(ProductGroupVendor);

    public ProductGroupSyncer(int vendorID, IRepository<ProductGroupVendor> repo)
    {
      Repo = repo;
      VendorID = vendorID;
      ProductGroups = repo.GetAll(c => c.VendorID == vendorID).ToList();
    }

    public List<ProductGroupVendor> ProductGroups { get; set; }

    public IRepository<ProductGroupVendor> Repo { get; set; }

    public int VendorID { get; set; }

    private ProductGroupVendor Traverse(int idx, List<ProductGroupNameCode> codes)
    {
      var pi = _pgvType.GetProperty(String.Format("VendorProductGroupCode{0}", idx + 1));

      var productGroup = ProductGroups.FirstOrDefault(x => pi.GetValue(x, null) != null && pi.GetValue(x, null).ToString() == codes[idx].code);
      if (productGroup == null)
      {
        productGroup = new ProductGroupVendor
        {
          VendorID = VendorID,
          ProductGroupID = -1
        };

        pi.SetValue(productGroup, codes[idx].code, null);

        if (!String.IsNullOrEmpty(codes[idx].name) && idx == 0)
          productGroup.VendorName = codes[idx].name;


        ProductGroups.Add(productGroup);
        Repo.Add(productGroup);
      }


      int newIdx = idx + 1;
      while (newIdx < codes.Count && codes[idx].code == codes[newIdx].code)
      {
        newIdx++;
      }

      return productGroup;
    }
  }

  public struct ProductGroupNameCode
  {
    public string code;
    public string name;
  }

}
