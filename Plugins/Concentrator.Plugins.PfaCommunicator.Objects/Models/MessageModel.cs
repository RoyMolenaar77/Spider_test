using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PfaCommunicator.Objects.Models
{
  public class MessageModel
  {
    public int Type { get; set; }

    public string LocalSubPath { get; set; }

    public string RemoteSubPath { get; set; }

    public bool Incoming { get; set; }
  }
}
