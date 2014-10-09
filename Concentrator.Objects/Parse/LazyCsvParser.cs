using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Parse
{
  public class LazyCsvParser : IContentRecordProvider<Dictionary<string, string>> // IDisposable, IEnumerable<Dictionary<string, string>>
  {
    private StreamReader _reader;
    private Stream _stream;


    protected StreamReader Reader
    {
      get
      {
        if (_reader == null)
        {
          if (_stream != null)
          {
            _reader = new StreamReader(_stream, Encoding.UTF8, false);
          }
          else
          {
            if (Location.IsFile)
            {
              _reader = new StreamReader(Location.OriginalString);
            }
            else if (Location.Scheme.ToLower() == "http")
            {

            }
          }
        }

        return _reader;
      }
    }

    protected Uri Location { get; set; }
    protected List<string> ColumnDef { get; set; }
    protected bool ContainsHeaders { get; set; }
    public Func<string, IEnumerable<string>> ColumnSplitter { get; set; }

    protected LazyCsvParser(List<string> columnDef, bool containsHeaders)
    :this(columnDef, containsHeaders, ';')
    
    {
    }

    public LazyCsvParser(List<string> columnDef, bool containsHeaders, char columnSplitter)
    {
      ColumnDef = columnDef;
      ContainsHeaders = containsHeaders;
      ColumnSplitter = line => line.Split(columnSplitter);
    }

    public LazyCsvParser(string path, List<string> columnDef, bool containsHeaders)
      : this(columnDef, containsHeaders)
    {
      if (!File.Exists(path))
        throw new ArgumentException(String.Format("File does not exist: {0}", path));

      Location = new Uri(path);
    }

    public LazyCsvParser(Stream source, List<string> columnDef, bool containsHeaders)
      : this(columnDef, containsHeaders)
    {
      this._stream = source;
    }

    public LazyCsvParser(Stream source, List<string> columnDef, bool containsHeaders, char columnSplitter)
      : this(columnDef, containsHeaders,columnSplitter)
    {
      this._stream = source;
    }

    #region IDisposable Members

    public void Dispose()
    {
      if (_reader != null)
        _reader.Dispose();
    }

    #endregion

    #region IEnumerable<Dictionary<string,string>> Members

    private string _document;

    public string Document
    {
      get { return _document; }
  }

    public IEnumerator<Dictionary<string, string>> GetEnumerator()
    {

      if (ContainsHeaders)
        Reader.ReadLine(); // Skip headers if necessary
      while (!Reader.EndOfStream)
      {
        string line = Reader.ReadLine();
        _document += line;

        var row = (from c in ColumnSplitter(line)
                   select c.Trim()).ToArray();// .Split(';');

        Dictionary<string, string> result = new Dictionary<string, string>(row.Length);
        for (int i = 0; i < ColumnDef.Count; i++)
        {
          result.Add(ColumnDef[i], row[i]);
        }
        yield return result;
      }

      yield break;
    }

    #endregion

    #region IEnumerable Members

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

  }
}