using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Magento;

namespace Concentrator.Plugins.ConnectorProductSync.Repositories
{
  public interface IMagentoProductGroupSettingRepository
  {
    MagentoProductGroupSetting GetMagentoSettingByID(int MagentoProductGroupSettingID);
    MagentoProductGroupSetting GetMagentoSettingByMasterGroupMappingID(int MasterGroupMappingID);

    List<MagentoProductGroupSetting> GetListOfMagentoSettingByConnector(int connectorID);

    int InsertMagentoSetting(MagentoProductGroupSetting magentoProductGroupSetting);
    void UpdateMagentoProductGroupSetting(MagentoProductGroupSetting currentMagentoSetting, MagentoProductGroupSetting compareMagentoSetting, List<string> ignoreProperties);
  }
}
