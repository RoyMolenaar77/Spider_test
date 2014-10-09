using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Vendors;

namespace Concentrator.Plugins.ACSystems
{
  class StockAndPriceUpdate : VendorBase
  {
    protected override int VendorID
    {
      get { return Int32.Parse(Config.AppSettings.Settings["VendorID"].Value); }
    }

    protected override int DefaultVendorID
    {
      get { return Int32.Parse(Config.AppSettings.Settings["VendorID"].Value); }
    }

    protected override System.Configuration.Configuration Config
    {
      get { return GetConfiguration(); }
    }

    public override string Name
    {
      get { return "ACSystems Stock Update"; }
    }

    protected override void SyncProducts()
    {
      var UserName = Config.AppSettings.Settings["Username"].Value;
      var Password = Config.AppSettings.Settings["Password"].Value;

      SessionService.SessionServiceClient session = new SessionService.SessionServiceClient();

      session.GetSessionKey(UserName, Password);

      ProductInfoService.ProductInfoServiceClient client = new ProductInfoService.ProductInfoServiceClient();

      //client.Get


      throw new NotImplementedException();
    }
  }
}
