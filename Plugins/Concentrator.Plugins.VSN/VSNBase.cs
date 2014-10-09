using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;

using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Attributes;

namespace Concentrator.Plugins.VSN
{
  public abstract class VSNBase : ConcentratorPlugin
  {
    public const int VendorID = 39;
    public const string BrandVendorCode = "VSN";
    public const int UnMappedID = -1;
    public const string GeneralProductGroupName = "General";

    protected int GetGeneralAttributegroupID(IUnitOfWork unit)
    {
      int generalAttributegroupID;
      generalAttributegroupID = (from pa in unit.Scope.Repository<ProductAttributeGroupName>().GetAll()
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
        unit.Scope.Repository<ProductAttributeGroupName>().Add(groupNameEng);

        var groupNameDut = new ProductAttributeGroupName
        {
          LanguageID = (int)LanguageTypes.Netherlands,
          Name = GeneralProductGroupName,
          ProductAttributeGroupMetaData = attributeGroup
        };
        unit.Scope.Repository<ProductAttributeGroupName>().Add(groupNameDut);
        unit.Save();
        generalAttributegroupID = attributeGroup.ProductAttributeGroupID;
      }
      return generalAttributegroupID;
    }
  }
}
