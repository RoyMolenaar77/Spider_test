using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PfaCommunicator.Objects.Models
{
  public class VendorMessageModel
  {
    public string LocalDirectory { get; set; }

    public string RemoteDirectory { get; set; }

    public string ArchiveDirectory { get; set; }

    public string UsernameForRemoteDirectory { get; set; }

    public string PasswordForRemoteDirectory { get; set; }

    public List<MessageModel> Messages { get; set; }
  }
}
