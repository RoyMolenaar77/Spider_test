using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Concentrator.Tasks
{
  using Objects.DataAccess;
  using Objects.DataAccess.Repository;
  using Objects.Models;
  using Objects.Models.Base;

  /// <summary>
  /// Represents a task-base with model as context.
  /// </summary>
  /// <typeparam name="TContext"></typeparam>
  public abstract class ContextTaskBase<TContext> : TaskBase
    where TContext : BaseModel<TContext>, new()
  {
    protected TContext Context
    {
      get;
      private set;
    }

    /// <summary>
    /// Is nullable, default returns true.
    /// </summary>
    protected virtual Expression<Func<TContext, Boolean>> ContextFilter
    {
      get;
      set;
    }

    protected virtual IRepository<TContext> ContextRepository
    {
      get
      {
        return Unit.Scope.Repository<TContext>();
      }
    }

    protected override void ExecuteTask()
    {
      var contextFilter = ContextFilter ?? (context => true);
      var contextRepository = Unit.Scope.Repository<TContext>();

      foreach (var context in contextRepository.GetAll(contextFilter).ToArray())
      {
        Context = context;

        if (ValidateContext())
        {
          ExecuteContextTask();
        }
      };
    }

    protected abstract void ExecuteContextTask();

    protected virtual Boolean ValidateContext()
    {
      return true;
    }
  }
}