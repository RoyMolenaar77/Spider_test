using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

namespace Concentrator.Objects.Write
{
  class CsvWriter
  {

    private DataTable _table;
    private char _delimiter;
    private StringBuilder _sb;

    public CsvWriter(DataTable table, char delimiter)
    {
      _table = table;
      _delimiter = delimiter;
    }

    public void WriteToDisk(string path, string filename)
    {
      var columns = _table.Columns.Count;

      _sb = new StringBuilder();

      foreach (DataRow row in _table.Rows)
      {
        string line = "";
        for (int i = 0; i < columns; i++)
        {
          line += row[i].ToString();
          if (i != (columns - 1))
          {
            line += _delimiter;
          }
        }
        _sb.AppendLine(line);
      }
      File.WriteAllText(path, _sb.ToString());
    }

    public void WriteToStream(Stream inputStream)
    {

    }

    public string AsString()
    {
      if (_sb != null)
      {
        return _sb.ToString();
      }
      else
      {
        Process();
        return _sb.ToString();
      }
    }

    private void Process()
    {
      var columns = _table.Columns.Count;

      var header = "";
      for (int c = 0; c < columns; c++)
      {
        header += _table.Columns[c].ToString();
        if (c != (columns - 1))
        {
          header += _delimiter;
        }
      }

      _sb = new StringBuilder();
      if (header != "") _sb.AppendLine(header);

      foreach (DataRow row in _table.Rows)
      {
        string line = "";
        for (int i = 0; i < columns; i++)
        {
          line += row[i].ToString();
          if (i != (columns - 1))
          {
            line += _delimiter;
          }
        }
        _sb.AppendLine(line);
      }
    }
  }
}
