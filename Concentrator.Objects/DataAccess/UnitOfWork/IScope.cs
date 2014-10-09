using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.DataAccess.Repository;
using System.Data.Objects;

namespace Concentrator.Objects.DataAccess.UnitOfWork
{
  public interface IScope
  {
    /// <summary>
    /// Returns the specific instance of IRepository for this object
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <returns></returns>
    IRepository<TModel> Repository<TModel>() where TModel : class, new();    
  }
}