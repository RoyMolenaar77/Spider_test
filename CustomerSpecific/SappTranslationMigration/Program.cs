using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Vendors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Text;
using Concentrator.Objects.Environments;

namespace SappTranslationMigration
{
  class Program
  {
    
    private const string _settingKeyModelDescriptions = "ModelDescriptions";
    private const string _settingKeyProductType = "ProductTypes";
    private const int _vendorID = 50;
    private const string _defaultLanguageName = "Nederland";
    
    static void Main(string[] args)
    {
      //migration JSON
      MigrateModelDescriptionModelJson();
      MigrateProductTypeJson();

    }

    private static void MigrateProductTypeJson()
    {
      Console.WriteLine("Migrate producttype json");

      using (var db = new PetaPoco.Database(Environments.Current.Connection, PetaPoco.Database.MsSqlClientProvider))
      {
        var defaultLanguageID = db.SingleOrDefault<int?>(@"select LanguageID from Language where Name = @0", _defaultLanguageName);
        if (defaultLanguageID == null)
        {
          Console.WriteLine(string.Format("Language {0} does not exist", _defaultLanguageName));
          Console.Read();
          return;
        }

        var vendorSetting = db.SingleOrDefault<string>("Select Value from VendorSetting where VendorID = @0 and SettingKey = @1", _vendorID, _settingKeyProductType);

        if (vendorSetting == null)
        {
          Console.WriteLine(string.Format("Vendorsetting {0} for vendor {1} does not exist", _settingKeyProductType, _vendorID));
          Console.Read();
          return;
        }

        var productTypeDescriptionValue = vendorSetting;

        var oldModel = productTypeDescriptionValue.FromJson<List<ProductTypeModelOld>>();

        var newList = new List<ProductTypeModelNew>();

        foreach (var value in oldModel)
        {
          var newObject = new ProductTypeModelNew();

          newObject.Type = value.Type;
          newObject.IsBra = value.IsBra;
          newObject.ProductType = value.ProductType;

          newObject.Translations = new Dictionary<int, string>();
          newObject.Translations.Add(defaultLanguageID.Value, value.Translation);

          newList.Add(newObject);
        }

        var newJsonValue = newList.ToJson();

        db.Execute("Update VendorSetting set Value = @0 where SettingKey = @1 and VendorID = @2", newJsonValue, _settingKeyProductType, _vendorID);

        Console.WriteLine("Done");
      }
    }

    private static void MigrateModelDescriptionModelJson()
    {
      Console.WriteLine("Migrate productdescriptionmodel json");

      using (var db = new PetaPoco.Database(Environments.Current.Connection, PetaPoco.Database.MsSqlClientProvider))
      {
        var defaultLanguageID = db.SingleOrDefault<int?>(@"select LanguageID from Language where Name = @0", _defaultLanguageName);
        if (defaultLanguageID == null)
        {
          Console.WriteLine(string.Format("Language {0} does not exist", _defaultLanguageName));
          Console.Read();
          return;
        }

        var vendorSetting = db.SingleOrDefault<string>("Select Value from VendorSetting where VendorID = @0 and SettingKey = @1", _vendorID, _settingKeyModelDescriptions);

        if (vendorSetting == null)
        {
          Console.WriteLine(string.Format("Vendorsetting {0} for vendor {1} does not exist", _settingKeyModelDescriptions, _vendorID));
          Console.Read();
          return;
        }

        var modelDescriptionValue = vendorSetting;

        var oldModel = modelDescriptionValue.FromJson<List<ModelDescriptionModelOld>>();

        var newList = new List<ModelDescriptionModelNew>();

        foreach (var value in oldModel)
        {
          var newObject = new ModelDescriptionModelNew();

          newObject.ModelCode = value.ModelCode;
          newObject.Translations = new Dictionary<int, string>();
          newObject.Translations.Add(defaultLanguageID.Value, value.Translation);

          newList.Add(newObject);
        }

        var newJsonValue = newList.ToJson();

        db.Execute("Update VendorSetting set Value = @0 where SettingKey = @1 and VendorID = @2", newJsonValue, _settingKeyModelDescriptions, _vendorID);
        
        Console.WriteLine("Done");
      }
    }


  }

  #region ProductTypeModels

  public class ProductTypeModelOld
  {
    public string Type { get; set; }

    public string Translation { get; set; }

    public bool IsBra { get; set; }

    public ProductTypeEnum? ProductType { get; set; }
  }

  public class ProductTypeModelNew
  {
    public string Type { get; set; }

    public bool IsBra { get; set; }

    /// <summary>
    /// int: LanguageID
    /// string: Value
    /// </summary>
    public Dictionary<int, string> Translations { get; set; }

    public ProductTypeEnum? ProductType { get; set; }
  }

  public enum ProductTypeEnum
  {
    Bottoms,
    Tops
  }

  #endregion

  #region ModelDescriptionModels

  public class ModelDescriptionModelOld
  {
    public string ModelCode { get; set; }

    public string Translation { get; set; }
  }


  public class ModelDescriptionModelNew
  {
    public string ModelCode { get; set; }

    public string Translation { get; set; }

    public Dictionary<int, string> Translations { get; set; }
  }

  #endregion

}
