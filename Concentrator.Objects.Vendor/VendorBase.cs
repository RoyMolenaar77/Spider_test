using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using System.Configuration;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Parse;

namespace Concentrator.Objects.Vendors
{
  public abstract class VendorBase : ConcentratorPlugin
  {
    protected new abstract int VendorID { get; }
    protected abstract int DefaultVendorID { get; }
    protected abstract System.Configuration.Configuration Config { get; }
    public const int UnMappedID = -1;
    public new const string GeneralProductGroupName = "General";

    protected int GetGeneralAttributegroupID(IUnitOfWork unit)
    {
      int generalAttributegroupID;

      var attributeGroupNameRepo = unit.Scope.Repository<ProductAttributeGroupName>();

      generalAttributegroupID = (from pa in attributeGroupNameRepo.GetAll()
                                 where pa.LanguageID == (int)LanguageTypes.English &&
                                 pa.Name == GeneralProductGroupName
                                 && !pa.ProductAttributeGroupMetaData.ConnectorID.HasValue
                                 && pa.ProductAttributeGroupMetaData.VendorID == VendorID
                                 select pa.ProductAttributeGroupID).FirstOrDefault();

      if (generalAttributegroupID == default(int))
      {
        var attributeGroup = new ProductAttributeGroupMetaData
        {
          Index = 0,
          VendorID = VendorID
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

    /// <summary>
    /// If needed, creates the attributes passed in
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="attributeMapping">Key = name, Value = IsConfigurable</param>
    /// <param name="attributes"></param>
    /// <param name="vendorID"></param>
    protected void SetupAttributes(IUnitOfWork unit, Dictionary<string, bool> attributeMapping, out List<ProductAttributeMetaData> attributes, int? vendorID)
    {
      int generalAttributegroupID = GetGeneralAttributegroupID(unit);

      var attributeRepo = unit.Scope.Repository<ProductAttributeMetaData>();
      var attributeNameRepo = unit.Scope.Repository<ProductAttributeName>();
      #region Basic Attributes

      var attributesTmp = (from a in attributeRepo.GetAll()
                           where attributeMapping.Keys.Contains(a.AttributeCode) &&
                                 a.VendorID == (vendorID.HasValue ? vendorID.Value : VendorID)
                           select a).ToList();

      var attributesToAdd = from a in attributeMapping
                            where !attributesTmp.Any(at => at.AttributeCode == a.Key)
                            select a;

      foreach (var toAdd in attributesToAdd)
      {
        var newAttribute = new ProductAttributeMetaData
        {
          AttributeCode = toAdd.Key,
          IsVisible = true,
          VendorID = (vendorID.HasValue ? vendorID.Value : VendorID),
          ProductAttributeGroupID = generalAttributegroupID,
          Index = 0,
          IsConfigurable = toAdd.Value,
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
          Name = toAdd.Key
        };
        attributeNameRepo.Add(attNameEng);

        var attNameDut = new ProductAttributeName
        {
          ProductAttributeMetaData = newAttribute,
          LanguageID = (int)LanguageTypes.Netherlands,
          Name = toAdd.Key
        };
        attributeNameRepo.Add(attNameDut);
        unit.Save();
      }


      #endregion Basic Attributes

      attributes = attributesTmp;
    }

    protected void SetupAttributes(IUnitOfWork unit, string[] attributeMapping, out List<ProductAttributeMetaData> attributes, int? vendorID)
    {
      var attr = ((from a in attributeMapping select new { Code = a, IsConfigurable = false }).ToDictionary(c => c.Code, c => c.IsConfigurable));

      SetupAttributes(unit, attr, out attributes, vendorID);
    }


    //public abstract string Name
    //{
    //  get;
    //}

    //protected abstract void Process()
    //{
    //  get;
    //}

    public abstract override string Name
    {
      get;
    }

    //protected abstract IContentRecordProvider Provider
    //{
    //  get;
    //}

    protected abstract void SyncProducts();

    protected override void Process()
    {
      SyncProducts();
    }
  }
}
