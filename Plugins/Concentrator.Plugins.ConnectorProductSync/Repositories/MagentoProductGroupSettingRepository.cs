using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Magento;
using Concentrator.Plugins.ConnectorProductSync.Helpers;
using Concentrator.Plugins.ConnectorProductSync.Models;
using PetaPoco;

namespace Concentrator.Plugins.ConnectorProductSync.Repositories
{
  public class MagentoProductGroupSettingRepository : IMagentoProductGroupSettingRepository
  {
    private IDatabase petaPoco;
    private IGenerateUpdateProperties generateUpdateFields;

    public MagentoProductGroupSettingRepository(IDatabase petaPoco, IGenerateUpdateProperties generateUpdateFields)
    {
      this.petaPoco = petaPoco;
      this.generateUpdateFields = generateUpdateFields;
    }

    public MagentoProductGroupSetting GetMagentoSettingByID(int MagentoProductGroupSettingID)
    {
      MagentoProductGroupSetting magentoProductGroupSetting = petaPoco.SingleOrDefault<MagentoProductGroupSetting>(string.Format(@"
        SELECT *
        FROM MagentoProductGroupSetting
        WHERE MagentoProductGroupSettingID = {0}
      ", MagentoProductGroupSettingID));

      return magentoProductGroupSetting;
    }

    public MagentoProductGroupSetting GetMagentoSettingByMasterGroupMappingID(int MasterGroupMappingID)
    {
      MagentoProductGroupSetting magentoProductGroupSetting = petaPoco.SingleOrDefault<MagentoProductGroupSetting>(string.Format(@"
        SELECT *
        FROM MagentoProductGroupSetting
        WHERE MasterGroupMappingID = {0}
      ", MasterGroupMappingID));

      return magentoProductGroupSetting;
    }

    public List<MagentoProductGroupSetting> GetListOfMagentoSettingByConnector(int connectorID)
    {
      List<MagentoProductGroupSetting> magentoProductGroupSettings = petaPoco.Fetch<MagentoProductGroupSetting>(string.Format(@"
        SELECT ms.*
        FROM MagentoProductGroupSetting ms
        INNER JOIN ProductGroupMapping pgm ON ms.ProductGroupmappingID = pgm.ProductGroupMappingID
        WHERE pgm.ConnectorID = {0}
      ", connectorID));

      return magentoProductGroupSettings;
    }

    public void UpdateMagentoProductGroupSetting(MagentoProductGroupSetting currentMagentoSetting, MagentoProductGroupSetting compareMagentoSetting, List<string> ignoreProperties)
    {
      List<string> changes = generateUpdateFields.GetPropertiesForUpdate(compareMagentoSetting, currentMagentoSetting, ignoreProperties);

      if (changes.Count > 0)
      {
        var updateQuery = string.Join(",", changes);
        petaPoco.Update<MagentoProductGroupSetting>(string.Format(@"
          SET {1}
          WHERE MagentoProductGroupSettingID = {0}
        ", currentMagentoSetting.MagentoProductGroupSettingID, updateQuery));
      }
    }

    public int InsertMagentoSetting(MagentoProductGroupSetting magentoProductGroupSetting)
    {
      PetaPocoMagentoProductGroupSetting newMagentoSetting = new PetaPocoMagentoProductGroupSetting();

      newMagentoSetting.ProductGroupmappingID = magentoProductGroupSetting.ProductGroupmappingID.Value; //todo: remove this
      newMagentoSetting.ShowInMenu = magentoProductGroupSetting.ShowInMenu;
      newMagentoSetting.DisabledMenu = magentoProductGroupSetting.DisabledMenu;
      newMagentoSetting.IsAnchor = magentoProductGroupSetting.IsAnchor;
      newMagentoSetting.MasterGroupMappingID = magentoProductGroupSetting.MasterGroupMappingID;
      newMagentoSetting.CreatedBy = 1; // todo: need fix!

      var newMagentoProductGroupSettingID = petaPoco.Insert("MagentoProductGroupSetting", "MagentoProductGroupSettingID", true, newMagentoSetting);
      return int.Parse(newMagentoProductGroupSettingID.ToString());
    }
  }
}
