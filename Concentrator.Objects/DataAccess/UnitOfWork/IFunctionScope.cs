using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.DataAccess.EntityFramework;

namespace Concentrator.Objects.DataAccess.UnitOfWork
{
  public interface IFunctionScope
  {
    /// <summary>
    /// Provides a repository against which contextual functions can be executed
    /// </summary>
    /// <returns>Instance of IFunctionRepository</returns>
    IFunctionRepository Repository();
  }
}
