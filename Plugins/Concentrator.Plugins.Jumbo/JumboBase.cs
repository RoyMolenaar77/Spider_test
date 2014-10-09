using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using System.Configuration;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Attributes;

namespace Concentrator.Plugins.Jumbo
{
  public abstract class JumboBase : ConcentratorPlugin
  {
    protected int DefaultVendorID;
    protected System.Configuration.Configuration Config;
    public const int UnMappedID = -1;
    public const string GeneralProductGroupName = "General";

    public JumboBase()
    {
      Config = GetConfiguration();
      DefaultVendorID = Int32.Parse(Config.AppSettings.Settings["vendorID"].Value);
    }

    protected int GetGeneralAttributegroupID(IUnitOfWork unit)
    {
      int generalAttributegroupID;

      var attributeGroupNameRepo = unit.Scope.Repository<ProductAttributeGroupName>();

      generalAttributegroupID = (from pa in attributeGroupNameRepo.GetAll()
                                 where pa.LanguageID == (int)LanguageTypes.English &&
                                       pa.Name == GeneralProductGroupName
                                       && !pa.ProductAttributeGroupMetaData.ConnectorID.HasValue
                                      && pa.ProductAttributeGroupMetaData.VendorID == DefaultVendorID
                                 select pa.ProductAttributeGroupID).FirstOrDefault();

      if (generalAttributegroupID == default(int))
      {
        var attributeGroup = new ProductAttributeGroupMetaData
        {
          Index = 0,
          VendorID = DefaultVendorID
        };
        unit.Scope.Repository<ProductAttributeGroupMetaData>().Add(attributeGroup);

        var groupNameEng = new ProductAttributeGroupName
        {
          LanguageID = (int)LanguageTypes.English,
          Name = GeneralProductGroupName,
          ProductAttributeGroupMetaData = attributeGroup
        };
        attributeGroupNameRepo.Add(groupNameEng);

        var groupNameDut = new ProductAttributeGroupName
        {
          LanguageID = (int)LanguageTypes.Netherlands,
          Name = GeneralProductGroupName,
          ProductAttributeGroupMetaData = attributeGroup
        };
        attributeGroupNameRepo.Add(groupNameDut);
        unit.Save();
        generalAttributegroupID = attributeGroup.ProductAttributeGroupID;
      }
      return generalAttributegroupID;
    }

    protected void SetupAttributes(IUnitOfWork unit, string[] attributeMapping, out List<ProductAttributeMetaData> attributes, int? vendorID)
    {
      int generalAttributegroupID = GetGeneralAttributegroupID(unit);

      var attributeRepo = unit.Scope.Repository<ProductAttributeMetaData>();
      var attributeNameRepo = unit.Scope.Repository<ProductAttributeName>();
      #region Basic Attributes

      var attributesTmp = (from a in attributeRepo.GetAll()
                           where attributeMapping.Contains(a.AttributeCode) &&
                                 a.VendorID == (vendorID.HasValue ? vendorID.Value : DefaultVendorID)
                           select a).ToList();

      var attributesToAdd = from a in attributeMapping
                            where !attributesTmp.Any(at => at.AttributeCode == a)
                            select a;

      foreach (var toAdd in attributesToAdd)
      {
        var newAttribute = new ProductAttributeMetaData
        {
          AttributeCode = toAdd,
          IsVisible = true,
          VendorID = (vendorID.HasValue ? vendorID.Value : DefaultVendorID),
          ProductAttributeGroupID = generalAttributegroupID,
          Index = 0,
          NeedsUpdate = true,
          IsSearchable = attributeMapping.Contains(toAdd) ? true : false,
          Sign = String.Empty
        };

        attributeRepo.Add(newAttribute);

        attributesTmp.Add(newAttribute);

        var attNameEng = new ProductAttributeName
        {
          ProductAttributeMetaData = newAttribute,
          LanguageID = (int)LanguageTypes.English,
          Name = toAdd
        };
        attributeNameRepo.Add(attNameEng);

        var attNameDut = new ProductAttributeName
        {
          ProductAttributeMetaData = newAttribute,
          LanguageID = (int)LanguageTypes.Netherlands,
          Name = toAdd
        };
        attributeNameRepo.Add(attNameDut);
        unit.Save();
      }


      #endregion Basic Attributes

      attributes = attributesTmp;
    }

  }
}
