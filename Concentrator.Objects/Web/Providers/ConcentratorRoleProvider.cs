using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using Ninject;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.DataAccess.Repository;
using Microsoft.Practices.ServiceLocation;

namespace Concentrator.Objects.Web.Providers
{
  public class ConcentratorRoleProvider : RoleProvider
  {
    
    
    private string _applicationName = "ConcentratorRoleProvider";
    public override void AddUsersToRoles(string[] usernames, string[] roleNames)
    {
      throw new NotImplementedException();
    }

    public override string ApplicationName
    {
      get
      {
        return _applicationName;
      }
      set
      {
        _applicationName = value;
      }
    }

    public override void CreateRole(string roleName)
    {
      throw new NotImplementedException();
    }

    public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
    {
      throw new NotImplementedException();
    }

    public override string[] FindUsersInRole(string roleName, string usernameToMatch)
    {
      throw new NotImplementedException();
    }

    public override string[] GetAllRoles()
    {
      throw new NotImplementedException();
    }

    public override string[] GetRolesForUser(string username)
    {
      var roles = (from ur in ServiceLocator.Current.GetInstance<IRepository<User>>().GetSingle(c => c.Username == username).UserRoles
                   select ur.Role.RoleName).ToArray();

      return roles;

    }

    public override string[] GetUsersInRole(string roleName)
    {
      throw new NotImplementedException();
    }

    public override bool IsUserInRole(string username, string roleName)
    {
      return ServiceLocator.Current.GetInstance<IRepository<User>>().GetSingle(c => c.Username == username).UserRoles.Any(c => c.Role.RoleName == roleName);
    }

    public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
    {
      throw new NotImplementedException();
    }

    public override bool RoleExists(string roleName)
    {
      throw new NotImplementedException();
    }
  }
}
