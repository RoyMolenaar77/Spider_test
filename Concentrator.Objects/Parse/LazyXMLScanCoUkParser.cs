using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Linq;

namespace Concentrator.Objects.Parse
{
  public class LazyXMLScanCoUkParser : IContentRecordProvider<XElement>
  {
    
    private string _url;
    private List<string> ColumnsDef { get; set; }
    

    private XDocument _Doc;
    protected XDocument Doc
    {
      get
      {
        if (this._Doc == null)
          this._Doc = XDocument.Load(this._url);
        return this._Doc;
      }
    }

    public LazyXMLScanCoUkParser(string url, List<string> Columns)
    {
      this._url = url;
      this.ColumnsDef = Columns;
    }


   

    #region IEnumerable Members

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      throw new NotImplementedException();
    }

    #endregion

    

    public IEnumerator<XElement> GetEnumerator()
    {
      foreach (XElement cat in this.Doc.Element("categories").Elements("category"))
      {
        foreach (XElement productContainer in cat.Elements("products"))
        {
          foreach (XElement product in productContainer.Elements("product"))
          {
            yield return product;
          }
        }
      }
      yield break;
    }

    
  }
}
