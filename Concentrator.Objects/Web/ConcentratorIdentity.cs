using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Data.Linq;
using Concentrator.Objects.Models.Users;
using Ninject;
using Concentrator.Objects.DataAccess.Repository;
using Microsoft.Practices.ServiceLocation;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Services.Base;
using System.Linq.Expressions;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Model.Users;

namespace Concentrator.Objects.Web
{
  [Serializable]
  public class ConcentratorIdentity : IConcentratorIdentity
  {
    public static ConcentratorIdentity SystemIdentity
    {
      get
      {
        return new ConcentratorIdentity("system");
      }
    }

    private string _name = "Anonymous";
    private bool _isAuthenticated = false;

    public string UserName { get; private set; }
    public int UserID { get; private set; }
    public int? ConnectorID { get; set; }
    public string Logo { get; private set; }
    public int Timeout { get; set; }

    public IEnumerable<String> Functionalities
    {
      get;
      private set;
    }

    public HashSet<string> FunctionalityRoles
    {
      get;
      private set;
    }
    private Role _role;
    public int LanguageID;

    #region IIdentity Members

    public string AuthenticationType
    {
      get { return "Concentrator Authentication"; }
    }

    public bool IsAuthenticated
    {
      get { return _isAuthenticated; }
    }

    public string Name
    {
      get { return _name; }
    }

    #endregion

    private ConcentratorIdentity()
    {
    }

    public ConcentratorIdentity(UserIdentityModel userIdentity)
    {
      InitUser(userIdentity);
    }

    public ConcentratorIdentity(string username)
    {
      InitUser(GetUser(username));
    }

    public bool IsInRole(Role role)
    {
      return _role.Equals(role);
    }

    private UserIdentityModel GetUser(string username, string password = null)
    {
      using (var unit = ServiceLocator.Current.GetInstance<IServiceUnitOfWork>())
      {
        return (((IUserService)unit.Service<User>()).GetIdentityModel(username, password));
      }
    }

    internal ConcentratorIdentity(string username, string password)
    {
      InitUser(GetUser(username, password));
    }

    private void InitUser(UserIdentityModel user)
    {
      if (user != null)
      {
        UserID = user.UserID;
        UserName = user.UserName;
        Logo = user.Logo;
        Timeout = user.Timeout;
        _name = user.Name;
        _isAuthenticated = true;
        LanguageID = user.LanguageID;
        _role = user.Role;
        OrganizationID = user.OrganizationID;
        ConnectorID = user.ConnectorID;
        Connector = user.Connector;
        Functionalities = user.Functionalities;
        FunctionalityRoles = new HashSet<string>(Functionalities ?? Enumerable.Empty<String>());
        VendorIDs = user.VendorIDs;
      }
    }

    public string Connector { get; private set; }

    public Role Role
    {
      get { return _role; }
    }

    public int[] VendorIDs { get; private set; }

    public int OrganizationID { get; private set; }
  }
}
