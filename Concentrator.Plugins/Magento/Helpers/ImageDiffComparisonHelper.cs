using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Concentrator.Plugins.Magento.Helpers
{
  public class ImageDiffComparisonHelper
  {
    private string _serializationPath;
    private const string IMAGES_ARCHIVE_FILENAME = "LastImageExport.xml";
    private XDocument _currentImagesDocument;
    public List<XElement> ProductsToProcess { get; set; }

    /// <summary>
    /// All elements in the original xml with matching vendoritemnumbers are removed on archiving
    /// </summary>
    private SynchronizedCollection<XElement> _productImageElementsToRemoveOnArchive;

    public ImageDiffComparisonHelper(XDocument imagesDocument, string serializationPath)
    {
      _serializationPath = serializationPath;
      _currentImagesDocument = imagesDocument;

      var lastExportedImagesDocument = LoadLastExportedImages(serializationPath);
      _productImageElementsToRemoveOnArchive = new SynchronizedCollection<XElement>();
      DiffImages(lastExportedImagesDocument, _currentImagesDocument);
    }

    private void DiffImages(XDocument lastExportedImagesDocument, XDocument currentExportedImagesDocument)
    {

      //todo: detecting changes?

      var currentImages = currentExportedImagesDocument.Root.Element("Products").Elements("ProductMedia").ToLookup(c => int.Parse(c.Attribute("ProductID").Value));

      if (lastExportedImagesDocument == null)
      {
        ProductsToProcess = currentExportedImagesDocument.Root.Element("Products").Elements("ProductMedia").ToList();
      }
      else
      {
        var lastProducts = lastExportedImagesDocument.Root.Element("Products").Elements("ProductMedia").ToLookup(c => int.Parse(c.Attribute("ProductID").Value));


        var newProductsIDsToProcess = (from p in currentImages
                                       where !lastProducts.Contains(p.Key)
                                       select p.Key).ToList();

        var deletedProducts = (from p in currentImages
                               where !currentImages.Contains(p.Key)
                               select p.Key).ToList();

        var changedProductIDs = (from p in currentImages
                                 where lastProducts.Contains(p.Key)
                                 &&
                                 (
                                 p.Count() != lastProducts[p.Key].Count() ||
                                    (from g in p.ToList().OrderBy(c => c.Attribute("Sequence").Value)
                                     select XElement.DeepEquals(g, lastProducts[p.Key].FirstOrDefault(c => c.Attribute("Sequence").Value == g.Attribute("Sequence").Value && c.Attribute("MediaID").Value == g.Attribute("MediaID").Value))
                                    ).ToList().Any(c => !c)
                                )
                                 select p.Key).ToList();



        var changedProductsCollection = newProductsIDsToProcess.Union(changedProductIDs).ToList();

        ProductsToProcess = currentImages.Where(c => changedProductsCollection.Contains(c.Key)).SelectMany(c => c).ToList();

      }
    }

    public void ArchiveImages()
    {
      var lastProductExportPath = Path.Combine(_serializationPath, IMAGES_ARCHIVE_FILENAME);

      foreach (var productElement in _productImageElementsToRemoveOnArchive)
      {
        productElement.Remove();
      }

      _currentImagesDocument.Save(lastProductExportPath);
    }



    public void MarkProductAsNotProcessed(List<XElement> productsToRemove)
    {
      foreach (var item in productsToRemove)
      {
        _productImageElementsToRemoveOnArchive.Add(item);
      }
    }

    private XDocument LoadLastExportedImages(string serializationPath)
    {
      var lastProductExportPath = Path.Combine(serializationPath, IMAGES_ARCHIVE_FILENAME);

      if (File.Exists(lastProductExportPath))
      {
        return XDocument.Load(lastProductExportPath);
      }

      return null;
    }
  }
}
