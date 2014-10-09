using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Nancy.Security;

namespace Concentrator.Web.API
{
  public class UserIdentity : IUserIdentity
  {
    public IEnumerable<String> Claims
    {
      get;
      set;
    }

    public string UserName
    {
      get;
      set;
    }
  }
}