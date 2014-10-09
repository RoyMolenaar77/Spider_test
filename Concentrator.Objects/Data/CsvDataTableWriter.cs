using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Data
{
  public class CsvDataTableWriter : IDataTableWriter
  {
    private StreamWriter StreamWriter
    {
      get;
      set;
    }

    public CsvDataTableWriter(Stream stream, Encoding encoding)
      : this(new StreamWriter(stream, encoding))
    {
    }

    public CsvDataTableWriter(StreamWriter streamWriter)
    {
    }

    public void Dispose()
    {
    }

    public void Write(DataTable dataTable)
    {
    }
  }
}
