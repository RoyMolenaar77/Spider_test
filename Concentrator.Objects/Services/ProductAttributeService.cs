using System;
using System.Collections.Generic;
using System.Linq;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Web;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Services.DTO;
using Concentrator.Objects.Logic;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Complex;
using Concentrator.Objects.DataAccess.UnitOfWork;

namespace Concentrator.Objects.Services
{
  public class ProductAttributeService : Service<ProductAttributeMetaData>, IFilterService
  {
    public override IQueryable<ProductAttributeMetaData> Search(string queryTerm)
    {
      queryTerm.IfNullOrEmpty("").ToLower();

      return Repository<ProductAttributeName>().GetAllAsQueryable(c => c.Name.Contains(queryTerm)).Select(c => c.ProductAttributeMetaData);
    }

    public List<AttributeGroup> getFiltersByProducts(List<ProductDto> products)
    {

      var repo = Scope.Repository<ProductAttributeMetaData>();
      var repo2 = Scope.Repository<ProductAttributeValue>();

      //get all the attributes
      //var attributes = repo.GetAll(a => a.IsSearchable == true);

      //to do: issearchable must be true
      var attributes = repo.GetAll();

      List<AttributeGroup> f = new List<AttributeGroup>(); // a list to store the attributes
      AttributeGroup attribute = null;

      foreach (var m in attributes)//get per attribute the attribute values and add it to the list
      {
        var attributeValues = repo2.GetAll(av => av.AttributeID == m.AttributeID).OrderBy(c => c.Value);

        List<string> tempValueList = new List<string>();

        foreach (var p in products)
        {
          foreach (var item in attributeValues)
          {
            if (item.ProductID == p.ProductID)
            {
              if (!string.IsNullOrEmpty(item.Value))
              {
                tempValueList.Add(item.Value); //als je een product hebt gevonden kun je stoppen in de attributeValues (je kan dan doorgaan met product nummer 2.
                break;
              }
            }
          }
        }
        tempValueList.Sort();

        if (tempValueList.Count == 0)
          continue;//dont save the attribute but go to new-one and check it that HAS attribute values
        else
        {
          //there are attribute values, so make an attribute and atributevalueList
          attribute = new AttributeGroup();
          attribute.AttributeName = m.AttributeCode;
          attribute.AttributeID = m.AttributeID;
          attribute.AttributeValueList = new List<AttributeValue>();

          int count = 1;//count and checkvalue is for counting the same attribute values
          AttributeValue PrecedingAttributeValue = null;
          for (int i = 0; i < tempValueList.Count; i++)
          {
            AttributeValue NewAttributeValue = new AttributeValue();//sla de value op in de lijst en het aantal
            NewAttributeValue.AttributeValueName = tempValueList[i];

            if (count == 1)//new attribute value
            {
              PrecedingAttributeValue = NewAttributeValue;
              count++;
              if (tempValueList.Count == 1)
                attribute.AttributeValueList.Add(PrecedingAttributeValue);
              else
                continue;
            }

            if ((string.Compare(PrecedingAttributeValue.AttributeValueName, NewAttributeValue.AttributeValueName, true) == 0))//preceding value is the same as new value
            {
              if (i == tempValueList.Count - 1) //if the last value is the same as preceding, then store preceding
              {
                PrecedingAttributeValue.NumberPerAttribute = count;
                attribute.AttributeValueList.Add(PrecedingAttributeValue);
              }
              else
              {
                count++;
                continue;
              }
            }
            else //last value is not new value, so store the last value
            {

              PrecedingAttributeValue.NumberPerAttribute = count - 1;

              attribute.AttributeValueList.Add(PrecedingAttributeValue); //save checkvalue (the last value)
              PrecedingAttributeValue = NewAttributeValue;
              count = 2;

              if (i == tempValueList.Count - 1)
              {
                PrecedingAttributeValue.NumberPerAttribute = count - 1;
                attribute.AttributeValueList.Add(PrecedingAttributeValue);
              }
            }
          }
          attribute.ProductsPerAttribute = tempValueList.Count;
          f.Add(attribute);
        }

      }

      //sort the list and return only the first ten attributes
      f.Sort(delegate(AttributeGroup g1, AttributeGroup g2) { return g1.ProductsPerAttribute.CompareTo(g2.ProductsPerAttribute); });
      f.Reverse();
      if (f.Count > 10)
        return f.GetRange(0, 10);
      else
        return f;
    }

