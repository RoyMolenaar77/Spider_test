using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Concentrator.Objects.DataAccess.Repository
{
  public interface IRepository<TModel> where TModel : class
  {
    
    /// <summary>
    /// Gets all TModel objects satisfying a certain condition
    /// </summary>
    /// <param name="predicate">Predicate</param>
    /// <returns></returns>
    IQueryable<TModel> GetAll(Expression<Func<TModel, bool>> predicate = null);

    /// <summary>
    /// Retrieve all T objects as queryable
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    IQueryable<TModel> GetAllAsQueryable(Expression<Func<TModel, bool>> predicate = null);

    /// <summary>
    /// Returns a single entity satisfying the condition
    /// </summary>
    /// <param name="predicate">Predicate</param>
    /// <returns></returns>
    TModel GetSingle(Expression<Func<TModel, bool>> predicate);

    /// <summary>
    /// Gets the count of all items satisfying a certain condition
    /// </summary>
    /// <param name="predicate">Predicate</param>
    /// <returns></returns>
    int Count(Expression<Func<TModel, bool>> predicate = null);

    /// <summary>
    /// Adds an entity
    /// </summary>
    /// <param name="entity">The entity to add</param>
    void Add(TModel entity);

    /// <summary>
    /// Add multiple entities
    /// </summary>
    /// <param name="entities">Collection</param>
    void Add(IEnumerable<TModel> entities);

    /// <summary>
    /// Deletes an entity
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    void Delete(TModel entity);

    /// <summary>
    /// Delete multiple entities
    /// </summary>
    /// <param name="entities">The entity collection</param>
    void Delete(IEnumerable<TModel> entities);

    /// <summary>
    /// Deletes an entity(ies) satisfiying a certain condition
    /// </summary>
    /// <param name="predicate">Predicate</param>
    void Delete(Expression<Func<TModel, bool>> predicate);

    /// <summary>
    /// Includes relationships in the result set
    /// </summary>
    /// <param name="includes">Properties to be included</param>
    /// <returns></returns>
    IRepository<TModel> Include(params Expression<Func<TModel, object>>[] includes);

    /// <summary>
    /// Includes relationships in the result set
    /// </summary>
    /// <param name="paths">Paths to be included</param>
    /// <returns></returns>
    IRepository<TModel> Include(params string[] paths);
  }
}
