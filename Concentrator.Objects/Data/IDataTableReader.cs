using System;
using System.Data;

namespace Concentrator.Objects.Data
{
  public interface IDataTableReader : IDisposable
  {
    void Read(DataTable dataTable);
  }
}