    public List<ProductDto> GetProductsWithFilter(List<int> productListIDs)
    {
      List<ProductDto> ProductList = new List<ProductDto>();

      var repo1 = Scope.Repository<AssortmentContentView>();
      var repo2 = Scope.Repository<ProductDescription>();
      var connector = Repository<Connector>().GetSingle(c => c.ConnectorID == Client.User.ConnectorID);
      ContentLogic logic = new ContentLogic(Scope, Client.User.ConnectorID.Value);

      foreach (var item in productListIDs)
      {
        ProductDto dto = new ProductDto();
        var product1 = repo1.GetSingle(a => a.ProductID == item);
        var product2 = repo2.GetSingle(b => b.ProductID == item);

        dto.ProductID = product1.ProductID;
        dto.Name = product2 == null ? "n/a" : product2.ProductName;
        dto.Description = product1.ShortDescription;
        dto.Price = logic.CalculatePrice(item, 1, connector, Enumerations.PriceRuleType.UnitPrice);
        dto.Stock = product1.QuantityOnHand;
        dto.Expected = product1.QuantityToReceive.ToString();

        ProductList.Add(dto);
      }
      return ProductList;

    }

    public List<ProductDto> GetAllProductsByGroup()
    {

      var repo = Scope.Repository<AssortmentContentView>();
      List<ProductDto> ListOfAllRecords = new List<ProductDto>();

      var allProducts = repo.GetAll(c => c.ConnectorID == Client.User.ConnectorID.Value); //get all the products specific to the parameters
      if (allProducts == null)
      {
        return ListOfAllRecords;
      }

      foreach (var productDto in allProducts)
      {
        if (productDto == null)
          continue;
        ProductDto pcm = new ProductDto();
        pcm.ProductID = productDto.ProductID;
        pcm.Name = productDto.ProductName.ToString();
        pcm.Description = productDto.ShortDescription;
        pcm.Price = 12.00M;

        ListOfAllRecords.Add(pcm);
      }

      return ListOfAllRecords;
    }

