using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace Concentrator.Objects.Parse
{

  public interface IContentRecordProvider<T> : IEnumerable<T>, IDisposable
  {
    
  }
}
