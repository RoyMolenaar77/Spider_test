using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.Authentication.Basic;
using Nancy.Security;

namespace Concentrator.Web.API
{
  public class UserValidator : IUserValidator
  {
    public IUserIdentity Validate(string username, string password)
    {
      if (username == "demo" && password == "demo")
      {
        return new UserIdentity { UserName = username };
      }

      return null;
    }
  }
}