using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace Concentrator.Objects.Web
{
  public interface IConcentratorIdentity : IIdentity
  {
    Int32 UserID
    {
      get;
    }

    String UserName
    {
      get;
    }
  }
}
