using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Axapta.Repositories
{
  public interface IVendorSettingRepository
  {
    Dictionary<string, string> GetVendorSettings(int vendorID);
    string GetVendorSetting(int vendorID, string SettingKey);
  }
}
