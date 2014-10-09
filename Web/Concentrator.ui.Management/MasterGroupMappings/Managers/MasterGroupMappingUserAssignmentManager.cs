using System;
using System.Configuration;
using System.Linq;
using Concentrator.Objects.Model.Users;
using Concentrator.Objects.Models;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Bootstrapper.Installation;
using System.Collections.Generic;

namespace Concentrator.ui.Management.MasterGroupMappings.Managers
{
  class MasterGroupMappingUserAssignmentManager
  {
    private readonly IService<Role> _roleService;
    private readonly IService<Objects.Models.MastergroupMapping.MasterGroupMapping> _masterGroupMappingService;
    private readonly IService<User> _userService;
    private readonly IService<FunctionalityRole> _functionalityRoleService; 

    public MasterGroupMappingUserAssignmentManager(
      IService<Role> roleService,
      IService<Objects.Models.MastergroupMapping.MasterGroupMapping> masterGroupMappingService,
      IService<User> userService, IService<FunctionalityRole> functionalityRoleService)
    {
      _roleService = roleService;
      _masterGroupMappingService = masterGroupMappingService;
      _userService = userService;
      _functionalityRoleService = functionalityRoleService;
    }

    public void Assign(int masterGroupMappingID, int userId)
    {
      AssignMasterGroupMapping(masterGroupMappingID, userId);
      AssignRole(userId);
    }

    public void Unassign(int masterGroupMappingID, int userId)
    {
      UnassignMasterGroupMapping(masterGroupMappingID, userId);
      UnassignUserFromRole(masterGroupMappingID, userId);
    }


    #region MasterGroupMapping assignment

    private void AssignMasterGroupMapping(int masterGroupMappingId, int userId)
    {
      var children = _masterGroupMappingService
        .GetAll(x => x.ParentMasterGroupMappingID.HasValue
                     && x.ParentMasterGroupMappingID.Value == masterGroupMappingId).ToList();

      children.ForEach(x => AssignMasterGroupMapping(x.MasterGroupMappingID, userId));

      var masterGroupMapping = _masterGroupMappingService
        .Get(x => x.MasterGroupMappingID == masterGroupMappingId);
      var user = _userService.Get(x => x.UserID == userId);

      masterGroupMapping.Users.Add(user);
    }

    private void UnassignMasterGroupMapping(int masterGroupMappingId, int userId)
    {
      var masterGroupMapping = _masterGroupMappingService
                                 .Get(x => x.MasterGroupMappingID == masterGroupMappingId);
      var user = _userService.Get(x => x.UserID == userId);

      masterGroupMapping.Users.Remove(user);
    }
    #endregion

    #region Assign and Unassing role

    private Role GetRole()
    {
      const string roleName = MasterGroupMappingConstants.MasterGroupMappingAssigneeRoleName;
      var role = _roleService.Get(x => x.RoleName == roleName);
      if (role == null)
      {
        role = new Role
        {
          RoleName = roleName,
          isHidden = true,
        };

        var functionalityNames = new[]
          {
            Functionalities.MasterGroupMappingViewVendorProducts,
            Functionalities.MasterGroupMappingViewVendorProductGroups
          }.Select(x=>x.ToString()).ToArray();

        var functionalities = _functionalityRoleService.GetAll(x => functionalityNames.Contains(x.FunctionalityName));

        role.FunctionalityRoles = new List<FunctionalityRole>();

        foreach (var functionality in functionalities)
        {
          role.FunctionalityRoles.Add(functionality);
        }
      }
      return role;
    }

    private UserRole GetUserForRole(int userId, Role role)
    {
      UserRole userRole = null;
      if (role.RoleID > 0)
      {
        userRole = _roleService.Get(x => x.RoleID == role.RoleID).UserRoles.SingleOrDefault(x => x.UserID == userId);
      }
      return userRole;
    }

    public void UnassignUserFromRole(int masterGroupMappingID, int userId)
    {
      var hasAnyMasterGroupMappingsAssigned = IsUserAssignedToOtherMasterGroupMappings(userId, masterGroupMappingID);

      if (!hasAnyMasterGroupMappingsAssigned) return;

      var role = GetRole();
      var user = GetUserForRole(userId, role);
      if (user == null) return;

      role.UserRoles.Remove(user);
    }

    private bool IsUserAssignedToOtherMasterGroupMappings(int masterGroupMappingID, int userId)
    {
      return _masterGroupMappingService.GetAll(x => x.MasterGroupMappingID != masterGroupMappingID && x.Users.Any(y => y.UserID == userId)).Any();
    }

    public void AssignRole(int userId)
    {
      //asssign a user to role

      var role = GetRole();
      var user = GetUserForRole(userId, role);

      if (user != null) return;

      role.UserRoles.Add(user);

    }
    #endregion

    
  }
}