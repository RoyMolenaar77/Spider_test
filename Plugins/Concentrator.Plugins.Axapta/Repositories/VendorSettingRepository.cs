using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Vendors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Axapta.Repositories
{
  public class VendorSettingRepository : UnitOfWorkPlugin, IVendorSettingRepository
  {
    public Dictionary<string, string> GetVendorSettings(int vendorID)
    {
      using (var db = GetUnitOfWork())
      {
        return db
          .Scope
          .Repository<VendorSetting>()
          .GetAll(x => x.VendorID == vendorID)
          .ToDictionary(x => x.SettingKey, x => x.Value);
      }
    }

    public string GetVendorSetting(int vendorID, string SettingKey)
    {
      using (var db = GetUnitOfWork())
      {
        return db
          .Scope
          .Repository<VendorSetting>()
          .GetAll(x => x.SettingKey == SettingKey && x.VendorID == vendorID)
          .Select(x => x.Value)
          .FirstOrDefault();
      }
    }
  }
}
