using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Xml;
using System.Xml.Linq;
using Spider.Objects;
using Spider.Objects.Selector;

namespace Spider.Web.SelectorService
{
  /// <summary>
  /// Summary description for SelectorService
  /// </summary>
  [WebService(Namespace = "http://spider.selectorservice.nl/")]
  [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
  [System.ComponentModel.ToolboxItem(false)]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  [System.Web.Script.Services.ScriptService]
  public class SelectorService : System.Web.Services.WebService
  {
    [WebMethod]
    public List<int> GetSelectors()
    {
      using (ConcentratorDataContext context = new ConcentratorDataContext())
      {
        return (from s in context.Selectors select s.SelectorID).ToList();
      }
    }

    public List<int> FindProduct(int selector, string query)
    {
      using (ConcentratorDataContext context = new ConcentratorDataContext())
      {
        return (from p in context.Products
                where
                    p.ProductDescription.Any(pd =>
                      pd.ShortContentDescription.Contains(query) ||
                      pd.LongContentDescription.Contains(query))
                    ||
                    p.ProductBarcodes.Any(pb =>
                      pb.Barcode.Contains(query) ||
                      pb.Barcode == query)
                select p.ProductID).ToList();

      }
    }

    public List<int> FindCompatibleProducts(int selectorID, int productID)
    {
      using (ConcentratorDataContext context = new ConcentratorDataContext())
      {
        var selector = (from s in context.Selectors where s.SelectorID == selectorID select s).FirstOrDefault();
        
        //var products = (from rp in context.RelatedProducts 
        //                where rp.ProductID == productID
        //                &&
        //                context.ProductGroupSelectors.Where(c=>c.SelectorID == selectorID).Select(c => c.ProductGroupVendorID).ToList().(
        //                  (from va in context.VendorAssortments where va.VendorID == selector.VendorID select va.VendorProductGroupAssortments.Select( c=> c.ProductGroupVendorID).ToList())
        //                ))
        var allowedVendorProductGroups =
          (from pg in context.ProductGroupSelectors 
           where pg.SelectorID == selectorID && pg.isAssortment.Try(c=>c.Value, false) select pg.ProductGroupVendorID).ToList();
        
        var products = (from rp in context.RelatedProducts
                        where rp.ProductID == productID
                          && allowedVendorProductGroups.(
                            from va in context.VendorAssortments
                             where
                               va.VendorID == selector.VendorID
                               &&
                               va.ProductID == rp.ProductID
                             select va.VendorProductGroupAssortments.Select(c => c.ProductGroupVendorID).ToList()
                          )
                        select rp.RelatedProductID
                          ).ToList();
        
      }
    }
    
  }
}
