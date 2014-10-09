using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Contents;

namespace Concentrator.Objects.Services.ServiceInterfaces
{
  public interface IContentVendorSettingService
  {
    /// <summary>
    /// Retrieves all contentvendorsettings for a specified productgroupmappingid and connectorid
    /// </summary>
    /// <param name="productGroupMappingID"></param>
    /// <param name="connectorID"></param>
    /// <returns></returns>
    IQueryable<ContentVendorSetting> GetByProductGroupMapping(int productGroupMappingID, int connectorID);
  }
}
