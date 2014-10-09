using System;
using System.Data;
using Concentrator.Objects.Environments;
using Concentrator.Plugins.Wehkamp.Enums;

namespace Concentrator.Plugins.Wehkamp.Helpers
{
  internal class VendorTranslationHelper: IDisposable
  {
    private readonly DataTable _magazijnMaatSizeTable;
    private readonly DataTable _presentatieMaatSizeTable;
    private readonly DataTable _colorTable;

    internal VendorTranslationHelper()
    {
      _magazijnMaatSizeTable = new DataTable();
      _magazijnMaatSizeTable.Columns.Add("SourceVendorID", typeof(int));
      _magazijnMaatSizeTable.Columns.Add("SourceVendorValue", typeof(string));
      _magazijnMaatSizeTable.Columns.Add("DestinationVendorID", typeof(int));
      _magazijnMaatSizeTable.Columns.Add("DestinationVendorValue", typeof(string));
      _magazijnMaatSizeTable.Columns.Add("TranslationType", typeof (int));

      _presentatieMaatSizeTable = new DataTable();
      _presentatieMaatSizeTable.Columns.Add("SourceVendorID", typeof(int));
      _presentatieMaatSizeTable.Columns.Add("SourceVendorValue", typeof(string));
      _presentatieMaatSizeTable.Columns.Add("DestinationVendorID", typeof(int));
      _presentatieMaatSizeTable.Columns.Add("DestinationVendorValue", typeof(string));
      _presentatieMaatSizeTable.Columns.Add("TranslationType", typeof(int));

      _colorTable = new DataTable();
      _colorTable.Columns.Add("SourceVendorID", typeof(int));
      _colorTable.Columns.Add("SourceVendorValue", typeof(string));
      _colorTable.Columns.Add("DestinationVendorID", typeof(int));
      _colorTable.Columns.Add("DestinationVendorValue", typeof(string));
      _colorTable.Columns.Add("TranslationType", typeof(int));


      using (var db = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var results = db.Fetch<VendorValueTranslation>(string.Format("select SourceVendorID, SourceVendorValue, DestinationVendorID, DestinationVendorValue, TranslationType from VendorTranslation WHERE TranslationType = {0}", (int)WehkampVendorTranslations.Magazijnmaat));
        
        foreach(var row in results)
          _magazijnMaatSizeTable.Rows.Add(row.SourceVendorID, row.SourceVendorValue, row.DestinationVendorID, row.DestinationVendorValue);
      }

      using (var db = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var results = db.Fetch<VendorValueTranslation>(string.Format("select SourceVendorID, SourceVendorValue, DestinationVendorID, DestinationVendorValue, TranslationType from VendorTranslation WHERE TranslationType = {0}", (int)WehkampVendorTranslations.Presentatiemaat));
        foreach (var row in results)
          _presentatieMaatSizeTable.Rows.Add(row.SourceVendorID, row.SourceVendorValue, row.DestinationVendorID, row.DestinationVendorValue);
      }

      using (var db = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var results = db.Fetch<VendorValueTranslation>(string.Format("select SourceVendorID, SourceVendorValue, DestinationVendorID, DestinationVendorValue, TranslationType from VendorTranslation WHERE TranslationType = {0}", (int)WehkampVendorTranslations.Kleurcode));
        foreach (var row in results)
          _colorTable.Rows.Add(row.SourceVendorID, row.SourceVendorValue, row.DestinationVendorID, row.DestinationVendorValue);
      }
    }

    public void Dispose()
    {
      _magazijnMaatSizeTable.Dispose();
      _presentatieMaatSizeTable.Dispose();
      _colorTable.Dispose();
    }

    //internal string TranslateMagazijnmaat(int sourceVendorID, int destinationVendorID, string size) 
    //{
    //  var query = string.Format("SourceVendorID = {0} AND DestinationVendorID = {1} AND SourceVendorValue = '{2}'", sourceVendorID, destinationVendorID, size);
    //  var translation = _magazijnMaatSizeTable.Select(query).FirstOrDefault();

    //  return translation != null ? (string)translation["DestinationVendorValue"] : string.Empty;
    //}
    //internal string TranslateMagazijnmaatBack(int sourceVendorID, int destinationVendorID, string size)
    //{
    //  var query = string.Format("SourceVendorID = {0} AND DestinationVendorID = {1} AND DestinationVendorValue = '{2}'", sourceVendorID, destinationVendorID, size);
    //  var translation = _magazijnMaatSizeTable.Select(query).FirstOrDefault();

    //  return translation != null ? (string)translation["SourceVendorValue"] : string.Empty;
    //}

    //internal string TranslatePresentatiemaat(int sourceVendorID, int destinationVendorID, string size)
    //{
    //  var query = string.Format("SourceVendorID = {0} AND DestinationVendorID = {1} AND SourceVendorValue = '{2}'", sourceVendorID, destinationVendorID, size);
    //  var translation = _presentatieMaatSizeTable.Select(query).FirstOrDefault();

    //  return translation != null ? (string)translation["DestinationVendorValue"] : string.Empty;
    //}
    //internal string TranslatePresentatiemaatBack(int sourceVendorID, int destinationVendorID, string size)
    //{
    //  var query = string.Format("SourceVendorID = {0} AND DestinationVendorID = {1} AND DestinationVendorValue = '{2}'", sourceVendorID, destinationVendorID, size);
    //  var translation = _presentatieMaatSizeTable.Select(query).FirstOrDefault();

    //  return translation != null ? (string)translation["SourceVendorValue"] : string.Empty;
    //}

    //internal string TranslateColor(int sourceVendorID, int destinationVendorID, string colorCode)
    //{
    //  var query = string.Format("SourceVendorID = {0} AND DestinationVendorID = {1} AND SourceVendorValue = '{2}'", sourceVendorID, destinationVendorID, colorCode);
    //  var translation = _colorTable.Select(query).FirstOrDefault();

    //  return translation != null ? (string)translation["DestinationVendorValue"] : string.Empty;
    //}
    


    public class VendorValueTranslation 
    {
      public int SourceVendorID { get; set; }
      public string SourceVendorValue { get; set; }
      public int DestinationVendorID { get; set; }
      public string DestinationVendorValue { get; set; }
      public int TranslationType { get; set; }
    }

    
  }
}
