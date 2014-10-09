using Concentrator.Objects.Models.Vendors;
using Concentrator.Plugins.PfaCommunicator.Objects.Models;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PfaCommunicator.Objects.Repositories
{
  public class PfaCommunicatorRepository
  {
    private Database _db;
    public PfaCommunicatorRepository(Database db)
    {
      _db = db;
    }

    public List<Vendor> GetVendorsWithPfaCommunication()
    {
      return _db.Fetch<Vendor>(@"select VendorID, Name from vendor where isactive = 1 and VendorType & @0 = @0", (int)VendorType.HasPfaCommunication);
    }

    public string GetVendorSetting(int vendorID, string settingkey)
    {
      return _db.SingleOrDefault<string>("select value from vendorsetting where vendorid = @0 and settingKey = @1", vendorID, settingkey);
    }


    internal MessageModel GetMessageForVendor(MessageTypes type, int vendorID)
    {
      return _db.SingleOrDefault<MessageModel>(@"select LocalSubPath from communicatormessage cm
                                      inner join vendorcommunicatormessage vm on vm.messageid = cm.id
                                      where vm.vendorid = @0 and cm.type = @1", vendorID, (int)type);
    }

    internal List<MessageModel> GetMessageTypesForVendor(int _vendorID)
    {
      return _db.Fetch<MessageModel>(@"select Type, LocalSubPath, RemoteSubPath, Incoming from communicatormessage cm
                                      inner join vendorcommunicatormessage vm on vm.messageid = cm.id
                                      where vm.vendorid = @0", _vendorID);
    }
  }
}
