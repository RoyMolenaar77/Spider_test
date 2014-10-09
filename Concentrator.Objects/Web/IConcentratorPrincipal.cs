using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace Concentrator.Objects.Web
{
  using Model.Users;

  public interface IConcentratorPrincipal : IPrincipal
  {
    Int32? ConnectorID
    {
      get;
      set;
    }

    Int32 OrganizationID
    {
      get;
     
    }

    String Connector
    {
      get;
    }

    Role CurrentRole
    {
      get;
    }

    IEnumerable<String> Functionalities
    {
      get;
    }

    HashSet<String> FunctionalityRoles
    {
      get;
    }

    Int32 LanguageID
    {
      get;
      set;
    }

    String Logo
    {
      get;
    }

    String Name
    {
      get;
    }

    Role Role
    {
      get;
    }

    Int32 UserID
    {
      get;
    }

    String UserName
    {
      get;
    }

    Int32 Timeout
    {
      get;
    }

    Int32[] VendorIDs
    {
      get;
    }

    Boolean IsInFunction(String functionalityName);
  }
}
