using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Models.Products;

namespace Concentrator.Objects.Services
{
  public class ContentVendorSettingService : Service<ContentVendorSetting>, IContentVendorSettingService
  {
    public override void Create(ContentVendorSetting model)
    {
      var rec = Repository().GetSingle(c => c.ConnectorID == model.ConnectorID && c.ProductGroupID == model.ProductGroupID && c.ContentVendorIndex == model.ContentVendorIndex);

      if (rec != null)
        throw new InvalidOperationException("Duplicate index for a connector/product group combination");

      base.Create(model);
    }

    #region IContentVendorSettingService Members

    public IQueryable<ContentVendorSetting> GetByProductGroupMapping(int productGroupMappingID, int connectorID)
    {
      var productGroupID = Repository<ProductGroupMapping>().GetSingle(c => c.ProductGroupMappingID == productGroupMappingID).ProductGroupID;
      return base.GetAll().Where(c => c.ProductGroupID == productGroupID && c.ConnectorID == connectorID);
    }

    #endregion
  }
}
