using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Utility.TransferServices.Interfaces
{
  public interface ITransferAdapter
  {    
    Boolean Init(Uri uri);
    Boolean Upload(Stream file, string fileName);
    Stream Download(string fileName);        
  }
}
