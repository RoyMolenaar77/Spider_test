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
  public class ProductModelRepository
  {
    private IServiceUnitOfWork _unit;
    private int _vendorID;
    private VendorSetting _vendorSetting;
    private const string _settingKey = "ModelDescriptions";



    public ProductModelRepository(IServiceUnitOfWork unit, int vendorID)
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

    public List<ModelDescriptionModel> GetAll()
    {
      return string.IsNullOrEmpty(_vendorSetting.Value) ? new List<ModelDescriptionModel>() : _vendorSetting.Value.FromJson<List<ModelDescriptionModel>>();
    }


    public ModelDescriptionModel Get(string modelCode)
    {
      return GetAll().FirstOrDefault(c => c.ModelCode == modelCode);
    }

    public void Add(ModelDescriptionModel model)
    {
      var currentList = new List<ModelDescriptionModel>();
      if (!string.IsNullOrEmpty(_vendorSetting.Value))
      {
        currentList = _vendorSetting.Value.FromJson<List<ModelDescriptionModel>>();
      }

      currentList.Add(model);
      _vendorSetting.Value = currentList.ToJson();

      _unit.Save();
    }

    public void Delete(string modelCode)
    {
      var currentList = new List<ModelDescriptionModel>();
      if (!string.IsNullOrEmpty(_vendorSetting.Value))
      {
        currentList = _vendorSetting.Value.FromJson<List<ModelDescriptionModel>>();
      }

      var toRemove = currentList.FirstOrDefault(c => c.ModelCode == modelCode);

      currentList.Remove(toRemove);
      _vendorSetting.Value = currentList.ToJson();

      _unit.Save();
    }

    public void Update(string modelCode, string translation)
    {
      var currentList = new List<ModelDescriptionModel>();
      if (!string.IsNullOrEmpty(_vendorSetting.Value))
      {
        currentList = _vendorSetting.Value.FromJson<List<ModelDescriptionModel>>();
      }

      var setting = currentList.FirstOrDefault(c => c.ModelCode == modelCode);
      if (setting == null)
      {
        setting = new ModelDescriptionModel();
        currentList.Add(setting);
        setting.ModelCode = modelCode;
      }

      setting.Translation = translation;

      _vendorSetting.Value = currentList.ToJson();

      _unit.Save();
    }
  }
}
