using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Web;
using System.Web.Script.Serialization;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Localization;
using System.Web.UI.WebControls;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Services.DTO;
using Concentrator.Objects.DataAccess.Repository;

namespace Concentrator.Objects.Services
{
  public class ProductGroupMappingService : Service<ProductGroupMapping>, IProductGroupMappingService
  {
    public override void Create(ProductGroupMapping model)
    {

      if (!(Repository<ProductGroupMapping>().GetSingle(c => c.ProductGroupID == model.ProductGroupID && model.ConnectorID == c.ConnectorID
          && (model.Score.HasValue ? model.Score.Value : 0) == (c.Score.HasValue ? c.Score.Value : c.ProductGroup.Score)
          && model.CustomProductGroupLabel != c.CustomProductGroupLabel) == null))
      {
        throw new InvalidOperationException("Two product group mappings with the same score and product group and different labels cannot be inserted. Change the score.");
      }

      base.Create(model);
    }

    public IQueryable<ContentProductGroupMapping> GetContentProductGroupMappings()
    {
      var contentProductGroups = Repository<ContentProductGroup>().GetAllAsQueryable().Take(30);

      List<ContentProductGroupMapping> mapping = new List<ContentProductGroupMapping>();

      contentProductGroups.ForEach((gr, index) =>
      {
        var lineage = gr.ProductGroupMapping.Lineage.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

        lineage.ForEach((line, idx) =>
        {
          mapping.Add(
              (from mapp in Repository().GetAll(c => c.ProductGroupMappingID.ToString() == line)
               let isDirect = gr.ProductGroupMappingID == mapp.ProductGroupMappingID
               select new ContentProductGroupMapping
               {
                 ProductGroupMappingID = mapp.ProductGroupMappingID,
                 ParentProductGroupMappingID = mapp.ParentProductGroupMappingID,
                 ConnectorID = isDirect ? mapp.ConnectorID : 0,
                 Score = mapp.Score,
                 ContentProductGroupID = isDirect ? gr.ContentProductGroupID : 0,
                 FilterByParentGroup = mapp.FilterByParentGroup,
                 FlattenHierarchy = mapp.FlattenHierarchy,
                 ProductGroupName =
                     mapp.ProductGroup.ProductGroupLanguages.FirstOrDefault(
                     l => l.LanguageID == Client.User.LanguageID).Name,
                 ProductGroupID = mapp.ProductGroupID,
                 ProductID = isDirect ? gr.ProductID : 0,
                 ProductName =
                     isDirect
                         ? gr.Product.ProductDescriptions.FirstOrDefault(
                               l => l.LanguageID == Client.User.LanguageID).Try(p => p.ProductName, string.Empty)

                         : string.Empty

               }).FirstOrDefault()
            );
        });
      });

      return mapping.AsQueryable();
    }

    public IQueryable<ProductGroupMapping> GetProductGroupPerParent(int parentID, string ids)
    {
      JavaScriptSerializer ser = new JavaScriptSerializer();
      var allIds = ser.Deserialize<int[]>(ids);
      string lineage = string.Empty;

      foreach (var w in allIds)
      {
        lineage += "/" + w.ToString();
      }

      lineage += '/';

      var mappings = (from rp in Repository<ProductGroupMapping>().GetAll()
                      where rp.Lineage.StartsWith(lineage)
                      select rp
                      ).ToList();

      mappings = mappings.Where(c => c.Lineage.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Count() == allIds.Count() + 1).ToList();

      return mappings.AsQueryable();
    }

    public void CopyProductGroupMapping(int sourceConnectorID, int destinationConnectorID)
    {
      Repository<ContentProductGroup>().Delete(x => x.ConnectorID == destinationConnectorID);
      Repository<ProductGroupMapping>().Delete(x => x.ConnectorID == destinationConnectorID);

      ((IFunctionScope)Scope).Repository().CopyProductGroupMappings(sourceConnectorID, destinationConnectorID);
    }

