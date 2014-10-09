using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.DataAccess.UnitOfWork;


namespace Concentrator.Plugins.RelatedProducts
{
  public class AttributeRule
  {
    public ProductAttributeMetaData Attribute { get; set; }
    public bool Equals { get; set; }
  }

  public class RelatedProductFiller : ConcentratorPlugin
  {
    struct AttributeIDAndValue
    {
      public int AttributeID;
      public string AttributeValue;
    }

    public int vendorID;
    public int relatedProductTypeID;


    public override string Name
    {
      get { return "Concentrator Related Products Maker Plugin"; }
    }

    protected override void Process()
    {
      var config = GetConfiguration();

      using (var unit = GetUnitOfWork())
      {
        vendorID = unit.Scope.Repository<Vendor>().GetSingle(x => x.Name == "PFA CC").VendorID;

        //complete the look:
        var AttributeRulesCompleteTheLook = new List<AttributeRule>();
        AttributeRulesCompleteTheLook.Add(new AttributeRule() { Attribute = unit.Scope.Repository<ProductAttributeMetaData>().GetSingle(c => c.AttributeCode == "Targetgroup"), Equals = true });
        AttributeRulesCompleteTheLook.Add(new AttributeRule() { Attribute = unit.Scope.Repository<ProductAttributeMetaData>().GetSingle(c => c.AttributeCode == "Productgroup"), Equals = false });
        AttributeRulesCompleteTheLook.Add(new AttributeRule() { Attribute = unit.Scope.Repository<ProductAttributeMetaData>().GetSingle(c => c.AttributeCode == "Inputcode"), Equals = true });
        AttributeRulesCompleteTheLook.Add(new AttributeRule() { Attribute = unit.Scope.Repository<ProductAttributeMetaData>().GetSingle(c => c.AttributeCode == "Season"), Equals = true });

        RelateProducts(AttributeRulesCompleteTheLook, vendorID, unit, unit.Scope.Repository<RelatedProductType>().GetSingle(c => c.Type == "Related"));
        //end complete the look

      }
    }
     
    private Dictionary<AttributeIDAndValue, List<int>> DictionaryFiller(List<AttributeRule> attributeRules, IUnitOfWork unit)
    {
      Dictionary<AttributeIDAndValue, List<int>> attributeDictionary = new Dictionary<AttributeIDAndValue, List<int>>();

      //get all products that have attribute values from parameter attributeRules
      List<int> attIDs = new List<int>();
      foreach (var att in attributeRules)
      {
        attIDs.Add(att.Attribute.AttributeID);
      }
      var allAttributesForProducts = unit.Scope.Repository<ProductAttributeValue>().GetAll(c => attIDs.Contains(c.AttributeID));

      //group the attributes per attribute
      var groupedPerAttribute = from a in allAttributesForProducts
                                group a by a.AttributeID;

      foreach (var groupedAttribute in groupedPerAttribute)
      {
        var groupedAttributeValues = from g in groupedAttribute
                                     group g by g.Value;

        foreach (var groupedAttributeValue in groupedAttributeValues)
        {
          AttributeIDAndValue attPlusValue;
          attPlusValue.AttributeID = groupedAttributeValue.FirstOrDefault().AttributeID;
          attPlusValue.AttributeValue = groupedAttributeValue.FirstOrDefault().Value;

          List<int> productList = new List<int>();
          foreach (var nogroup in groupedAttributeValue)
          {
            productList.Add(nogroup.ProductID);
            var j = nogroup;
          }
          attributeDictionary.Add(attPlusValue, productList);
        }
      }
      return attributeDictionary;
    }

