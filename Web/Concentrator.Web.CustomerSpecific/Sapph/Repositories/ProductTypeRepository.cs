using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Web.CustomerSpecific.Sapph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Text;

namespace Concentrator.Web.CustomerSpecific.Sapph.Repositories
{
  public class ProductTypeRepository
  {
    private IServiceUnitOfWork _unit;
    private int _vendorID;
    private VendorSetting _vendorSetting;
    private const string _settingKey = "ProductTypes";



    public ProductTypeRepository(IServiceUnitOfWork unit, int vendorID)
    {
      _unit = unit;
      _vendorID = vendorID;
      //check and create setting if not existing
      var vendorSetting = _unit.Service<VendorSetting>().Get(c => c.VendorID == vendorID && c.SettingKey == _settingKey);
      if (vendorSetting == null)
      {
        vendorSetting = new VendorSetting()
        {
          SettingKey = _settingKey,
          VendorID = _vendorID,
          Value = string.Empty
        };
        unit.Service<VendorSetting>().Create(vendorSetting);
        unit.Save();
      }

      _vendorSetting = vendorSetting;
    }

    public List<ProductTypeModel> GetAll()
    {

      return string.IsNullOrEmpty(_vendorSetting.Value) ? new List<ProductTypeModel>() : _vendorSetting.Value.FromJson<List<ProductTypeModel>>();
    }


    public ProductTypeModel Get(string type)
    {
      return GetAll().FirstOrDefault(c => c.Type == type);
    }

    public void Add(ProductTypeModel model)
    {
      var currentList = new List<ProductTypeModel>();
      if (!string.IsNullOrEmpty(_vendorSetting.Value))
      {
        currentList = _vendorSetting.Value.FromJson<List<ProductTypeModel>>();
      }

      currentList.Add(model);
      _vendorSetting.Value = currentList.ToJson();

      _unit.Save();
    }

    public void Delete(string type)
    {
      var currentList = new List<ProductTypeModel>();
      if (!string.IsNullOrEmpty(_vendorSetting.Value))
      {
        currentList = _vendorSetting.Value.FromJson<List<ProductTypeModel>>();
      }

      var toRemove = currentList.FirstOrDefault(c => c.Type == type);

      currentList.Remove(toRemove);
      _vendorSetting.Value = currentList.ToJson();

      _unit.Save();
    }

    public void Update(string type, string translation, bool? isBra, string productType, int? LanguageID)
    {
      var currentList = new List<ProductTypeModel>();
      if (!string.IsNullOrEmpty(_vendorSetting.Value))
      {
        currentList = _vendorSetting.Value.FromJson<List<ProductTypeModel>>();
      }

      var setting = currentList.FirstOrDefault(c => c.Type == type);
      if (setting == null)
      {
        setting = new ProductTypeModel();
        currentList.Add(setting);
        setting.Type = type;
      }
          
      if (isBra.HasValue)
        setting.IsBra = isBra.Value;
      if (LanguageID.HasValue)
      {
        Dictionary<int, string> translations = setting.Translations ?? new Dictionary<int, string>();
     
        if (!translations.ContainsKey(LanguageID.Value))
          translations.Add(LanguageID.Value, translation);
        else
          translations[LanguageID.Value] = translation;

        setting.Translations = translations;
      }

      if (string.IsNullOrEmpty(productType))
        setting.ProductType = null;
      else
        setting.ProductType = (ProductTypeEnum)Enum.Parse(typeof(ProductTypeEnum), productType);

      _vendorSetting.Value = currentList.ToJson();

      _unit.Save();
    }
  }
}
