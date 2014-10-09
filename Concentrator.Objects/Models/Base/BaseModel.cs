using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Concentrator.Objects.Models.Base
{
  public abstract class BaseModel<TModel>
    where TModel : BaseModel<TModel>
  {

    /// <summary>
    /// Returns the function (lambda) on which every object coming from the storage will be filtered.
    /// If null than it will not filter on anything
    /// </summary>
    /// <returns></returns>
    public virtual Expression<Func<TModel, bool>> GetFilter() {
      return null;
    }
  }
}
