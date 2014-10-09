using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;
using System.Xml.Linq;

namespace Concentrator.Objects.Vendors.Base
{
  public class XmlDataReader : IDataReader
  {
    public XmlDataReader()
    {

    }

    public virtual int FieldCount
    {
      get { return -1; }
    }

    public object GetValue(int i)
    {
      return CurrentElement[i];
    }

    public virtual bool IsElement(XmlReader reader)
    {
      return false;
    }

    private XmlReader _xmlReader;

    private bool _disposed;

    protected IEnumerator<object[]> _enumerator;

    public virtual void Load(XmlReader reader)
    {
      _xmlReader = reader;
      _enumerator = GetXmlStream().GetEnumerator();
    }

    public bool Read()
    {
      return _enumerator.MoveNext();
    }

    public Object[] CurrentElement
    {
      get { return _enumerator.Current; }
    }

    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/system.xml.linq.xstreamingelement.aspx
    /// </summary>
    /// <param name="m_xmlReader"></param>
    /// <returns></returns>
    private IEnumerable<object[]> GetXmlStream()
    {
      return Enumerate(_xmlReader);
    }

    protected virtual IEnumerable<object[]> Enumerate(XmlReader reader)
    {
      XElement rowElement;

      reader.MoveToContent();

      while (_xmlReader.Read())
      {
        if (IsRowElement())
        {
          rowElement = XElement.ReadFrom(_xmlReader) as XElement;
          if (rowElement != null)
          {
            object[] row = new object[FieldCount];
            FillRow(rowElement, row);
            yield return row;
          }
        }
      }
    }

    protected virtual void FillRow(XElement element, object[] row)
    {

    }

    private bool IsRowElement()
    {
      if (_xmlReader.NodeType != XmlNodeType.Element)
        return false;


      return IsElement(_xmlReader);
    }

    public virtual void Dispose()
    {
      if (_disposed)
        return;

      _enumerator.Dispose();
      _disposed = true;
    }

    public bool NextResult()
    {
      throw new NotImplementedException();
    }

    public int RecordsAffected
    {
      get { throw new NotImplementedException(); }
    }

    public string GetDataTypeName(int i)
    {
      throw new NotImplementedException();
    }

    // Deleted tons of methods not required...

    #region IDataReader Members

    public void Close()
    {
      throw new NotImplementedException();
    }

    public int Depth
    {
      get { throw new NotImplementedException(); }
    }

    public DataTable GetSchemaTable()
    {
      throw new NotImplementedException();
    }

    public bool IsClosed
    {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region IDataRecord Members


    public bool GetBoolean(int i)
    {
      throw new NotImplementedException();
    }

    public byte GetByte(int i)
    {
      throw new NotImplementedException();
    }

    public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
    {
      throw new NotImplementedException();
    }

    public char GetChar(int i)
    {
      throw new NotImplementedException();
    }

    public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
    {
      throw new NotImplementedException();
    }

    public IDataReader GetData(int i)
    {
      throw new NotImplementedException();
    }

    public DateTime GetDateTime(int i)
    {
      throw new NotImplementedException();
    }

    public decimal GetDecimal(int i)
    {
      throw new NotImplementedException();
    }

    public double GetDouble(int i)
    {
      throw new NotImplementedException();
    }

    public Type GetFieldType(int i)
    {
      throw new NotImplementedException();
    }

    public float GetFloat(int i)
    {
      throw new NotImplementedException();
    }

    public Guid GetGuid(int i)
    {
      throw new NotImplementedException();
    }

    public short GetInt16(int i)
    {
      throw new NotImplementedException();
    }

    public int GetInt32(int i)
    {
      throw new NotImplementedException();
    }

    public long GetInt64(int i)
    {
      throw new NotImplementedException();
    }

    public string GetName(int i)
    {
      throw new NotImplementedException();
    }

    public int GetOrdinal(string name)
    {
      throw new NotImplementedException();
    }

    public string GetString(int i)
    {
      throw new NotImplementedException();
    }

    public int GetValues(object[] values)
    {
      throw new NotImplementedException();
    }

    public bool IsDBNull(int i)
    {
      throw new NotImplementedException();
    }

    public object this[string name]
    {
      get { throw new NotImplementedException(); }
    }

    public object this[int i]
    {
      get { throw new NotImplementedException(); }
    }

    #endregion
  }



}