    private void RelateProducts(List<AttributeRule> attributeRules, int vendorID, IUnitOfWork unit, RelatedProductType relatedProductType, bool isConfigured = false)
    {
      Dictionary<AttributeIDAndValue, List<int>> attributeDictionary = DictionaryFiller(attributeRules, GetUnitOfWork());

      int allCount = attributeRules.Count; //4 rules

      //get only the products that have the attributes in attributeRules
      List<int> attributeIDList = new List<int>();
      foreach (var att in attributeRules)
      {
        attributeIDList.Add(att.Attribute.AttributeID);
      }
      List<int> filteredProductIDs = unit.Scope.Repository<ProductAttributeValue>().GetAll(c => attributeIDList.Contains(c.AttributeID)).Select(c => c.ProductID).Distinct().ToList();

      foreach (var product in unit.Scope.Repository<VendorAssortment>().GetAll(c => c.VendorID == vendorID && filteredProductIDs.Contains(c.ProductID)))
      {
        List<int> RelatedProductIDs = new List<int>();

        foreach (var attributeRule in attributeRules)
        {
          int currentAttRuleAttID = attributeRule.Attribute.AttributeID;

          var AttributeOfProduct = product.Product.ProductAttributeValues.FirstOrDefault
              (c => c.AttributeID == currentAttRuleAttID);

          if (AttributeOfProduct == null)
            continue;

          if (attributeRule.Equals)
          {
            if (AttributeOfProduct != null)//huidige product heeft een attribuutwaarde voor targetgroup
            {
              //kijk nu in de dictionary welke producten ook een attributewaade = men hebben
              AttributeIDAndValue attIDAndValue;
              attIDAndValue.AttributeID = currentAttRuleAttID;
              attIDAndValue.AttributeValue = AttributeOfProduct.Value;

              if (attributeDictionary.ContainsKey(attIDAndValue))
              {
                RelatedProductIDs.AddRange(attributeDictionary[attIDAndValue].ToList());
              }
              else
              {
                continue; //go to outer loop
              }
            }
            else
              continue; //go to next product?
          }
          else
          {
            if (AttributeOfProduct != null)
            {
              //kijk nu in de dictionary die geen waarde men tops hebben

              //hoeveel verschillende waardes heeft deze attribute?
              var differentAttValues = unit.Scope.Repository<ProductAttributeValue>()
                .GetAll(c => c.AttributeID == currentAttRuleAttID).Select(c => c.Value).Distinct(); //top, underwear, jeans

              AttributeIDAndValue AttIDAndValue;
              AttIDAndValue.AttributeID = currentAttRuleAttID;//49
              AttIDAndValue.AttributeValue = AttributeOfProduct.Value; //top

              AttributeIDAndValue AttIDAndValueToCompare;
              AttIDAndValueToCompare.AttributeID = currentAttRuleAttID;//49

              foreach (var differentAttValue in differentAttValues)
              {
                AttIDAndValueToCompare.AttributeValue = differentAttValue; //top

                if (AttIDAndValueToCompare.AttributeValue != AttIDAndValue.AttributeValue)//als ze verschillend zijn, sla ze op
                {
                  if (attributeDictionary.ContainsKey(AttIDAndValueToCompare))
                  {
                    RelatedProductIDs.AddRange(attributeDictionary[AttIDAndValueToCompare].ToList());
                  }
                }
                else
                  continue;
              }

            }
            else
              continue; //go to next product?
          }
        }

        //remove current id of list
        RelatedProductIDs.RemoveAll(c => c == product.ProductID);

        RelatedProductIDs = RelatedProductIDs.FindAll(delegate(int i)
        {
          return RelatedProductIDs.FindAll(delegate(int j)
          {
            return j == i;
          }).Count() > allCount - 1;
        }).Distinct().ToList();

        foreach (var prod in RelatedProductIDs)
        {
          var exist = unit.Scope.Repository<RelatedProduct>().GetSingle(c => c.ProductID == product.ProductID && c.RelatedProductID == prod);

          if (exist != null)
            continue;

          unit.Scope.Repository<RelatedProduct>().Add(new RelatedProduct()
          {
            ProductID = product.ProductID,
            RelatedProductID = prod,
            CreationTime = DateTime.Now,
            CreatedBy = 4,
            VendorID = vendorID,
            RelatedProductTypeID = relatedProductType.RelatedProductTypeID,
            IsConfigured = false
          });
        }

      }
      unit.Save();

    }
  }
}
