using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Concentrator.Web.CustomerSpecific.Coolcat.Models;

namespace Concentrator.Web.CustomerSpecific.Coolcat.Repositories
{
  public class ProductGroupRelatedProductGroupsRepository
  {
    private readonly XDocument _doc;

    public ProductGroupRelatedProductGroupsRepository(XDocument document)
    {
      _doc = document;
    }


    public void Add(ProductGroupRelatedProductGroupsModel model)
    {
      _doc.Root.Add(new XElement("Relation",
          new XElement("ProductGroup", model.ProductGroup),
          new XElement("RelatedProductGroups", model.RelatedProductGroups)
         ));
    }

    public List<ProductGroupRelatedProductGroupsModel> GetAllRules()
    {
      var models = (from r in _doc.Root.Elements("Relation")
                     select new ProductGroupRelatedProductGroupsModel()
                       {
                         ProductGroup = r.Element("ProductGroup").Value,
                         RelatedProductGroups = r.Element("RelatedProductGroups").Value
                       }).ToList();


      return models;
    }

    public void Update(ProductGroupRelatedProductGroupsModel model)
    {
      var xmlEntry = _doc.Root.Elements("Relation").Where(c => c.Element("ProductGroup").Value == model.ProductGroup).FirstOrDefault();
      xmlEntry.Element("RelatedProductGroups").Value = model.RelatedProductGroups;
    }

    public void Delete(ProductGroupRelatedProductGroupsModel model)
    {
      var xmlEntry = _doc.Root.Elements("Relation").Where(c => c.Element("ProductGroup").Value == model.ProductGroup).FirstOrDefault();
      xmlEntry.Remove();
    }
  }
}
