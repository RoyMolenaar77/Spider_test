using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Lucene
{
  public class SearchIndexModel
  {
    public int ProductID
    {
      get;
      set;
    }

    public string ProductName
    {
      get;
      set;
    }

    public string ProductDescription
    {
      get;
      set;
    }

    public string ImagePath
    {
      get;
      set;
    }

    public string VendorItemNumber
    {
      get;
      set;
    }

    public string CustomItemNumber
    {
      get;
      set;
    }
  }
}
