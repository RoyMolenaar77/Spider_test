using System;
using System.Collections.Generic;
using System.Linq;
using Concentrator.Objects.Bootstrapper.Installation.Roles;
using Concentrator.Objects.Models.Users;

namespace Concentrator.Objects.Bootstrapper.Installation
{
  public static class MasterGroupMappingConstants
  {
    public const string MasterGroupMappingAssigneeRoleName = "MasterGroupMapping assignee";
    public const string MasterGroupMappingAdminRoleName = "MasterGroupMapping admin";
    public const string MasterGroupMappingUserRoleName = "MasterGroupMapping user";
  }

  class MasterGroupMappingRolesInstaller : RoleInstaller
  {
    protected override IEnumerable<RoleToInstall> GetRolesToInstall()
    {
      var allFunctionalities = GetAllMasterGroupMappingFunctionalities()
        .Union(new[] { Functionalities.View_MasterGroupMappings })
        .Distinct()
        .ToArray();
      var roles = new[]
        {
          new RoleToInstall(MasterGroupMappingConstants.MasterGroupMappingAdminRoleName, allFunctionalities),
          new RoleToInstall(
            MasterGroupMappingConstants.MasterGroupMappingUserRoleName,
            Functionalities.View_MasterGroupMappings,
            Functionalities.DefaultMasterGroupMapping,
            Functionalities.MasterGroupMappingAddProductGroupMapping,
            Functionalities.MasterGroupMappingDeleteProductGroupMapping,
            Functionalities.MasterGroupMappingProductGroupSettings,
            Functionalities.MasterGroupMappingRenameConnectorMapping,
            Functionalities.MasterGroupMappingFindSource,
            Functionalities.MasterGroupMappingViewProducts,
            Functionalities.MasterGroupMappingViewGroupAttributeMapping,
            Functionalities.MasterGroupMappingViewPriceRule,
            Functionalities.MasterGroupMappingViewPriceTagMapping,
            Functionalities.MasterGroupMappingViewConnectorMapping,
            Functionalities.MasterGroupMappingMoveProductGroupMapping,
            Functionalities.MasterGroupMappingChooseSource,
            Functionalities.MasterGroupMappingAddConnectorMapping,
            Functionalities.MasterGroupMappingDeleteConnectorMapping,
            Functionalities.MasterGroupMappingViewConnectorPublicationRule
          ),
          new RoleToInstall(
            MasterGroupMappingConstants.MasterGroupMappingAssigneeRoleName, 
            /*isHidden:*/true,
            Functionalities.View_MasterGroupMappings,
            Functionalities.DefaultMasterGroupMapping,
            Functionalities.MasterGroupMappingViewVendorProducts,
            Functionalities.MasterGroupMappingVendorProductGroupsManagement
          ),
        };
      return roles;
    }

    private static IEnumerable<Functionalities> GetAllMasterGroupMappingFunctionalities()
    {
      return Enum.GetValues(typeof(Functionalities)).Cast<Functionalities>().Where(x =>
      {
        var info = x.GetAttribute<FunctionalityInfoAttribute>();
        return info != null && info.Group.Contains(FunctionalityGroups.MasterGroupMapping);
      });
    }
  }
}