    public void GenerateBrandMapping(int connectorID, int Score)
    {
      var productGroupLanguage = Repository<ProductGroupLanguage>().GetSingle(x => x.Name == "Brandshop");

      if (productGroupLanguage == null)
      {
        throw new InvalidOperationException("Brandshop product group is missing");
      }

      var productGroupItem = (from p in Repository<ProductGroupMapping>().GetAll(p => p.ProductGroupID == productGroupLanguage.ProductGroupID)
                              select p).FirstOrDefault();


      if (productGroupItem == null)
      {
        productGroupItem = new ProductGroupMapping
        {
          ProductGroupID = productGroupLanguage.ProductGroupID,
          ConnectorID = connectorID,
          FlattenHierarchy = false,
          FilterByParentGroup = false,
          Score = Score
        };

        base.Create(productGroupItem);
      }

      var brandName = (from c in Repository<Concentrator.Objects.Models.Contents.Content>().GetAll()
                       select new
                       {
                         c.ProductID,
                         c.Product.Brand.Name
                       });

      var contentProductGroups = (from c in Repository<ContentProductGroup>().GetAll()
                                  select new
                                  {
                                    c.ProductID,
                                    c.ProductGroupMapping.ProductGroupID
                                  });

      var productgroupmappings = (from pgm in Repository<ProductGroupMapping>().GetAll(pgm => pgm.ConnectorID == connectorID)
                                  select pgm);

      var productGrouplanguages = (from pg in Repository<ProductGroupLanguage>().GetAll()
                                   select pg);

      var languages = (from l in Repository<Language>().GetAll()
                       select l);

      brandName.ForEach((model, idx) =>
      {
        var pgl = productGrouplanguages.FirstOrDefault(x => x.Name == model.Name);
        ProductGroup pg = null;
        if (pgl != null)
          pg = pgl.ProductGroup;

        if (pg == null)
        {
          pg = new ProductGroup() { Score = Score };

          Repository<ProductGroup>().Add(pg);



          foreach (var l in languages)
          {
            var newPgl = new ProductGroupLanguage()
            {
              ProductGroup = pg,
              Name = model.Name,
              LanguageID = l.LanguageID
            };

            Repository<ProductGroupLanguage>().Add(newPgl);
          }
        }
        else
          pg = pgl.ProductGroup;

        ProductGroupMapping brandProductGroupMapping = productgroupmappings.FirstOrDefault(x => x.ProductGroupID == pg.ProductGroupID && x.ParentProductGroupMappingID == productGroupItem.ProductGroupMappingID);

        if (brandProductGroupMapping == null)
        {
          brandProductGroupMapping = new ProductGroupMapping
          {
            ProductGroup = pg,
            ParentProductGroupMappingID = productGroupItem.ProductGroupMappingID,
            ConnectorID = connectorID,
            FlattenHierarchy = false,
            FilterByParentGroup = false
          };
          Repository<ProductGroupMapping>().Add(brandProductGroupMapping);
        }

        contentProductGroups.Where(x => x.ProductID == model.ProductID).ForEach((contentModel, index) =>
        {
          if (!productgroupmappings.Any(x => x.ProductGroupID == contentModel.ProductGroupID && x.ParentMapping == brandProductGroupMapping))
          {
            ProductGroupMapping productGroup = new ProductGroupMapping
            {
              ProductGroupID = contentModel.ProductGroupID,
              ParentMapping = brandProductGroupMapping,
              ConnectorID = connectorID,
              FlattenHierarchy = false,
              FilterByParentGroup = false
            };

            Repository<ProductGroupMapping>().Add(productGroup);
          }
        });
      });
    }

