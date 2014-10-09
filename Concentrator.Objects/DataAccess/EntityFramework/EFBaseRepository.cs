using System;
using System.Collections.Generic;
using System.Linq;
using Concentrator.Objects.DataAccess.Repository;
using System.Data.Objects;
using System.Linq.Expressions;
using Concentrator.Objects.Models.Base;
using System.Configuration;

namespace Concentrator.Objects.DataAccess.EntityFramework
{
  public class EFBaseRepository<TModel> : IRepository<TModel>
    where TModel : BaseModel<TModel>, new()
  {
    private ObjectSet<TModel> _objectSet;
    protected ObjectSet<TModel> ObjectSet { get { return _objectSet; } }

    private ObjectContext _context;

    //private Expression<Func<TModel, bool>> _predicate;

    private ObjectQuery<TModel> _baseQuery;
    public ObjectQuery<TModel> BaseQuery { get { return _baseQuery ?? _objectSet; } }

    public EFBaseRepository(ObjectContext context)
      : this(context,true)
    {
      
    }

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="context">Object context</param>
    /// <param name="predicate">Base predicate applied to every query</param>
    public EFBaseRepository(ObjectContext context, bool filterOnVendor)
    {
      _context = context;

      _objectSet = context.CreateObjectSet<TModel>();

      _baseQuery = BaseQuery;

      if (filterOnVendor)
      {
        var _predicate = new TModel().GetFilter();
        if (_predicate != null)
        {
          _baseQuery = (ObjectQuery<TModel>)BaseQuery.Where(_predicate);
        }
      }
    }

    public IQueryable<TModel> GetAll(System.Linq.Expressions.Expression<Func<TModel, bool>> predicate = null)
    {
      if (predicate != null)
        return BaseQuery.Where(predicate).AsQueryable();
      else
        return BaseQuery.AsQueryable();
    }

    public IQueryable<TModel> GetAllAsQueryable(System.Linq.Expressions.Expression<Func<TModel, bool>> predicate = null)
    {
      var a = BaseQuery.AsQueryable();

      if (predicate != null) a = a.Where(predicate);

      return a;
    }

    public TModel GetSingle(System.Linq.Expressions.Expression<Func<TModel, bool>> predicate)
    {
      return BaseQuery.FirstOrDefault(predicate);
    }

    public int Count(System.Linq.Expressions.Expression<Func<TModel, bool>> predicate = null)
    {
      var q = BaseQuery.AsQueryable();
      if (predicate != null) q = q.Where(predicate);

      return q.Count();
    }

    public void Add(TModel entity)
    {
      ObjectSet.AddObject(entity);
    }

    public void Delete(TModel entity)
    {
      ObjectSet.DeleteObject(entity);
    }

    public void Delete(System.Linq.Expressions.Expression<Func<TModel, bool>> predicate)
    {
      IEnumerable<TModel> model = BaseQuery.Where(predicate).AsEnumerable();
      if (model != null)
      {
        Delete(model);
      }
    }

    public void Delete(IEnumerable<TModel> entities)
    {
      var delList = entities.ToList();

      delList.ForEach((model, id) =>
      {
        Delete(model);
      });


    }

    public void Add(IEnumerable<TModel> entities)
    {
      entities.ForEach((en, id) => { Add(en); });
    }

    public IRepository<TModel> Include(params Expression<Func<TModel, object>>[] includes)
    {
      foreach (var include in includes)
      {
        var body = include.Body as MemberExpression;

        if (body == null && body.Expression.NodeType != ExpressionType.Parameter)
        {
          throw new InvalidOperationException("Only MemberAccess is allowed");
        }

        _baseQuery = BaseQuery.Include(body.Member.Name);
      };

      return this;
    }

    public IRepository<TModel> Include(params string[] paths)
    {
      foreach (var path in paths)
      {
        _baseQuery = BaseQuery.Include(path);
      };

      return this;
    }
  }
}
