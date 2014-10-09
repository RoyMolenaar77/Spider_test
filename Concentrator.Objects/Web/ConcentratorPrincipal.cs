using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using Concentrator.Objects.Models.Users;
using Ninject;
using Concentrator.Objects.DataAccess.Repository;
using Concentrator.Objects.Model.Users;

namespace Concentrator.Objects.Web
{
  [Serializable]
  public class ConcentratorPrincipal : IConcentratorPrincipal
  {
    private ConcentratorIdentity _identity;
    private List<string> _roles;
    private IKernel _kernel = null;

    public static ConcentratorPrincipal SystemPrincipal
    {
      get
      {
        return new ConcentratorPrincipal(ConcentratorIdentity.SystemIdentity);
      }
    }

    public int UserID
    {
      get { return _identity.UserID; }
    }

    public string Name
    {
      get { return _identity.Name; }
    }
    public string UserName
    {
      get { return _identity.UserName; }
    }

    public int LanguageID
    {
      get { return _identity.LanguageID; }
      set { _identity.LanguageID = value; }
    }

    public int? ConnectorID
    {
      get { return _identity.ConnectorID; }
      set { _identity.ConnectorID = value; }
    }

    public int OrganizationID
    {
      get { return _identity.OrganizationID; }      
    }

    public string Connector { get { return _identity.Connector; } }

    public int Timeout
    {
      get { return _identity.Timeout; }
    }

    public string Logo
    {
      get { return _identity.Logo; }
    }

    public Role Role
    {
      get { return _identity.Role; }
    }

    #region IPrincipal Members

    public IIdentity Identity
    {
      get { return _identity; }
    }

    public IEnumerable<string> Functionalities
    {
      get
      {
        return _identity.Functionalities;
      }
    }

    public HashSet<string> FunctionalityRoles
    {
      get
      {
        return _identity.FunctionalityRoles;
      }
    }

    public Role CurrentRole
    {
      get { return _identity.Role; }
    }

    public List<string> Roles
    {
      get
      {
        if (_roles == null)
        {
          _roles = (from ur in _kernel.Get<IRepository<User>>().GetSingle(c => c.UserID == this.UserID).UserRoles
                    select ur.Role.RoleName).Distinct().ToList();
        }
        return _roles;
      }
    }

    public bool IsInRole(string role)
    {
      return Roles.Contains(role);
    }

    public bool IsInFunction(string functionality)
    {
      return Functionalities.Contains(functionality);
    }

    public bool IsInRole(Role role)
    {
      return _identity.IsInRole(role);
    }

    #endregion

    public ConcentratorPrincipal(ConcentratorIdentity identity)
    {
      _identity = identity;
    }


    public static bool Login(string username, string password)
    {
      if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
        return false;

      ConcentratorIdentity identity = new ConcentratorIdentity(username, password);

      if (identity.IsAuthenticated)
      {
        ConcentratorPrincipal principal = new ConcentratorPrincipal(identity);
        Client.User = principal;
      }

      return identity.IsAuthenticated;
    }

    public int[] VendorIDs { get { return _identity.VendorIDs; } }
  }

  public static class ConcentratorPrincipalExtensions
  {
    public static bool HasFunctionality(this IConcentratorPrincipal principal, Functionalities functionalitiy)
    {
      return principal.Functionalities.Any(x => x == functionalitiy.ToString());
    }
  }
}