    public void CreateProductGroupAttributeMapping(int AttributeID, string value, int productGroupMappingID)
    {
      var list = (from i in Repository<ContentProductGroup>().GetAll(c => c.ProductGroupMappingID == productGroupMappingID) select i.Product);

      foreach (var p in list)
      {
        var attribute = p.ProductAttributeValues.FirstOrDefault(c => c.AttributeID == AttributeID);
        if (attribute == null)
        {
          attribute = new ProductAttributeValue
          {
            AttributeID = AttributeID,
            Value = value,
            ProductID = p.ProductID
          };
          Repository<ProductAttributeValue>().Add(attribute);
        }

      }
    }

    public void DeleteProductGroupMappingAttribute(int AttributeID, int productGroupMappingID)
    {
      foreach (var p in Repository<ContentProductGroup>().GetAll().Where(c => c.ProductGroupMappingID == productGroupMappingID).Select(c => c.Product))
      {
        var attribute = p.ProductAttributeValues.FirstOrDefault(c => c.AttributeID == AttributeID);

        Repository<ProductAttributeValue>().Delete(attribute);
      }
    }

    public void Publish(int sourceConnectorID, int destinationConnectorID, int? root = null, int? rootID = null, bool? copyAttributes = null, bool? copyPrices = null, bool? copyProducts = null, bool? copyContentVendorSettings = null, bool? copyPublications = null, bool? copyConnectorProductStatuses = null, bool? preferredContentSettings = null)
    {
      ((IFunctionScope)Scope).Repository().CopyProductGroupMappings(sourceConnectorID, destinationConnectorID, root, rootID, copyAttributes, copyPrices, copyProducts, copyContentVendorSettings, copyPublications, copyConnectorProductStatuses, preferredContentSettings);
    }

    private ProductGroupMappingDto GetProductGroupMappingLevelDto(ProductGroupMappingDto dto, ProductGroupMapping mapping, List<ProductGroupMapping> mappings, IRepository<ProductGroupMapping> repo)
    {
      dto.Name = mapping.ProductGroup.ProductGroupLanguages.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID).Name;
      dto.ProductGroupMappingID = mapping.ProductGroupMappingID;
      dto.HasChildren = repo.GetAllAsQueryable(c => c.ParentProductGroupMappingID == dto.ProductGroupMappingID).Count() > 0;
      dto.Lineage = mapping.Lineage;
      dto.Depth = mapping.Depth;
      var children = mappings.Where(c => c.ParentProductGroupMappingID.HasValue && c.ParentProductGroupMappingID == dto.ProductGroupMappingID).ToList();

      if (children.Count > 0)
      {
        dto.Children = new List<ProductGroupMappingDto>();
        children.ForEach((map, id) =>
        {

          dto.Children.Add(GetProductGroupMappingLevelDto(new ProductGroupMappingDto(), map, mappings, repo));
        });
      }
      return dto;
    }

