using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.DataAccess.Repository;
using System.Linq.Expressions;
using Microsoft.Practices.ServiceLocation;
using ThisMember.Core.Interfaces;
using ThisMember.Core;

namespace Concentrator.Objects.Services.Base
{
  public class Service<TModel> : IService<TModel>
    where TModel : class, new()
  {
    /// <summary>
    /// The awesome mapper. For questions -> Julian
    /// </summary>
    private IMemberMapper mapper = new MemberMapper();

    protected IMemberMapper Mapper
    {
      get { return mapper; }
    }

    protected void TryMap<TMappableTo, TMappableFrom>(TMappableTo instanceToMapTo, TMappableFrom instanceToMapFrom)
      where TMappableTo : class, new()
      where TMappableFrom : class
    {
      mapper.Map(instanceToMapFrom, instanceToMapTo);
    }


    private IScope _scope;

    protected IScope Scope { get { return _scope; } }

    #region IService<TModel> Members

    public virtual IQueryable<TModel> GetAll(System.Linq.Expressions.Expression<Func<TModel, bool>> predicate = null)
    {
      return Repository().GetAllAsQueryable(predicate);
    }

    public virtual void Create(TModel model)
    {
      Repository().Add(model);
    }

    public void Create(IEnumerable<TModel> model)
    {
      model.ForEach((m, idx) => Create(m));
    }

    public virtual void Delete(TModel model)
    {
      Repository().Delete(model);
    }

    public void Delete(IEnumerable<TModel> model)
    {
      Repository().Delete(model);      
    }

    public void Delete(System.Linq.Expressions.Expression<Func<TModel, bool>> predicate = null)
    {
      Repository().GetAllAsQueryable(predicate).ForEach((m, idx) => { Delete(m); });
    }

    public void Update(Action<TModel> updater, Expression<Func<TModel, bool>> predicate = null)
    {
      Repository().GetAllAsQueryable(predicate).ForEach((m, idx) => updater(m));
    }

    public virtual TModel Get(Expression<Func<TModel, bool>> predicate)
    {
      return Repository().GetSingle(predicate);
    }

    public void SetScope(IScope scope)
    {
      _scope = scope;
    }

    /// <summary>
    /// Shortcut to the repository of the current model
    /// </summary>
    /// <returns></returns>
    protected virtual IRepository<TModel> Repository()
    {
      return _scope.Repository<TModel>();
    }

    /// <summary>
    /// Gets a repositor for a model T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected IRepository<T> Repository<T>()
      where T : class, new()
    {
      return _scope.Repository<T>();
    }

    public virtual IQueryable<TModel> Search(string queryTerm)
    {
      return GetAll().Search(queryTerm);
    }

    #endregion

    #region IService<TModel> Members


    public IQueryable<TModel> GetAll()
    {
      return Repository().GetAllAsQueryable();
    }

    #endregion
  }
}
