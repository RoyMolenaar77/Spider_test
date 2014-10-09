using Concentrator.Objects.AssortmentService;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Concentrator.Plugins.Magento.Helpers
{
  public class ProductDiffComparisonHelper
  {
    private XDocument _currentAssortmentDocument;
    public List<AssortmentProductInfo> CurrentFullAssortment;
    private List<AssortmentProductInfo> OldFullAssortment;
    private XDocument _attributesDocument;
    private XDocument _contentDocument;

    private string _serializationPath;


    private const string PRODUCT_ARCHIVE_FILE_NAME = "LastProductExport.xml";
    private const string ATTRIBUTES_ARCHIVE_FILE_NAME = "LastAttributeExport.xml";
    private const string CONTENT_ARCHIVE_FILE_NAME = "LastContentExport.xml";
    /// <summary>
    /// Gets the products that need to be processed by the export
    /// </summary>
    public List<AssortmentProductInfo> ProductsToProcess { get; set; }

    /// <summary>
    /// Gets the products that need to be deactivated
    /// </summary>
    public List<AssortmentProductInfo> ProductsToDelete { get; set; }

    public ProductDiffComparisonHelper(List<AssortmentProductInfo> currentAssortment, XDocument contentDocument, XDocument attributesDocument, string serializationPath)
    {
      _serializationPath = serializationPath;
      CurrentFullAssortment = currentAssortment;

      _currentAssortmentDocument = SerializeAssortment(currentAssortment); //serialize assortment for later diffing


      _attributesDocument = attributesDocument;
      _contentDocument = contentDocument;

      var lastExportedAssortmentDocument = LoadLastExportedAssortment(serializationPath);
      var lastExportedAttributesDocument = LoadLastExportedAttributes(serializationPath);
      var lastExportedContentDocument = LoadLastExportedContent(serializationPath);

      if (lastExportedAssortmentDocument != null)
        OldFullAssortment = DeserializeAssortment(lastExportedAssortmentDocument);

      DiffAssortmentAndInitialize(lastExportedAssortmentDocument, _currentAssortmentDocument);
      DiffAttributes(lastExportedAttributesDocument, _attributesDocument);
      DiffContent(lastExportedContentDocument, _contentDocument);

    }

    private void DiffAttributes(XDocument lastExportedAttributesDocument, XDocument currentExportedAttributesDocument)
    {

      var currentAttributeProducts = new SortedList<int, XElement>(currentExportedAttributesDocument.Root.Elements("ProductAttribute").ToDictionary(c => int.Parse(c.Attribute("ProductID").Value), c => c));

      //if the last exported att
      if (lastExportedAttributesDocument == null)
      {
        ProductsToProcess = CurrentFullAssortment; //if no last exported product
      }
      else
      {
        //check for filter groups
        var filterGroupsNew = _currentAssortmentDocument.Root.Element("AttributeValueGroups");
        var filterGroupsOld = _currentAssortmentDocument.Root.Element("AttributeValueGroups");

        if (!XElement.DeepEquals(filterGroupsNew, filterGroupsOld))
        {
          ProductsToProcess = CurrentFullAssortment; //if no last exported product
          return;
        }
        var lastAttributeProducts = currentExportedAttributesDocument.Root.Elements("ProductAttribute").ToLookup(c => int.Parse(c.Attribute("ProductID").Value));

        var changedProductIDs = (from p in currentAttributeProducts
                                 where lastAttributeProducts.Contains(p.Key)
                                 && !XElement.DeepEquals(p.Value, lastAttributeProducts[p.Key].First())
                                 select p.Key).ToList();

        ProductsToProcess = ProductsToProcess.Union(CurrentFullAssortment.Where(c => changedProductIDs.Contains(c.ProductID))).ToList();
      }
    }

    private void DiffContent(XDocument lastExportedContentDocument, XDocument currentExportedContentDocument)
    {
      var currentAttributeProducts = new SortedList<int, XElement>(currentExportedContentDocument.Root.Elements("Product").ToDictionary(c => int.Parse(c.Attribute("ProductID").Value), c => c));

      if (lastExportedContentDocument == null)
      {
        ProductsToProcess = CurrentFullAssortment; //if no last exported product
      }
      else
      {
        var lastAttributeProducts = lastExportedContentDocument.Root.Elements("Product").ToLookup(c => int.Parse(c.Attribute("ProductID").Value));
        var changedProductIDs = (from p in currentAttributeProducts
                                 where lastAttributeProducts.Contains(p.Key)
                                 && !XElement.DeepEquals(p.Value, lastAttributeProducts[p.Key].First())
                                 select p.Key).ToList();

        ProductsToProcess = ProductsToProcess.Union(CurrentFullAssortment.Where(c => changedProductIDs.Contains(c.ProductID))).ToList();
      }
    }

    private void DiffAssortmentAndInitialize(XDocument lastExportAssortment, XDocument currentExportAssortment)
    {
      XNamespace docNs = "http://schemas.datacontract.org/2004/07/Concentrator.Objects.AssortmentService";

      var currentProducts = currentExportAssortment.Root.Elements(docNs + "AssortmentProductInfo").ToLookup(c => int.Parse(c.Element(docNs + "ProductID").Value));

      // if the last exported assortment was not saved for any reason, run a full import
      if (lastExportAssortment == null)
      {
        ProductsToProcess = CurrentFullAssortment;
      }
      else
      {
        SortedList<int, XElement> lastProducts = new SortedList<int, XElement>(lastExportAssortment.Root.Elements(docNs + "AssortmentProductInfo").ToDictionary(c => int.Parse(c.Element(docNs + "ProductID").Value), c => c));


        //get diffs
        var newProductsIDsToProcess = (from p in currentProducts
                                       where !lastProducts.ContainsKey(p.Key)
                                       select p.Key).ToList();

        var deletedProducts = (from p in lastProducts
                               where !currentProducts.Contains(p.Key)
                               select p.Key).ToList();

        var changedProductIDs = (from p in currentProducts
                                 where lastProducts.ContainsKey(p.Key)
                                 && !XElement.DeepEquals(p.First(), lastProducts[p.Key])
                                 select p.Key).ToList();

        var changedProductsCollection = newProductsIDsToProcess.Union(changedProductIDs).ToList();

        //check parent/vs children -> we want to process a "full" product (the parent + the children) as soon as something in one of the products has changed
        var productsSorted = new SortedList<int, AssortmentProductInfo>(CurrentFullAssortment.ToDictionary(c => c.ProductID, c => c));

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

    public void ArchiveExportedAssortment()
    {
      var lastProductExportPath = Path.Combine(_serializationPath, PRODUCT_ARCHIVE_FILE_NAME);
      _currentAssortmentDocument.Save(lastProductExportPath);

      var lastAttributeExportPath = Path.Combine(_serializationPath, ATTRIBUTES_ARCHIVE_FILE_NAME);
      _attributesDocument.Save(lastAttributeExportPath);

      var lastContentExportPath = Path.Combine(_serializationPath, CONTENT_ARCHIVE_FILE_NAME);
      _contentDocument.Save(lastContentExportPath);
    }

    private XDocument LoadLastExportedContent(string serializationPath)
    {
      var lastProductExportPath = Path.Combine(serializationPath, CONTENT_ARCHIVE_FILE_NAME);

      if (File.Exists(lastProductExportPath))
      {
        return XDocument.Load(lastProductExportPath);
      }
      return null;
    }

    private XDocument LoadLastExportedAttributes(string serializationPath)
    {
      var lastProductExportPath = Path.Combine(serializationPath, ATTRIBUTES_ARCHIVE_FILE_NAME);

      if (File.Exists(lastProductExportPath))
      {
        return XDocument.Load(lastProductExportPath);
      }
      return null;
    }

    private XDocument LoadLastExportedAssortment(string serializationPath)
    {
      var lastProductExportPath = Path.Combine(serializationPath, PRODUCT_ARCHIVE_FILE_NAME);

      if (File.Exists(lastProductExportPath))
      {
        return XDocument.Load(lastProductExportPath);
      }
      return null;
    }

    private XDocument SerializeAssortment(List<AssortmentProductInfo> currentAssortment)
    {
      var currentAssortmentXml = currentAssortment.ToXml();

      return XDocument.Parse(currentAssortmentXml);
    }

    private List<AssortmentProductInfo> DeserializeAssortment(XDocument assortmentDocument)
    {
      var lastAssortment = assortmentDocument.ToString().FromXml<List<AssortmentProductInfo>>();
      return lastAssortment;
    }
  }
}
