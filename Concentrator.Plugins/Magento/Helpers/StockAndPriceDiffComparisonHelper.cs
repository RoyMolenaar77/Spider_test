using Concentrator.Objects.AssortmentService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using ServiceStack.Text;
using System.IO;

namespace Concentrator.Plugins.Magento.Helpers
{
  public class StockAndPriceDiffComparisonHelper
  {
    private XDocument _currentAssortmentDocument;
    public List<AssortmentStockPriceProduct> CurrentFullAssortment;
    private List<AssortmentStockPriceProduct> OldFullAssortment;
    private const string STOCK_ARCHIVE_FILE_NAME = "LastStockExport.xml";
    private string _serializationPath;

    /// <summary>
    /// Gets the products that need to be processed by the export
    /// </summary>
    public List<AssortmentStockPriceProduct> ProductsToProcess { get; set; }

    /// <summary>
    /// Gets the products that need to be deactivated
    /// </summary>
    public List<AssortmentStockPriceProduct> ProductsToDelete { get; set; }

    public StockAndPriceDiffComparisonHelper(List<AssortmentStockPriceProduct> currentAssortment, string serializationPath)
    {
      _serializationPath = serializationPath;
      CurrentFullAssortment = currentAssortment;
      _currentAssortmentDocument = SerializeAssortment(currentAssortment);

      var lastExportedAssortmentDocument = LoadLastExportedAssortment(serializationPath);

      if (lastExportedAssortmentDocument != null)
        OldFullAssortment = DeserializeAssortment(lastExportedAssortmentDocument);

      DiffAssortment(lastExportedAssortmentDocument, _currentAssortmentDocument);
    }

    private void DiffAssortment(XDocument lastExportAssortment, XDocument currentExportAssortment)
    {
      XNamespace docNs = "http://schemas.datacontract.org/2004/07/Concentrator.Objects.AssortmentService";

      var currentProducts = currentExportAssortment.Root.Elements(docNs + "AssortmentStockPriceProduct").ToLookup(c => int.Parse(c.Element(docNs + "ProductID").Value));

      // if the last exported assortment was not saved for any reason, run a full import
      if (lastExportAssortment == null)
      {
        ProductsToProcess = CurrentFullAssortment;
      }
      else
      {
        var lastProducts = lastExportAssortment.Root.Elements(docNs + "AssortmentStockPriceProduct").ToLookup(c => int.Parse(c.Element(docNs + "ProductID").Value));


        //get diffs
        var newProductsIDsToProcess = (from p in currentProducts
                                       where !lastProducts.Contains(p.Key)
                                       select p.Key).ToList();

        var deletedProducts = (from p in lastProducts
                               where !currentProducts.Contains(p.Key)
                               select p.Key).ToList();

        var changedProductIDs = (from p in currentProducts
                                 where lastProducts.Contains(p.Key)
                                 && !XElement.DeepEquals(p.First(), lastProducts[p.Key].First())
                                 select p.Key).ToList();

        var changedProductsCollection = newProductsIDsToProcess.Union(changedProductIDs).ToList();

        //check parent/vs children -> we want to process a "full" product (the parent + the children) as soon as something in one of the products has changed
        var productsSorted = new SortedList<int, AssortmentStockPriceProduct>(CurrentFullAssortment.ToDictionary(c => c.ProductID, c => c));

        foreach (var productID in changedProductIDs.Union(newProductsIDsToProcess).ToList())
        {
          var originalProduct = productsSorted[productID];
          if (originalProduct.IsConfigurable) //if configurable -> load all children
          {
            var childIDs = originalProduct.RelatedProducts.Where(c => c.IsConfigured).Select(c => c.RelatedProductID).ToList();
            changedProductsCollection = changedProductsCollection.Union(childIDs).ToList();
          }
          else
          {
            //make sure parent is loaded as well + remaining children
            var parent = productsSorted.Values.Where(c => c.IsConfigurable && c.RelatedProducts.Any(l => l.IsConfigured && l.RelatedProductID == productID)).FirstOrDefault();

            if (parent != null) //if there is a parent configurable product. If not -> it is a standalone simple product 
            {
              var changedIDs = new List<int>() { parent.ProductID };
              changedIDs = changedIDs.Union(parent.RelatedProducts.Where(c => c.IsConfigured).Select(c => c.RelatedProductID)).ToList();
              changedProductsCollection = changedProductsCollection.Union(changedIDs).ToList();
            }
          }
        }

        ProductsToProcess = CurrentFullAssortment.Where(c => changedProductsCollection.Contains(c.ProductID)).ToList();
        ProductsToDelete = OldFullAssortment.Where(c => deletedProducts.Contains(c.ProductID)).ToList();
      }
    }


    private XDocument LoadLastExportedAssortment(string serializationPath)
    {
      var lastProductExportPath = Path.Combine(serializationPath, STOCK_ARCHIVE_FILE_NAME);

      if (File.Exists(lastProductExportPath))
      {
        return XDocument.Load(lastProductExportPath);
      }
      return null;
    }

    private XDocument SerializeAssortment(List<AssortmentStockPriceProduct> currentAssortment)
    {
      var currentAssortmentXml = currentAssortment.ToXml();

      return XDocument.Parse(currentAssortmentXml);
    }


    private List<AssortmentStockPriceProduct> DeserializeAssortment(XDocument assortmentDocument)
    {
      var lastAssortment = assortmentDocument.ToString().FromXml<List<AssortmentStockPriceProduct>>();
      return lastAssortment;
    }

    public void ArchiveExportedAssortment()
    {
      var lastProductExportPath = Path.Combine(_serializationPath, STOCK_ARCHIVE_FILE_NAME);
      _currentAssortmentDocument.Save(lastProductExportPath);

    }
  }
}
