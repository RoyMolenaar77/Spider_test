using Concentrator.Objects.Environments;
using Concentrator.Plugins.PfaCommunicator.Objects.Helpers;
using Concentrator.Plugins.PfaCommunicator.Objects.Models;
using Concentrator.Plugins.PfaCommunicator.Objects.Repositories;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concentrator.Plugins.PfaCommunicator.Objects.Services
{
  public static class CommunicatorService
  {
    public static MessagePathResult GetMessagePath(int vendorID, MessageTypes type)
    {
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        PFACommunicatorHelper helper = new PFACommunicatorHelper(new PfaCommunicatorRepository(db), vendorID);

        var path = helper.GetLocalMessagePath(type);

        var errorPath = Path.Combine(path, "Error");
        var processedPath = Path.Combine(path, "Processed");

        if (!Directory.Exists(errorPath))
          Directory.CreateDirectory(errorPath);

        if (!Directory.Exists(processedPath))
          Directory.CreateDirectory(processedPath);

        return new MessagePathResult() { MessagePath = path, ErrorPath = errorPath, ProcessedPath = processedPath };
      }
    }
  }
}
