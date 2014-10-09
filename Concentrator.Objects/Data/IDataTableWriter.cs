using System;
using System.Data;

namespace Concentrator.Objects.Data
{
  public interface IDataTableWriter : IDisposable
  {
    void Write(DataTable dataTable);
  }
}
