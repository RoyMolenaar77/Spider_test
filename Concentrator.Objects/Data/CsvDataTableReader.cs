using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Data
{
  public class CsvDataTableReader : IDataTableReader
  {
    private Stream Stream
    {
      get;
      set;
    }
    
    public CsvDataTableReader(Stream stream, Boolean disposeStream = true)
    {
      if (stream == null)
      {
        //throw new Inv
      }
    }

    public void Dispose()
    {
    }

    public void Read(DataTable dataTable)
    {
     // new StreamReader()
    }
  }
}
