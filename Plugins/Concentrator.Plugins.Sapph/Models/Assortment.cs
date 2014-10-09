using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Sapph.Models
{
  public class Assortment
  {
    public string SupplierArticlecode { get; set; }
    public string ColourcodeSupplier { get; set; }
    public string SizeSupplier { get; set; }
    public string SubsizeSupplier { get; set; }

    public string Brand { get; set; }
    public string Barcode { get; set; }
    public string ShortDescription { get; set; }
    public string LongDescription { get; set; }

    public string Website { get; set; } // XML tag Website: [WEB, NON-WEB], Geeft aan of het product voor de website bedoeld.

    public string ProductGroupCode1 { get; set; }
    public string ProductGroupCode2 { get; set; }

    public string ModelCode
    {
      get { return SupplierArticlecode.Try(c => c.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries)[0], string.Empty); }
    }

    public string TypeCode
    {
      get
      {
        var types = SupplierArticlecode.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

        return types.Length > 1 ? types[1] : string.Empty;

        //return SupplierArticlecode.Try(c => c.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries)[1], string.Empty);
      }
    }

    public string ProductGroupCode3
    {
      get
      {
        string[] split = SupplierArticlecode.Split('-');
        return split[0];
      }
    }

    public decimal AdvicePrice { get; set; }
    public decimal LabellingPrice { get; set; }

    public string ConfigurableVendorItemNumber
    {
      get
      {
        return SupplierArticlecode;
      }
    }
    public string SimpleVendorItemNumber
    {
      get
      {
        return string.Format("{0} {1} {2} {3}", SupplierArticlecode, ColourcodeSupplier, SizeSupplier, (SubsizeSupplier != null ? SubsizeSupplier : string.Empty));
      }
    }
  }
}
