using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Concentrator.Objects.DataAccess.Repository;
using Concentrator.Objects.DataAccess.UnitOfWork;

namespace Concentrator.Objects.Services.Base
{
  /// <summary>
  /// A generic service interface
  /// </summary>
  /// <typeparam name="TModel"></typeparam>
  public interface IService<TModel> where TModel : class, new()
  {
    void SetScope(IScope scope);

    /// <summary>
    /// Searches the storage for a specific query term
    /// </summary>
    /// <param name="queryTerm"></param>
    /// <returns></returns>
    IQueryable<TModel> Search(string queryTerm);

    /// <summary>
    /// Returns all entities from storage.
    /// </summary>
    /// <returns></returns>
    IQueryable<TModel> GetAll();

    /// <summary>
    /// Returns all entities from storage.
    /// </summary>
    /// <param name="predicate">Optional predicate</param>
    /// <returns></returns>
    IQueryable<TModel> GetAll(Expression<Func<TModel, bool>> predicate = null);

    /// <summary>
    /// Adds entity to storage
    /// </summary>
    /// <param name="model">Entity to insert</param>
    void Create(TModel model);


    /// <summary>
    /// Adds entities to storage 
    /// </summary>
    /// <param name="model"></param>
    void Create(IEnumerable<TModel> model);

    /// <summary>
    /// Deletes entity from storage
    /// </summary>
    /// <param name="model"></param>
    void Delete(TModel model);

    /// <summary>
    /// Deletes entities from storage
    /// </summary>
    /// <param name="model">Entities</param>
    void Delete(IEnumerable<TModel> model);

    /// <summary> 
    /// Deletes all entities satisfying the optional predicate
    /// </summary>
    /// <param name="predicate">Optional predicate</param>
    void Delete(Expression<Func<TModel, bool>> predicate = null);

    /// <summary>
    /// Returns a single entitity
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    TModel Get(Expression<Func<TModel, bool>> predicate);

    /// <summary>
    /// Update all entities matched by the predicate
    /// </summary>
    /// <param name="updater"></param>
    /// <param name="predicate"></param>
    void Update(Action<TModel> updater, Expression<Func<TModel, bool>> predicate = null);
  }
}