    public ProductDetailDto getProductDetailsByID(int productID)
    {
      int connectorID = Client.User.ConnectorID.Value;
      int languageID = Client.User.LanguageID;
      ProductDetailDto dto = new ProductDetailDto();

      var productDescription = Repository<ProductDescription>().GetSingle(a => a.ProductID == productID && a.LanguageID == languageID);
      var assortment2 = Repository<AssortmentContentView>().GetAll(x => x.ProductID == productID);
      var assortment = assortment2.FirstOrDefault();


      dto.ProductID = productID;



      //
      //descriptions and productname, and modelname
      if (productDescription == null)
      {
        //check for default language
        var productDescription2 = Repository<ProductDescription>().GetSingle(a => a.ProductID == productID);

        if (productDescription2 != null)
        {
          dto.ShortDescription = productDescription2.ShortContentDescription;
          dto.LongDescription = productDescription2.LongContentDescription;
          dto.ProductName = productDescription2.ProductName;
        }
        else//lool in assortmentcontentview
        {
          var productDescription3 = Repository<AssortmentContentView>().GetSingle(w => w.ProductID == productID);
          if (productDescription3 != null)
          {
            dto.ShortDescription = productDescription3.ShortDescription;
            dto.LongDescription = productDescription3.LongDescription;
            dto.ProductName = productDescription3.ShortDescription;//productname doesn't excist in AssortmentView
            dto.ModelName = "";
          }

        }

      }
      else
      {
        //string longDes = productDescription.LongContentDescription;
        dto.ShortDescription = productDescription.ShortContentDescription != null ? productDescription.ShortContentDescription : productDescription.ShortSummaryDescription;
        dto.LongDescription = productDescription.LongContentDescription == null ? " " : productDescription.LongContentDescription.Replace("\\n", "<br />");
        dto.ModelName = productDescription.ModelName;
        dto.ProductName = productDescription.ProductName;
        dto.ModelName = productDescription.ModelName;

      }

      //
      //extra description
      var extr = Repository<VendorAssortment>().GetSingle(w => w.ProductID == productID);
      if (extr != null)
      {
        var longDes = (!string.IsNullOrEmpty(extr.LongDescription)) ? extr.LongDescription : " ";
        dto.ExtraDescription = (!string.IsNullOrEmpty(extr.ShortDescription)) ? extr.ShortDescription : longDes;
      }
      else
        dto.ExtraDescription = "";

      //
      //brandname
      if (assortment == null)
      {
        dto.BrandName = "";
        var p = Repository<Product>().GetSingle(c => c.ProductID == productID);
        if (p != null)
        {
          var b = Repository<Brand>().GetSingle(c => c.BrandID == p.BrandID);
          if (b != null)
            dto.BrandName = b.Name;
        }
        else
          dto.BrandName = "";


      }
      else
      {
        dto.BrandName = assortment.BrandName;
      }

      //productgroup
      //var tempProduct = Scope.Repository<ContentProductGroup>().GetSingle(t => t.ConnectorID == connectorID && t.ProductID == productID);
      //connectorid hoeft niet?
      var tempProduct = Scope.Repository<ContentProductGroup>().GetSingle(t => t.ProductID == productID);

      if (tempProduct == null)
        dto.ProductGroup = "";
      else
      {
        var tempProduct2 = Scope.Repository<ProductGroupMapping>().GetSingle(q => q.ProductGroupMappingID == tempProduct.ProductGroupMappingID);
        if (tempProduct2 == null)
          dto.ProductGroup = "";
        else
        {
          var productGroup = Scope.Repository<ProductGroupLanguage>().GetSingle(r => r.ProductGroupID == tempProduct2.ProductGroupID && r.LanguageID == languageID);
          var productGroup2 = Scope.Repository<ProductGroupLanguage>().GetSingle(r => r.ProductGroupID == tempProduct2.ProductGroupID).Name;

          if (productGroup == null) //supllied language doesnt excists
            dto.ProductGroup = productGroup2;
          else
            dto.ProductGroup = productGroup.Name;
        }
      }

      //barcode
      List<String> stringList = new List<String>();
      var barCodes = Repository<ProductBarcode>().GetAll(r => r.ProductID == productID);
      foreach (var item2 in barCodes)
        stringList.Add(item2.Barcode);
      dto.Barcode = string.Join(", ", stringList);

      //attributes
      dto.AttributeNameValueList = new List<AttributeNameAndValueDto>();
      //to do: language id is not set
      var attributesValues = Repository<ProductAttributeValue>().GetAll(d => d.ProductID == productID).ToList();

      foreach (var a in attributesValues)
      {
        var temp = Repository<ProductAttributeMetaData>().GetSingle(e => e.AttributeID == a.AttributeID && e.IsSearchable == true);
        if (temp != null)
        {
          AttributeNameAndValueDto av = new AttributeNameAndValueDto();
          //to do: language id is not set

          av.AttributeName = Repository<ProductAttributeName>().GetSingle(w => w.AttributeID == a.AttributeID && w.LanguageID == languageID).Name;
          av.AttributeValue = a.Value;
          av.AttributeValueID = a.AttributeValueID;
          av.ImagePath = a.ProductAttributeMetaData.AttributePath;
          dto.AttributeNameValueList.Add(av);

        }

      }
      return dto;
    }

    public List<AttributeValueGroupingResult> GetProductAttributeValueGrouping(int? connectorID, int languageID)
    {
      return ((IFunctionScope)Scope).Repository().GetAttributeValueGrouping(connectorID, languageID).ToList();
    }
  }
}