    public List<ProductGroupMappingDto> Search(string query, bool languageSpecific = true, int levels = -1, int? connectorID = null)
    {

      if (!connectorID.HasValue) connectorID = Client.User.ConnectorID;

      List<ProductDescription> decs = new List<ProductDescription>();

      if (connectorID.HasValue)
        decs = Repository<ProductDescription>().GetAll(l => l.Product.ContentProducts.Any(m => m.ConnectorID == connectorID.Value)).ToList();
      else
        decs = Repository<ProductDescription>().GetAll().ToList();
      /**
       * 1) Search within all content products
       * 2) Build hierarchy
       * 3) Record the already loaded product group mapping levels
       * 4) Search within the rest of the product group mappings
       * 5) Build hierarchy from them
       */

      List<ProductGroupMappingDto> result = new List<ProductGroupMappingDto>();

      //Record the loaded product group mappings
      List<int> searchedProductGroupMappingIDs = new List<int>();



      //search within the  product hierarchy
      var matched = (from c in decs
                     where c.ProductName.Contains(query)
                     || c.ShortContentDescription.Contains(query)
                     || c.Product.VendorAssortments.Any(v => v.ShortDescription.Contains(query))
                     select c.Product).ToList();

      Dictionary<int, ProductGroupMappingDto> cacher = new Dictionary<int, ProductGroupMappingDto>();

      matched.ForEach(product =>
      {
        //loop the pgms and cache them
        product.ContentProductGroups.ForEach((content, idx) =>
        {

          var name = product.ProductDescriptions.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID).Description;


          ProductGroupMappingDto dto = null;
          cacher.TryGetValue(content.ProductGroupMappingID, out dto);

          if (dto == null) //not cached yet. Create and cache
          {
            ProductGroupMapping mapping = Repository().GetSingle(c => c.ProductGroupMappingID == content.ProductGroupMappingID);
            searchedProductGroupMappingIDs.Add(mapping.ProductGroupMappingID);

            dto = new ProductGroupMappingDto()
            {
              Name = mapping.ProductGroup.ProductGroupLanguages.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID).Name,
              ProductGroupMappingID = mapping.ProductGroupMappingID,
              Depth = mapping.Depth,
              Lineage = mapping.Lineage,
              Products = new List<ProductDto>(){new ProductDto(){
                Name = name,
                ProductID = product.ProductID
              }}
            };

            cacher.Add(content.ProductGroupMappingID, dto);
          }
          else
          {
            dto.Products.Add(new ProductDto() { Name = name, ProductID = product.ProductID });
          }
        });
      });

      cacher.ForEach((pair, idx) =>
      {
        var mapping = Repository().GetSingle(c => c.ProductGroupMappingID == pair.Value.ProductGroupMappingID);
        result.Add(BuildUp(pair.Value, mapping, c => searchedProductGroupMappingIDs.Add(c.ProductGroupMappingID)));
      });

      //start on mappings
      var mappingMatches = (from m in Repository().GetAllAsQueryable()
                            where m.ProductGroup.ProductGroupLanguages.Any(c => c.Name.Contains(query))
                            select m).Distinct().ToList();
      mappingMatches.RemoveAll(c => searchedProductGroupMappingIDs.Contains(c.ProductGroupMappingID));



      mappingMatches.ForEach(mapping =>
      {

        ProductGroupMappingDto dto = new ProductGroupMappingDto()
        {
          Name = mapping.ProductGroup.ProductGroupLanguages.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID).Name,
          ProductGroupMappingID = mapping.ProductGroupMappingID,
          Lineage = mapping.Lineage,
          Depth = mapping.Depth
        };

        result.Add(BuildUp(dto, mapping));
      });

      return result;
    }

    private ProductGroupMappingDto BuildUp(ProductGroupMappingDto dto, ProductGroupMapping mapping, Action<ProductGroupMappingDto> onLevel = null)
    {
      ProductGroupMappingDto parentDto = new ProductGroupMappingDto();

      parentDto.Name = mapping.ProductGroup.ProductGroupLanguages.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID).Name;
      parentDto.ProductGroupMappingID = mapping.ProductGroupMappingID;
      parentDto.HasChildren = true;
      parentDto.Children = new List<ProductGroupMappingDto>();
      parentDto.Lineage = mapping.Lineage;
      parentDto.Depth = mapping.Depth;

      //attach the child
      parentDto.Children.Add(dto);

      if (onLevel != null) onLevel(parentDto);

      if (mapping.ParentMapping != null) //has more parents
      {
        return BuildUp(parentDto, mapping.ParentMapping, onLevel);
      }

      return parentDto;
    }

    public List<ProductGroupMappingDto> GetByLineage(string lineage, int? connectorID = null, bool wholeTree = true)
    {
      if (!connectorID.HasValue) connectorID = Client.User.ConnectorID;
      var objects = Repository().GetAll(c => (!connectorID.HasValue || (connectorID.HasValue && connectorID.Value == c.ConnectorID)));

      if (!wholeTree)
      {

        var depth = lineage.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Count();

        var mapps = objects.Where(c => c.Lineage.StartsWith(lineage) && c.Depth == depth).ToList();

        List<ProductGroupMappingDto> dtos = new List<ProductGroupMappingDto>();
        mapps.ForEach((mapping, ix) =>
       {



         //testing
         var checkExcists2 = mapping.ProductGroup.ProductGroupLanguages.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID);
         string menuName2;
         if (checkExcists2 != null)
           menuName2 = checkExcists2.Name;
         else
           menuName2 = mapping.ProductGroup.ProductGroupLanguages.FirstOrDefault().Name;
         //
         //




         var dto = new ProductGroupMappingDto()
         {
           ProductGroupMappingID = mapping.ProductGroupMappingID,
           Depth = mapping.Depth,
           Lineage = mapping.Lineage,
           Name = menuName2
           //Name = mapping.ProductGroup.ProductGroupLanguages.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID).Name
         };
         dtos.Add(dto);
       });

        return dtos;
      }

      List<int> levels = new List<int>() { -1 };


      if (!string.IsNullOrEmpty(lineage))
      {
        var lvs = lineage.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

        levels.AddRange((from c in lvs select int.Parse(c)).ToList());
      }

      List<ProductGroupMappingDto> result = new List<ProductGroupMappingDto>();



      levels.ForEach((lv, idx) =>
      {
        int level = lv;

        int? parentID = null;

        if (idx > 0) parentID = levels.ElementAt(idx);

        var mapps = objects.Where(c => (parentID.HasValue ? parentID.Value == c.ParentProductGroupMappingID : c.ParentProductGroupMappingID == null)).ToList();

        mapps.ForEach((mapping, ix) =>
        {
          //testing
          var checkExcists = mapping.ProductGroup.ProductGroupLanguages.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID);
          string menuName;
          if (checkExcists != null)
            menuName = checkExcists.Name;
          else
            menuName = mapping.ProductGroup.ProductGroupLanguages.FirstOrDefault().Name;
          //
          //



          var dto = new ProductGroupMappingDto()
          {
            ProductGroupMappingID = mapping.ProductGroupMappingID,
            Depth = mapping.Depth,
            Lineage = mapping.Lineage,
            //Name = mapping.ProductGroup.ProductGroupLanguages.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID).Name
            Name = menuName
          };

          if (parentID == null)
          {
            result.Add(dto);
          }
          else
          {
            AttachChildByLineage(dto, parentID.Value, result, levels, levels[1]);
          }
        });
      });
      return result;
    }

    private void AttachChildByLineage(ProductGroupMappingDto dtoToAttach, int parentMappingID, List<ProductGroupMappingDto> collection, List<int> lineage, int currentLevel)
    {
      var parent = collection.Where(c => c.ProductGroupMappingID == parentMappingID).FirstOrDefault();

      if (parent == null)
      {
        AttachChildByLineage(dtoToAttach, parentMappingID, collection.FirstOrDefault(c => c.ProductGroupMappingID == currentLevel).Children, lineage, lineage[lineage.IndexOf(currentLevel) + 1]);
      }
      else
      {
        if (parent.Children == null) parent.Children = new List<ProductGroupMappingDto>();

        parent.Children.Add(dtoToAttach);
      }

    }

    public void Delete(int id)
    {
      var mapp = Repository().GetSingle(c => c.ProductGroupMappingID == id);

      this.Delete(mapp, id);

      Repository().Delete(mapp);
    }

    private void Delete(ProductGroupMapping pgm, int id)
    {
      foreach (var child in pgm.ChildMappings)
      {
        if (child.ProductGroupMappingID == id) return;

        if (child.ChildMappings.Count < 1)
          Repository().Delete(pgm);
        else
          Delete(child, id);
      }
    }

    public void DeleteWholeConnectorMapping(int connectorID)
    {
      var list = Repository().GetAll(x => x.ConnectorID == connectorID);

      list.ForEach((x, idx) =>
      {
        Repository().Delete(x);
      });
    }
  }
}