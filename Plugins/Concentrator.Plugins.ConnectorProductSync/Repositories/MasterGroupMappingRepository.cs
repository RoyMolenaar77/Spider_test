using Concentrator.Objects.Models.MastergroupMapping;
using Concentrator.Plugins.ConnectorProductSync.Helpers;
using Concentrator.Plugins.ConnectorProductSync.Models;
using PetaPoco;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.ConnectorProductSync.Repositories
{
  // todo: UpdateMasterGroupMapping verbeteren

  public class MasterGroupMappingRepository : IMasterGroupMappingRepository
  {
    private IDatabase petaPoco;
    private IGenerateUpdateProperties generateUpdateFields;

    public MasterGroupMappingRepository(IDatabase petaPoco, IGenerateUpdateProperties generateUpdateFields)
    {
      this.petaPoco = petaPoco;
      this.generateUpdateFields = generateUpdateFields;
    }

    public List<MasterGroupMapping> GetListOfAllMasterGroupMappings()
    {
      List<MasterGroupMapping> masterGroupMappings = petaPoco.Fetch<MasterGroupMapping>(@"
        SELECT *
        FROM MasterGroupMapping
        WHERE ConnectorID IS NULL
      ");
      return masterGroupMappings;
    }

    public List<MasterGroupMapping> GetListOfProductGroupsByConnector(int connectorID)
    {
      List<MasterGroupMapping> productGroups = petaPoco.Fetch<MasterGroupMapping>(string.Format(@"
        SELECT * 
        FROM dbo.MasterGroupMapping
        WHERE ConnectorID = {0}
      ", connectorID));

      return productGroups;
    }

    public List<MasterGroupMapping> GetListOfMasterGroupMappingChildren(int masterGroupMappingID)
    {
      List<MasterGroupMapping> masterGroupMappings = petaPoco.Query<MasterGroupMapping>(string.Format(@";
          WITH   allMGM
                AS ( SELECT   parentMGM.MasterGroupMappingID
                      FROM     dbo.MasterGroupMapping parentMGM
                      WHERE    MasterGroupMappingID = {0}
                      UNION ALL
                      SELECT   childMGM.MasterGroupMappingID
                      FROM     dbo.MasterGroupMapping childMGM
                              INNER JOIN allMGM ON childMGM.ParentMasterGroupMappingID = allMGM.MasterGroupMappingID
                    )
          SELECT  *
          FROM    dbo.MasterGroupMapping m
                  INNER JOIN allMGM ON m.MasterGroupMappingID = allMGM.MasterGroupMappingID
                  WHERE m.MasterGroupMappingID != {0}
      ", masterGroupMappingID))
       .ToList()
       ;

      return masterGroupMappings;
    }

    public List<MasterGroupMapping> GetListOfMasterGroupMappingsByProductGroupID(int productGroupID)
    {
      List<MasterGroupMapping> masterGroupMappings = petaPoco.Fetch<MasterGroupMapping>(string.Format(@"
        SELECT *
        FROM MasterGroupMapping
        WHERE ConnectorID IS NULL
	        AND ProductGroupID = {0}
      ", productGroupID));

      return masterGroupMappings;
    }

    public List<MasterGroupMappingProduct> GetListOfMappedProductsByMasterGroupMapping(int masterGroupMappingID)
    {
      List<MasterGroupMappingProduct> products = petaPoco.Fetch<MasterGroupMappingProduct>(string.Format(@"
        SELECT  *
        FROM    dbo.MasterGroupMappingProduct
        WHERE   MasterGroupMappingID = {0} AND IsProductMapped = 1
      ", masterGroupMappingID));

      return products;
    }

    public List<MasterGroupMappingLanguage> GetListOfMasterGroupMappingLanguagesByMasterGroupMappingID(int masterGroupMappingID)
    {
      List<MasterGroupMappingLanguage> masterGroupMappingLanguages = petaPoco.Fetch<MasterGroupMappingLanguage>(string.Format(@"
        SELECT *
        FROM MasterGroupMappingLanguage
        WHERE MasterGroupMappingID = {0}
      ", masterGroupMappingID));

      return masterGroupMappingLanguages;
    }

    public MasterGroupMapping GetMasterGroupMappingByMasterGroupMappingID(int masterGroupMappingID)
    {
      MasterGroupMapping masterGroupMapping = petaPoco.SingleOrDefault<MasterGroupMapping>(string.Format(@"
        SELECT  *
        FROM    dbo.MasterGroupMapping
        WHERE   MasterGroupMappingID = {0}
      ", masterGroupMappingID));

      return masterGroupMapping;
    }

    public MasterGroupMappingProduct GetMasterGroupMappingProductByID(int masterGroupMappingID, int productID)
    {
      MasterGroupMappingProduct masterGroupMappingProduct = petaPoco.SingleOrDefault<MasterGroupMappingProduct>(string.Format(@"
        SELECT *
        FROM MasterGroupMappingProduct
        WHERE MasterGroupMappingID = {0}
	        AND ProductID = {1}
      ", masterGroupMappingID, productID));

      return masterGroupMappingProduct;
    }

    public bool IsMasterGroupMappingExists(int masterGroupMappingID)
    {
      MasterGroupMapping masterGroupMapping = petaPoco.SingleOrDefault<MasterGroupMapping>(string.Format(@"
        SELECT  *
        FROM    dbo.MasterGroupMapping
        WHERE   MasterGroupMappingID = {0}
      ", masterGroupMappingID));

      return masterGroupMapping != null;
    }

    public int InsertMasterGroupMapping(MasterGroupMapping newMasterGroupMapping)
    {
      PetaPocoMasterGroupMappingModel newPetaPocoMasterGroupMapping = new PetaPocoMasterGroupMappingModel();

      newPetaPocoMasterGroupMapping.ProductGroupID = newMasterGroupMapping.ProductGroupID;
      if (newMasterGroupMapping.ParentMasterGroupMappingID.HasValue && newMasterGroupMapping.ParentMasterGroupMappingID.Value > 0)
      {
        newPetaPocoMasterGroupMapping.ParentMasterGroupMappingID = newMasterGroupMapping.ParentMasterGroupMappingID;
      }

      if (newMasterGroupMapping.ConnectorID.HasValue && newMasterGroupMapping.ConnectorID.Value > 0)
      {
        newPetaPocoMasterGroupMapping.ConnectorID = newMasterGroupMapping.ConnectorID;
      }

      if (newMasterGroupMapping.SourceMasterGroupMappingID.HasValue && newMasterGroupMapping.SourceMasterGroupMappingID.Value > 0)
      {
        newPetaPocoMasterGroupMapping.SourceMasterGroupMappingID = newMasterGroupMapping.SourceMasterGroupMappingID;
      }

      if (newMasterGroupMapping.SourceProductGroupMappingID.HasValue && newMasterGroupMapping.SourceProductGroupMappingID.Value > 0)
      {
        newPetaPocoMasterGroupMapping.SourceProductGroupMappingID = newMasterGroupMapping.SourceProductGroupMappingID;
      }

      if (newMasterGroupMapping.ExportID.HasValue && newMasterGroupMapping.ExportID.Value > 0)
      {
        newPetaPocoMasterGroupMapping.ExportID = newMasterGroupMapping.ExportID;
      }

      newPetaPocoMasterGroupMapping.Score = (newMasterGroupMapping.Score.HasValue && newMasterGroupMapping.Score.Value > 0) ? newMasterGroupMapping.Score : 0;
      newPetaPocoMasterGroupMapping.FlattenHierarchy = newMasterGroupMapping.FlattenHierarchy;
      newPetaPocoMasterGroupMapping.FilterByParentGroup = newMasterGroupMapping.FilterByParentGroup;

      var masterGroupMappingID = petaPoco.Insert("MasterGroupMapping", "MasterGroupMappingID", true, newPetaPocoMasterGroupMapping);
      return int.Parse(masterGroupMappingID.ToString());
    }

    public void InsertMasterGroupMappingLanguage(MasterGroupMappingLanguage newMasterGroupMappingLanguage)
    {
      petaPoco.Insert("MasterGroupMappingLanguage", "MasterGroupMappingID, LanguageID", false, new
      {
        MasterGroupMappingID = newMasterGroupMappingLanguage.MasterGroupMappingID,
        LanguageID = newMasterGroupMappingLanguage.LanguageID,
        Name = newMasterGroupMappingLanguage.Name
      });
    }

    public void InsertMasterGroupMappingProduct(int masterGroupMappingID, int productID)
    {
      petaPoco.Insert("MasterGroupMappingProduct", "MasterGroupMappingID, ProductID", false,
        new
        {
          MasterGroupMappingID = masterGroupMappingID,
          ProductID = productID,
          IsApproved = false,
          IsProductMapped = true
        });
    }

    public void InsertMasterGroupMappingProduct(int masterGroupMappingID, MasterGroupMappingProduct masterGroupMappingProduct)
    {
      petaPoco.Insert("MasterGroupMappingProduct", "MasterGroupMappingID, ProductID", false,
        new
        {
          MasterGroupMappingID = masterGroupMappingID,
          ProductID = masterGroupMappingProduct.ProductID,
          IsApproved = masterGroupMappingProduct.IsApproved,
          IsProductMapped = masterGroupMappingProduct.IsProductMapped,
          ConnectorPublicationRuleID = masterGroupMappingProduct.ConnectorPublicationRuleID
        });
    }

    public void UpdateMasterGroupMappingIDOfMasterGroupMappingProduct(MasterGroupMappingProduct masterGroupMappingProduct, int masterGroupMappingID)
    {
      petaPoco.Update<MasterGroupMappingProduct>(string.Format(@"
        SET MasterGroupMappingID = {0}
        WHERE MasterGroupMappingID = {1} AND ProductID = {2}
      ", masterGroupMappingID, masterGroupMappingProduct.MasterGroupMappingID, masterGroupMappingProduct.ProductID));
    }

    public void UpdateMasterGroupMapping(MasterGroupMapping masterGroupMapping)
    {
      MasterGroupMapping currentMasterGroupMapping = GetMasterGroupMappingByMasterGroupMappingID(masterGroupMapping.MasterGroupMappingID);

      StringBuilder changes = new StringBuilder();

      List<string> listOfChanges = generateUpdateFields.GetPropertiesForUpdate(masterGroupMapping, currentMasterGroupMapping);

      if (masterGroupMapping.ParentMasterGroupMappingID.HasValue && masterGroupMapping.ParentMasterGroupMappingID.Value > 0)
      {
        if (masterGroupMapping.ParentMasterGroupMappingID != currentMasterGroupMapping.ParentMasterGroupMappingID)
        {
          changes.Append(string.Format("ParentMasterGroupMappingID = {0}", masterGroupMapping.ParentMasterGroupMappingID));
        }
      }

      if (masterGroupMapping.ConnectorID.HasValue && masterGroupMapping.ConnectorID.Value > 0)
      {
        if (masterGroupMapping.ConnectorID != currentMasterGroupMapping.ConnectorID)
        {
          if (!string.IsNullOrEmpty(changes.ToString()))
          {
            changes.Append(",");
          }
          changes.Append(string.Format("ConnectorID = {0}", masterGroupMapping.ConnectorID));
        }
      }

      if (masterGroupMapping.ProductGroupID != currentMasterGroupMapping.ProductGroupID)
      {
        if (!string.IsNullOrEmpty(changes.ToString()))
        {
          changes.Append(",");
        }
        changes.Append(string.Format("ProductGroupID = {0}", masterGroupMapping.ProductGroupID));
      }

      if (masterGroupMapping.SourceMasterGroupMappingID.HasValue && masterGroupMapping.SourceMasterGroupMappingID.Value > 0)
      {
        if (masterGroupMapping.SourceMasterGroupMappingID != currentMasterGroupMapping.SourceMasterGroupMappingID)
        {
          if (!string.IsNullOrEmpty(changes.ToString()))
          {
            changes.Append(",");
          }
          changes.Append(string.Format("SourceMasterGroupMappingID = {0}", masterGroupMapping.SourceMasterGroupMappingID));
        }
      }

      if (masterGroupMapping.Score.HasValue)
      {
        if (masterGroupMapping.Score != currentMasterGroupMapping.Score)
        {
          if (!string.IsNullOrEmpty(changes.ToString()))
          {
            changes.Append(",");
          }
          changes.Append(string.Format("Score = {0}", masterGroupMapping.Score));
        }
      }

      if (masterGroupMapping.FlattenHierarchy != currentMasterGroupMapping.FlattenHierarchy)
      {
        if (!string.IsNullOrEmpty(changes.ToString()))
        {
          changes.Append(",");
        }
        changes.Append(string.Format("FlattenHierarchy = {0}", (masterGroupMapping.FlattenHierarchy ? 1 : 0)));
      }

      if (masterGroupMapping.FilterByParentGroup != currentMasterGroupMapping.FilterByParentGroup)
      {
        if (!string.IsNullOrEmpty(changes.ToString()))
        {
          changes.Append(",");
        }
        changes.Append(string.Format("FilterByParentGroup = {0}", (masterGroupMapping.FilterByParentGroup ? 1 : 0)));
      }

      if (!string.IsNullOrEmpty(changes.ToString()))
      {
        petaPoco.Update<MasterGroupMapping>(string.Format(@"
          SET {1}
          WHERE MasterGroupMappingID = {0}
        ", masterGroupMapping.MasterGroupMappingID, changes));
      }
    }

    public void UpdateMasterGroupMapping(MasterGroupMapping currentMasterGroupMapping, MasterGroupMapping compareMasterGroupMapping, List<string> ignoreProperties)
    {
      List<string> changes = generateUpdateFields.GetPropertiesForUpdate(compareMasterGroupMapping, currentMasterGroupMapping, ignoreProperties);

      if (changes.Count > 0)
      {
        var updateQuery = string.Join(",", changes);
        petaPoco.Update<MasterGroupMapping>(string.Format(@"
          SET {1}
          WHERE MasterGroupMappingID = {0}
        ", currentMasterGroupMapping.MasterGroupMappingID, updateQuery));
      }
    }

    public void DeleteMasterGroupMappingProduct(MasterGroupMappingProduct masterGroupMappingProduct)
    {
      petaPoco.Delete("MasterGroupMappingProduct", "MasterGroupMappingID, ProductID", masterGroupMappingProduct);
    }

    public void DeleteMasterGroupMapping(MasterGroupMapping masterGroupMapping)
    {
      //DeleteMasterGroupMappingLanguage(masterGroupMapping);
      petaPoco.Delete("ContentProductGroup", "MasterGroupMappingID", masterGroupMapping.MasterGroupMappingID);
      petaPoco.Delete("MasterGroupMapping", "MasterGroupMappingID", masterGroupMapping);
    }

    public void DeleteProductsByMasterGroupMappingID(int masterGroupMappingID)
    {
      petaPoco.Execute(string.Format(@"
        DELETE
        FROM MasterGroupMappingProduct
        WHERE MasterGroupMappingID = {0}
      ", masterGroupMappingID));
    }

    public void DeleteHierarchy(MasterGroupMapping masterGroupMapping)
    {
      // Delete ContentProductGroup
      petaPoco.Execute(string.Format(@";
        WITH MasterGroupMappingChilds
        AS (
	        SELECT m.MasterGroupMappingID
	        FROM MasterGroupMapping m
	        WHERE m.ParentMasterGroupMappingID = {0}
	
	        UNION ALL
	
	        SELECT m.MasterGroupMappingID
	        FROM MasterGroupMapping m
	        INNER JOIN MasterGroupMappingChilds mc ON m.ParentMasterGroupMappingID = mc.MasterGroupMappingID
	        )
        DELETE cpg
        FROM MasterGroupMappingChilds mc
        INNER JOIN ContentProductGroup cpg ON mc.MasterGroupMappingID = cpg.MasterGroupMappingID      
      ", masterGroupMapping.MasterGroupMappingID));

      // Delete Child MasterGroupMappings
      petaPoco.Execute(string.Format(@";
        WITH MasterGroupMappingChilds
        AS (
	        SELECT m.MasterGroupMappingID
	        FROM MasterGroupMapping m
	        WHERE m.ParentMasterGroupMappingID = {0}
	
	        UNION ALL
	
	        SELECT m.MasterGroupMappingID
	        FROM MasterGroupMapping m
	        INNER JOIN MasterGroupMappingChilds mc ON m.ParentMasterGroupMappingID = mc.MasterGroupMappingID
	        )
        DELETE m
        FROM MasterGroupMapping m
        INNER JOIN MasterGroupMappingChilds mc ON m.MasterGroupMappingID = mc.MasterGroupMappingID
      ", masterGroupMapping.MasterGroupMappingID));

      //Delete MasterGroupMapping
      DeleteMasterGroupMapping(masterGroupMapping);
    }

    public void DeleteMasterGroupMappingLanguage(MasterGroupMapping masterGroupMapping)
    {
      petaPoco.Delete("MasterGroupMappingLanguage", "MasterGroupMappingID", masterGroupMapping);
    }

    public void MapVendorProductGroupToMasterGroupMapping(int vendorProductGroupID, int masterGroupMappingID)
    {
      petaPoco.Insert("MasterGroupMappingProductGroupVendor", "MasterGroupMappingID, ProductGroupVendorID", false, new
      {
        MasterGroupMappingID = masterGroupMappingID,
        ProductGroupVendorID = vendorProductGroupID
      });
    }

    public void ImportProductGroupMappingPerConnector(int connectorID)
    {
      petaPoco.Execute(string.Format(@"
        ;EXEC CopyProductGroupMappingToConnectorMappingPerConnector
		        {0}
      ", connectorID));
    }

    public void UpdateMasterGroupMappingProduct(MasterGroupMappingProduct masterGroupMappingProduct)
    {
      MasterGroupMappingProduct currentMasterGroupMappingProduct = GetMasterGroupMappingProductByID(masterGroupMappingProduct.MasterGroupMappingID, masterGroupMappingProduct.ProductID);

      StringBuilder changes = new StringBuilder();

      List<string> listOfChanges = generateUpdateFields.GetPropertiesForUpdate(masterGroupMappingProduct, currentMasterGroupMappingProduct);

      if (masterGroupMappingProduct.IsApproved != currentMasterGroupMappingProduct.IsApproved)
      {
        changes.Append(string.Format("IsApproved = {0}", (masterGroupMappingProduct.IsApproved ? 1 : 0)));
      }

      if (masterGroupMappingProduct.IsCustom != currentMasterGroupMappingProduct.IsCustom)
      {
        if (!string.IsNullOrEmpty(changes.ToString()))
        {
          changes.Append(",");
        }
        changes.Append(string.Format("IsCustom = {0}", (masterGroupMappingProduct.IsCustom ? 1 : 0)));
      }

      if (masterGroupMappingProduct.IsProductMapped != currentMasterGroupMappingProduct.IsProductMapped)
      {
        if (!string.IsNullOrEmpty(changes.ToString()))
        {
          changes.Append(",");
        }
        changes.Append(string.Format("IsProductMapped = {0}", (masterGroupMappingProduct.IsProductMapped ? 1 : 0)));
      }

      if (masterGroupMappingProduct.ConnectorPublicationRuleID.HasValue && masterGroupMappingProduct.ConnectorPublicationRuleID.Value > 0)
      {
        if ((!currentMasterGroupMappingProduct.ConnectorPublicationRuleID.HasValue) || masterGroupMappingProduct.ConnectorPublicationRuleID.Value != currentMasterGroupMappingProduct.ConnectorPublicationRuleID.Value)
        {
          if (!string.IsNullOrEmpty(changes.ToString()))
          {
            changes.Append(",");
          }
          changes.Append(string.Format("ConnectorPublicationRuleID = {0}", masterGroupMappingProduct.ConnectorPublicationRuleID.Value));
        }
      }

      if (!string.IsNullOrEmpty(changes.ToString()))
      {
        petaPoco.Update<MasterGroupMappingProduct>(string.Format(@"
          SET {2}
          WHERE MasterGroupMappingID = {0}
	          AND ProductID = {1}
        ", masterGroupMappingProduct.MasterGroupMappingID, masterGroupMappingProduct.ProductID, changes));
      }
    }
  }
}
